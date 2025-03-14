using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Linq;

public abstract class Event
{
    public async Task Dispatch(IJSRuntime js)
    {
        await js.InvokeVoidAsync("document.body.dispatchEvent",
            await js.InvokeAsync<IJSObjectReference>("eval", $"new CustomEvent('{GetType().Name}', {{ bubbles: true }})"));
    }
}

public class MyTurn : Event { }

class EventService
{
    private static readonly ConcurrentQueue<Event> _events = new();
    private static readonly SemaphoreSlim _semaphore = new(0);

    public static void Initialize(IJSRuntime js)
    {
        _ = Task.Run(async () =>
       {
           while (true)
           {
               await _semaphore.WaitAsync();
               _events.TryDequeue(out var e);
               if (e != null)
               {
                   _ = e.Dispatch(js);
               }
           }
       });
    }

    public static void EnqueueEvent(Event e)
    {
        _events.Enqueue(e);
        _semaphore.Release();
    }

}
