using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automata.Kinds;

namespace Automata.Client
{
    public class KindGraph
    {
        private readonly Dictionary<KindName, KindModel> _kindsByName;

        public KindGraph(List<KindModel> kinds)
        {
            _kindsByName = kinds.ToDictionary(q => q.Name);
        }
        
        public bool TryGetKind(KindName kindName, [NotNullWhen(true)] out KindModel? kind)
        {
            return _kindsByName.TryGetValue(kindName, out kind);
        }
    }
}