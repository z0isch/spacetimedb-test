using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WebClient;

public record ChatMessage(string? Sender, string Text);

public class ServerApi
{
	private Identity? local_identity;
	private readonly ConcurrentQueue<(string Command, string Args)> input_queue = new();
	private Timer? _timer;
	private Action? _stateHasChanged;
	public Dictionary<Identity, PlayerState> PlayerStates = new();
	public DbConnection? DbConnection { get; private set; }

	public void Run(string? authToken, Action<string> setAuthToken, Action stateHasChanged)
	{
		_stateHasChanged = stateHasChanged;
		DbConnection = ConnectToDB(authToken, setAuthToken);
		RegisterCallbacks(DbConnection);
		_timer = new Timer(_ => { ProcessThread(DbConnection); }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(8));
	}

	private const string HOST = "http://localhost:3000";
	private const string DBNAME = "quickstart-chat";

	private DbConnection ConnectToDB(string? authToken, Action<string> setAuthToken)
	{
		var conn = DbConnection.Builder()
							   .WithCompression(Compression.Gzip)
							   .WithUri(HOST)
							   .WithModuleName(DBNAME)
							   .WithToken(authToken)
							   .OnConnect((conn, identity, authToken) =>
							   {
								   setAuthToken(authToken);
								   local_identity = identity;

								   conn.SubscriptionBuilder()
									   .OnApplied(OnSubscriptionApplied)
									   .SubscribeToAllTables();
							   })
							   .OnConnectError((Exception e) =>
							   {
								   Console.WriteLine($"Error while connecting: {e}");
							   })
							   .OnDisconnect((conn, e) =>
							   {
								   Console.WriteLine(e != null
								   ? $"Disconnected abnormally: {e}"
								   : "Disconnected normally.");
							   })
							   .Build();
		return conn;
	}

	private void RegisterCallbacks(DbConnection conn)
	{
		conn.Db.PlayerState.OnInsert += PlayerState_OnInsert;
		conn.Db.PlayerState.OnUpdate += PlayerState_OnUpdate;

		conn.Reducers.OnSetTyping += Reducer_OnSetTypingEvent;
	}

	private void PlayerState_OnInsert(EventContext ctx, PlayerState insertedValue)
	{
		PlayerStates[insertedValue.Identity] = insertedValue;
		_stateHasChanged?.Invoke();
	}

	private void PlayerState_OnUpdate(EventContext ctx, PlayerState _oldValue, PlayerState newValue)
	{
		PlayerStates[newValue.Identity] = newValue;
		_stateHasChanged?.Invoke();
	}


	private void Reducer_OnSetTypingEvent(ReducerEventContext ctx, string text)
	{
		var e = ctx.Event;
		if (e.CallerIdentity == local_identity && e.Status is Status.Failed(var error))
		{
			Console.WriteLine($"Failed to change typing to {text}: {error}");
		}
	}

	private void OnSubscriptionApplied(SubscriptionEventContext ctx)
	{
		foreach (var playerState in ctx.Db.PlayerState.Iter())
		{
			PlayerStates[playerState.Identity] = playerState;
		}
	}

	private void ProcessThread(DbConnection conn)
	{
		try
		{
			conn.FrameTick();
			ProcessCommands(conn.Reducers);
		}
		catch
		{
			conn.Disconnect();
			_timer?.Dispose();
			_timer = null;
			throw;
		}
	}

	private void ProcessCommands(RemoteReducers reducers)
	{
		while (input_queue.TryDequeue(out var command))
		{
			switch (command.Command)
			{
				case "typing":
					reducers.SetTyping(command.Args);
					break;
			}
		}
	}

	public void SetTyping(string text)
	{
		input_queue.Enqueue(("typing", text));
	}

}