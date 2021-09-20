using Automata.Kinds;

namespace Automata.HostServer.Kinds
{
    internal class InstalledKind
    {
        public InstalledKind(KindModel kindModel)
        {
            KindModel = kindModel;
        }

        public KindModel KindModel { get; }
    }
}