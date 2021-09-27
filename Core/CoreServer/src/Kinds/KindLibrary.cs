using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Automata.HostServer.Infrastructure;
using Automata.Kinds;

namespace Automata.HostServer.Kinds
{
    internal class KindLibrary : IResourceProvider
    {
        private readonly IReadOnlyList<KindModel> _installedKinds;

        public KindLibrary(IEnumerable<InstalledKind> installedKinds)
        {
            _installedKinds = installedKinds
                .GroupBy(q => q.KindModel.Name)
                .Select(q => q.First().KindModel)
                .ToList();
        }

        public IAsyncEnumerable<ResourceDocument> GetResources()
        {
            return Impl();
            
            async IAsyncEnumerable<ResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                //  base kind
                yield return KindModel.GetKind(typeof(Record)).AsResource();

                //  kind
                yield return KindModel.GetKind(typeof(KindRecord)).AsResource();

                foreach (var kind in _installedKinds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return kind.AsResource();
                }
            }
        }
    }
}