using System;

namespace Automata.Devices
{
    public class UnableToCompleteOperationException : Exception
    {
        public ResourceDocument Device { get; }
        public DeviceControlRequest Request { get; }

        public UnableToCompleteOperationException(string message,
            ResourceDocument device, DeviceControlRequest request) :
            base(message)
        {
            Device = device;
            Request = request;
        }
    }
}