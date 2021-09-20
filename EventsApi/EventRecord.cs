using Automata.Kinds;

namespace Automata.Events
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "event", "events")]
    public abstract record EventRecord : Record;
}