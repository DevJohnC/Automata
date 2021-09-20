using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Automata.HostServer.Resources;
using Newtonsoft.Json;

namespace Automata
{
    public static class IResourceIdPersistenceExtensions
    {
        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Objects
        };

        private static readonly HashAlgorithm _hashAlgorithm = new MD5CryptoServiceProvider();
        
        private static string ConvertToResourceKey(Record record)
        {
            var json = JsonConvert.SerializeObject(record, _serializerSettings);
            var hash = _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(hash);
        }
        
        public static Guid GetOrCreateResourceId(
            this IResourceIdPersistence service,
            Record record)
        {
            return service.GetOrCreateResourceId(record.GetKind().Name, ConvertToResourceKey(record));
        }

        public static Task<Guid> GetOrCreateResourceIdAsync(
            this IResourceIdPersistence service,
            Record record)
        {
            return service.GetOrCreateResourceIdAsync(record.GetKind().Name, ConvertToResourceKey(record));
        }
    }
}