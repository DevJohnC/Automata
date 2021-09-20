using Automata.Kinds;

namespace Automata.GrpcServices
{
    public partial class PluralKindUri
    {
        public static PluralKindUri FromKindName(KindName kindName)
        {
            return new PluralKindUri()
            {
                Group = kindName.Group,
                Version = kindName.Version,
                KindNamePlural = kindName.NamePlural
            };
        }
    }
}