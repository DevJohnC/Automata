using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Automata.Kinds
{
    internal static class TypeKinds
    {
        private static readonly Dictionary<Type, KindModel> TypeKindCache = new();

        private static Type GetMostSpecificType(List<Type> types)
        {
            //  todo: optimize this!!
            var sortedList = new List<Type>();
            var leastSpecificType = types.First(q => !types.Contains(q.BaseType));
            sortedList.Add(leastSpecificType);
            for (var i = 1; i < types.Count; i++)
            {
                sortedList.Add(types.First(q => q.BaseType == sortedList.Last()));
            }

            return sortedList.Last();
        }
        
        public static KindModel GetKind(Type type)
        {
            if (TypeKindCache.TryGetValue(type, out var kind))
            {
                return kind;
            }

            lock (TypeKindCache)
            {
                if (TypeKindCache.TryGetValue(type, out kind))
                {
                    return kind;
                }
                
                kind = KindFactory.DeriveFromType(type);
                TypeKindCache.Add(type, kind);
                return kind;
            }
        }
    }
}