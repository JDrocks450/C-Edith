using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code.Definitions
{
    /// <summary>
    /// Represents a Type in C# -- defined in a CodeUnit
    /// </summary>
    public sealed class TypeDefinition : CodeObjectDefinition
    {
        public override CSharpSpecialTokens SpecialToken => CSharpSpecialTokens.TYPE;
        public enum ClassTypes
        {
            Class,
            Struct,
            Interface,
            Enum
        }
        public ClassTypes ClassType { get; }
        public string TypeName { get; }

        public TypeDefinition(ClassTypes classType, string Name) : base(classType.ToString(), Name)
        {
            ClassType = classType;
            Description = TypeName = Name;
        }

        public override string ToString()
        {
            return Title + " " + TypeName;
        }
    }
}
