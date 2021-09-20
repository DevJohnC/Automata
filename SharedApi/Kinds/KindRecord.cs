namespace Automata.Kinds
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "kind", "kinds")]
    public record KindRecord(KindName Name, string? ParentKind,
        KindSchema Schema) : Record;
}