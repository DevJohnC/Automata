using System.Collections.Generic;
using System.Linq;
using Automata.Kinds;

namespace Automata.Client
{
    public record KindGraphBuilder(List<ResourceDocument<KindRecord>> KindDefinitions)
    {
        public KindGraphBuilder DefineKind(ResourceDocument<KindRecord> kind)
        {
            var kindsCopy = new List<ResourceDocument<KindRecord>>(KindDefinitions.Count + 1);
            kindsCopy.AddRange(KindDefinitions);
            kindsCopy.Add(kind);
            return this with
            {
                KindDefinitions = kindsCopy
            };
        }

        public KindGraph Build()
        {
            var remoteKinds = BuildKinds(KindDefinitions);
            return new KindGraph(remoteKinds);
        }

        private static List<KindModel> BuildKinds(
            List<ResourceDocument<KindRecord>> kindDefinitions)
        {
            var remoteKinds = new List<KindModel>();
            foreach (var kindRecord in kindDefinitions)
            {
                FindOrCreateKind(kindRecord);
            }
            return remoteKinds;

            KindModel? FindOrCreateKindByUri(string? kindUriString)
            {
                if (string.IsNullOrWhiteSpace(kindUriString))
                    return default;

                var uriSegments = kindUriString.Split('/');
                var kindUri = KindUri.Singular(uriSegments[0], uriSegments[1], uriSegments[2]);

                var kind = remoteKinds.FirstOrDefault(q => q.Name.MatchesUri(kindUri));
                if (kind != null)
                {
                    return kind;
                }

                var remoteSpec = kindDefinitions
                    .First(q => q.Record.Name.MatchesUri(kindUri));
                return FindOrCreateKind(remoteSpec);
            }
            
            KindModel FindOrCreateKind(ResourceDocument<KindRecord> remoteKind)
            {
                var kind = remoteKinds.FirstOrDefault(q => q.Name == remoteKind.Record.Name);
                if (kind != null)
                {
                    return kind;
                }

                kind = new UntypedKindModel(remoteKind.Record.Name,
                    FindOrCreateKindByUri(remoteKind.Record.ParentKind),
                    remoteKind.Record.Schema);
                remoteKinds.Add(kind);
                return kind;
            }
        }
    }
}