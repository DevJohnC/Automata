using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Automata.Client;
using Automata.Client.Networking;
using Automata.Client.Resources;
using Automata.Kinds;

namespace Automata.Devices
{
    public class DeviceMonitor<TDevice> : IDisposable
        where TDevice : DeviceDefinition
    {
        private static readonly KindModel DeviceKind = KindModel.GetKind(typeof(TDevice));

        private static readonly KindModel StateKind = KindModel.GetKind(typeof(DeviceState));
        
        private readonly DeviceCollection _devices;
        
        private readonly IResourceCollectionMonitor _monitor;

        /// <summary>
        /// Raised whenever a device is added, removed or state added, removed or changed.
        /// </summary>
        public event EventHandler? Changed;

        private DeviceMonitor(IResourceCollectionMonitor monitor)
        {
            _monitor = monitor;
            _devices = new DeviceCollection(_monitor);
            _devices.CollectionChanged += (_, _) =>
            {
                Changed?.Invoke(this, EventArgs.Empty);
            };
        }

        public DeviceMonitor(AutomataNetwork source) :
            this(new NetworkResourceCollectionMonitor(source, DeviceKind.Name, StateKind.Name))
        {
        }

        public DeviceMonitor(IAutomataServer source) :
            this(new ServerResourceCollectionMonitor(source, DeviceKind.Name, StateKind.Name))
        {
        }

        public IReadOnlyList<Device<TDevice>> Devices => _devices;

        public void Dispose()
        {
            _devices.Dispose();
            _monitor.Dispose();
        }

        private class DeviceCollection : IReadOnlyList<Device<TDevice>>, INotifyCollectionChanged, IDisposable
        {
            private readonly ReadOnlyObservableCollection<SerializedResourceGraph>
                _baseCollection;

            public DeviceCollection(IResourceCollectionMonitor monitor)
            {
                _baseCollection = monitor.Resources;
                HookEvents(_baseCollection);
            }

            private void HookEvents(INotifyCollectionChanged target)
            {
                target.CollectionChanged += BaseCollectionChanged;
            }

            private void UnhookEvents(INotifyCollectionChanged target)
            {
                target.CollectionChanged -= BaseCollectionChanged;
            }

            private void BaseCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, ConvertChangedArgs(e));
            }

            private NotifyCollectionChangedEventArgs ConvertChangedArgs(NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        return new NotifyCollectionChangedEventArgs(e.Action);
                    case NotifyCollectionChangedAction.Replace:
                        return new NotifyCollectionChangedEventArgs(e.Action,
                            ConvertList(e.NewItems!), ConvertList(e.OldItems!), e.NewStartingIndex);
                    case NotifyCollectionChangedAction.Move:
                        return new NotifyCollectionChangedEventArgs(e.Action,
                            ConvertList(e.NewItems!), e.NewStartingIndex, e.OldStartingIndex);
                    case NotifyCollectionChangedAction.Add:
                        return new NotifyCollectionChangedEventArgs(e.Action,
                            ConvertList(e.NewItems!), e.NewStartingIndex);
                    case NotifyCollectionChangedAction.Remove:
                        return new NotifyCollectionChangedEventArgs(e.Action,
                            ConvertList(e.OldItems!), e.OldStartingIndex);
                }

                throw new NotSupportedException();
            }

            private IList ConvertList(IList list)
            {
                //  todo: consider something more performant than LINQ?
                return list
                    .OfType<SerializedResourceGraph>()
                    .Select(q => Convert(q))
                    .ToList();
            }

            private Device<TDevice> Convert(SerializedResourceGraph resource)
            {
                return new Device<TDevice>(
                    resource.Resource.ResourceId,
                    resource.Resource.Deserialize<TDevice>().Record,
                    resource.AssociatedResources);
            }
            
            public IEnumerator<Device<TDevice>> GetEnumerator()
            {
                foreach (var resource in _baseCollection)
                {
                    yield return Convert(resource);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _baseCollection.Count;
            
            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public Device<TDevice> this[int index] => Convert(_baseCollection[index]);
            
            public void Dispose()
            {
                UnhookEvents(_baseCollection);
            }
        }
    }
}