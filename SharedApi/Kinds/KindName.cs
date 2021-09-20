using System;

namespace Automata.Kinds
{
    public enum KindNameFormat
    {
        Singular,
        Plural
    }
    
    public record KindName(
        string Group,
        string Version,
        string Name,
        string NamePlural)
    {
        public bool MatchesUri(KindUri kindUri)
        {
            if (!string.Equals(kindUri.Group, Group, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            if (!string.Equals(kindUri.Version, Version, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (kindUri.IsPlural &&
                !string.Equals(kindUri.Name, NamePlural, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            if (!kindUri.IsPlural &&
                !string.Equals(kindUri.Name, Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            return true;
        }

        public SingularKindUri SingularUri => KindUri.Singular(Group, Version, Name);

        public PluralKindUri PluralUri => KindUri.Plural(Group, Version, NamePlural);

        public override string ToString()
        {
            return ToString(KindNameFormat.Singular);
        }

        public string ToString(KindNameFormat format)
        {
            if (format == KindNameFormat.Plural)
                return $"{Group}/{Version}/{NamePlural}";
            return $"{Group}/{Version}/{Name}";
        }
    }
}