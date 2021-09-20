using System;
using System.Linq;

namespace Automata.Kinds
{
    internal static class KindFactory
    {
        private static readonly Type ResourceType = typeof(Record);
        
        public static KindModel DeriveFromType(Type type)
        {
            if (!ResourceType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException("Type must extend `RecordData` to be a Kind.");
            }

            var kindIdentifierAttr = type.GetCustomAttributes(typeof(KindAttribute), true)
                .First() as KindAttribute;

            if (kindIdentifierAttr == null)
            {
                throw new InvalidOperationException("Type doesn't describe a kind.");
            }

            return new TypeKindModel(
                new(
                    kindIdentifierAttr.Group,
                    kindIdentifierAttr.Version,
                    kindIdentifierAttr.KindName,
                    kindIdentifierAttr.KindNamePlural),
                GetParentKind(type, new(
                    kindIdentifierAttr.Group,
                    kindIdentifierAttr.Version,
                    kindIdentifierAttr.KindName,
                    kindIdentifierAttr.KindNamePlural)),
                type);
        }

        private static KindModel? GetParentKind(Type type, KindName typeKindName)
        {
            KindModel? parentKind = default;
            if (type.BaseType != null &&
                ResourceType.IsAssignableFrom(type.BaseType))
            {
                parentKind = TypeKinds.GetKind(type.BaseType);
            }

            return ReduceParentKind(parentKind, typeKindName);
        }

        private static KindModel? ReduceParentKind(KindModel? parentKind, KindName typeKindName)
        {
            if (parentKind == null)
                return null;

            while (parentKind != null &&
                   typeKindName == parentKind.Name)
            {
                parentKind = parentKind.ParentKind;
            }

            return parentKind;
        }

        private class TypeKindModel : KindModel
        {
            public TypeKindModel(KindName name, KindModel? parentKind,
                Type type) :
                base(name, parentKind, KindSchema.CreateFromType(type))
            {
            }
        }
    }
}