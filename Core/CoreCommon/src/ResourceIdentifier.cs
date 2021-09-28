using System;
using Automata.Kinds;

namespace Automata
{
    public record ResourceIdentifier(Guid ResourceId, SingularKindUri KindUri);
}