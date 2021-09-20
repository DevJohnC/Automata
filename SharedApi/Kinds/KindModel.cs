using System;
using System.Security.Cryptography;
using System.Text;

namespace Automata.Kinds
{
    public abstract class KindModel
    {
        private ResourceDocument<KindRecord>? _asResource;
        
        public KindName Name { get; }
        
        public KindModel? ParentKind { get; }
        
        public KindSchema KindSchema { get; }

        public KindModel(KindName name, KindModel? parentKind,
            KindSchema kindSchema)
        {
            Name = name;
            ParentKind = parentKind;
            KindSchema = kindSchema;
        }

        public ResourceDocument<KindRecord> AsResource()
        {
            if (_asResource == null)
            {
                _asResource = new ResourceDocument<KindRecord>(
                    GetKindId(Name),
                    GetResourceRecord());
            }

            return _asResource;
        }

        public KindRecord GetResourceRecord()
        {
            return new KindRecord(
                Name,
                ParentKind?.Name?.ToString(KindNameFormat.Singular),
                KindSchema);
        }
        
        private static readonly SHA1 HashAlgorithm = SHA1.Create();
        
        public static KindModel GetKind(Type type)
        {
            return TypeKinds.GetKind(type);
        }

        private static Guid GetKindId(KindName name)
        {
            //  todo: consider if the full 160bits need to be encoded into the resulting guid, we might be losing uniqueness and increasing changes over collisions
            //  todo: consider a larger hashing algorithm
            var srcData = Encoding.UTF8.GetBytes(
                $"{name.ToString(KindNameFormat.Singular)}\n{name.ToString(KindNameFormat.Plural)}"
            );
            var hash = HashAlgorithm.ComputeHash(srcData);
            return new Guid(hash.AsSpan().Slice(0, 16));
        }
    }
}