using Automata.Kinds;

namespace Automata.Client
{
    internal class UntypedKindModel : KindModel
    {
        public UntypedKindModel(KindName name, KindModel? parentKind, Kinds.KindSchema kindSchema) :
            base(name, parentKind, kindSchema)
        {
        }
    }
}