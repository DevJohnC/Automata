using System;
using System.Collections.ObjectModel;
using Automata.Client.Resources;

namespace Automata.Client
{
    public interface IResourceCollectionMonitor : IDisposable
    {
        ReadOnlyObservableCollection<SerializedResourceGraph> Resources { get; }
    }
}