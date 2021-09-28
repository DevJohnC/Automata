using System;
using System.Collections.Generic;
using Automata.Client;

namespace Automata.Devices
{
    public class DevicesWatcher<TDevice>
        where TDevice : DeviceDefinition
    {
        private readonly IResourceWatcher _resourceWatcher;

        /// <summary>
        /// Raised whenever a device is added, removed or state added, removed or changed.
        /// </summary>
        public event AsyncEventHandler<DevicesWatcher<TDevice>> Changed;

        public DevicesWatcher(IResourceWatcher resourceWatcher)
        {
            _resourceWatcher = resourceWatcher;
        }

        public IReadOnlyList<Device<TDevice>> Devices =>
            //  todo: implement
            throw new NotImplementedException();
    }
}