using System;

namespace Automata.Kinds
{
    /// <summary>
    /// Identifies a resource type as a <see cref="KindModel"/>.
    /// Must be a record type that extends <see cref="Record"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KindAttribute : Attribute
    {
        public KindAttribute(string @group, string version, string kindName, string kindNamePlural)
        {
            Group = @group;
            Version = version;
            KindName = kindName;
            KindNamePlural = kindNamePlural;
        }

        public string Group { get; }
        
        public string Version { get; }
        
        public string KindName { get; }
        
        public string KindNamePlural { get; }
    }
}