using System.Threading;
using System.Threading.Tasks;

namespace Automata.Events
{
    public interface IEventObserver
    {
        public Task Next(ResourceDocument eventRecord, CancellationToken cancellationToken);
    }
}