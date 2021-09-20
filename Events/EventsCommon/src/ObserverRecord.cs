using Automata.Kinds;

namespace Automata.Events
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "observer", "observers")]
    public sealed record ObserverRecord : Record;
}