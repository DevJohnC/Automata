using Automata.Kinds;

namespace Automata
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "resource", "resources")]
    public abstract record Record
    {
        public virtual KindModel GetKind()
        {
            return KindModel.GetKind(GetType());
        }

        public bool IsOfKind(KindUri kindUri)
        {
            KindModel? kind = GetKind();
            while (kind != null)
            {
                if (kind.Name.MatchesUri(kindUri))
                    return true;

                kind = kind.ParentKind;
            }

            return false;
        }
    }
}