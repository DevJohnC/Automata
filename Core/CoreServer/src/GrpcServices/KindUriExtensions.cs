using System;

namespace Automata.GrpcServices
{
    public partial class SingularKindUri
    {
        private Automata.Kinds.SingularKindUri? _kindUri;
        public Automata.Kinds.SingularKindUri NativeKindUri
        {
            get
            {
                if (_kindUri == null)
                    _kindUri = BuildKindUri();
                return _kindUri;
            }
        }
        
        private Automata.Kinds.SingularKindUri BuildKindUri()
        {
            return Kinds.KindUri.Singular(Group, Version, KindNameSingular);
        }
    }
    
    public partial class KindUri
    {
        private Automata.Kinds.KindUri? _kindUri;
        public Automata.Kinds.KindUri NativeKindUri
        {
            get
            {
                if (_kindUri == null)
                    _kindUri = BuildKindUri();
                return _kindUri;
            }
        }

        private Automata.Kinds.KindUri BuildKindUri()
        {
            return TestUriCase switch
            {
                TestUriOneofCase.PluralUri => Kinds.KindUri.Plural(PluralUri.Group, PluralUri.Version,
                    PluralUri.KindNamePlural),
                TestUriOneofCase.SingularUri => Kinds.KindUri.Singular(SingularUri.Group, SingularUri.Version,
                    SingularUri.KindNameSingular),
                _ => throw new InvalidOperationException()
            };
        }
    }
}