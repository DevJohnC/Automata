using System;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Client.Services;

namespace Automata.Devices
{
    public interface IStateClient : IAutomataNetworkService
    {
        Task RequestStateChange(
            Guid controllerId,
            Guid deviceId,
            ResourceDocument deviceControlRequest,
            CancellationToken cancellationToken = default);
    }
}