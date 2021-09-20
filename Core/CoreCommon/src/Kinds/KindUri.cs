namespace Automata.Kinds
{
    public record KindUri(string Group, string Version, string Name, bool IsPlural)
    {
        public static SingularKindUri Singular(string group, string version, string kindNameSingular)
        {
            return new SingularKindUri(group, version, kindNameSingular);
        }
        
        public static PluralKindUri Plural(string group, string version, string kindNamePlural)
        {
            return new PluralKindUri(group, version, kindNamePlural);
        }
    };

    public record SingularKindUri(string Group, string Version, string Name) :
        KindUri(Group, Version, Name, false);
    
    public record PluralKindUri(string Group, string Version, string Name) :
        KindUri(Group, Version, Name, true);
}