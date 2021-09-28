using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.HostServer.Infrastructure;

namespace Automata.Devices
{
    public class DeviceControllerManager : IResourceProvider
    {
        private readonly DeviceManager _deviceManager;
        private readonly DeviceControllerMetadataProvider _deviceControllerMetadataProvider;
        private readonly List<IDeviceController> _deviceControllers;
        private readonly ReaderWriterLockSlim _lock = new();
        
        private List<DeviceControllerMetadata>? _controllerMetadata;

        public DeviceControllerManager(
            DeviceManager deviceManager,
            DeviceControllerMetadataProvider deviceControllerMetadataProvider,
            IEnumerable<IDeviceController> deviceControllers)
        {
            _deviceManager = deviceManager;
            _deviceControllerMetadataProvider = deviceControllerMetadataProvider;
            _deviceControllers = deviceControllers
                .ToList();
        }

        [MemberNotNull(nameof(_controllerMetadata))]
        private async Task EnsureMetadataLoaded()
        {
            using var readLock = _lock.UseUpgradableReadLock();
            if (_controllerMetadata != null)
                return;

            using var writeLock = _lock.UseWriteLock();

            _controllerMetadata = new List<DeviceControllerMetadata>(_deviceControllers.Count);
            foreach (var controller in _deviceControllers)
            {
                _controllerMetadata.Add(
                    await _deviceControllerMetadataProvider.GetMetadata(controller));
            }
        }

        public async Task RequestDeviceStateChange(
            Guid controllerId,
            Guid deviceId,
            SerializedResourceDocument serializedResourceDocument)
        {
            await EnsureMetadataLoaded();
            //  todo: perform an indexed lookup using a Dictionary
            var controllerMetadata = _controllerMetadata.FirstOrDefault(
                q => q.ControllerId == controllerId);
            if (controllerMetadata == null)
                //  todo: return a controller-not-found response
                return;

            if (!controllerMetadata.TryGetMessageInvoker(
                serializedResourceDocument.KindUri,
                out var messageInvoker))
                //  todo: return a message not supported response
                return;

            //  todo: return result from controller
            await messageInvoker.Invoke(
                _deviceManager,
                deviceId,
                serializedResourceDocument);
        }
        
        IAsyncEnumerable<ResourceDocument> IResourceProvider.GetResources()
        {
            return Impl();
            
            async IAsyncEnumerable<ResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken ct = default)
            {
                await EnsureMetadataLoaded();
                foreach (var deviceController in _controllerMetadata)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return await deviceController.GetResourceDocument(_deviceManager);

                    foreach (var messageInvoker in deviceController.MessageInvokers)
                    {
                        ct.ThrowIfCancellationRequested();
                        yield return messageInvoker.RequestKind.AsResource();
                    }
                }
            }
        }

        public async IAsyncEnumerable<ResourceDocument> GetAssociatedResources(ResourceIdentifier resourceIdentifier)
        {
            yield break;
        }
    }
}