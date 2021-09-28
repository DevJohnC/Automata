using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata
{
    public delegate Task AsyncEventHandler<TSender>(
        TSender sender,
        CancellationToken cancellationToken)
        where TSender : notnull;
    
    public delegate Task AsyncEventHandler<TSender, TEventArgs>(
        TSender sender,
        TEventArgs e,
        CancellationToken cancellationToken)
        where TSender : notnull;

    public static class AsyncEventExtensions
    {
        public static async Task ParallelInvoke<TSender, TEventArgs>(
            this AsyncEventHandler<TSender, TEventArgs> eventHandler,
            TSender sender,
            TEventArgs args,
            CancellationToken cancellationToken = default
        )
            where TSender : notnull
        {
            var delegates = eventHandler.GetInvocationList();
            var tasks = new List<Task>(delegates.Length);
            foreach (var @delegate in delegates)
            {
                var @event = (AsyncEventHandler<TSender, TEventArgs>)@delegate;
                tasks.Add(@eventHandler.Invoke(sender, args, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
        
        public static async Task SerialInvoke<TSender, TEventArgs>(
            this AsyncEventHandler<TSender, TEventArgs> eventHandler,
            TSender sender,
            TEventArgs args,
            CancellationToken cancellationToken = default
        )
            where TSender : notnull
        {
            foreach (var @delegate in eventHandler.GetInvocationList())
            {
                var @event = (AsyncEventHandler<TSender, TEventArgs>)@delegate;
                await @event(sender, args, cancellationToken);
            }
        }
    }
}