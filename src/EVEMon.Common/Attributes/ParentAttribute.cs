using System;
using System.Linq;

namespace EVEMon.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ParentAttribute : Attribute
    {
        public ParentAttribute(params object[] parents)
        {
            Parents = parents?.Where(x => x as Enum != null).Select(x => (Enum)x).ToArray();
        }

        public Enum[] Parents { get; }
    }
}
