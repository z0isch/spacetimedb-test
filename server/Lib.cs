using SpacetimeDB;

public static partial class Module
{
	[Table(Name = "PlayerState", Public = true)]
	public partial class PlayerState
	{
		[PrimaryKey]
		public Identity Identity;
		public string? Typing;
	}

	[Reducer]
	public static void SetTyping(ReducerContext ctx, string text)
	{
		ctx.Db.PlayerState.Identity.Update(
			new PlayerState
			{
				Identity = ctx.Sender,
				Typing = text
			}
		);
	}

	[Reducer(ReducerKind.ClientConnected)]
	public static void ClientConnected(ReducerContext ctx)
	{
		Log.Info($"Connect {ctx.Sender}");
		var playerState = ctx.Db.PlayerState.Identity.Find(ctx.Sender);

		if (playerState is null)
		{
			ctx.Db.PlayerState.Insert(
				new PlayerState
				{
					Identity = ctx.Sender,
					Typing = null,
				}
			);
		}
	}

}
