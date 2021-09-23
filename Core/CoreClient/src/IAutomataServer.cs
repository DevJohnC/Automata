﻿using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    public interface IAutomataServer
    {
        KindGraph? KindGraph { get; }
        
        [MemberNotNull(nameof(KindGraph))]
        Task RefreshKindGraph(CancellationToken cancellationToken = default);

        bool SupportsService<TService>()
            where TService : IAutomataNetworkService;

        TService CreateService<TService>()
            where TService : IAutomataNetworkService;
    }
}