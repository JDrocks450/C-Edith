using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code.Definitions
{
    /// <summary>
    /// Represents a namespace, please refer to <see cref="CodeUnit"/> for getting types defined in this namespace
    /// </summary>
    public sealed class NamespaceContainerDefinition : CodeObjectDefinition
    {
        public override CSharpSpecialTokens SpecialToken => CSharpSpecialTokens.NAMESPACE;
        /// <summary>
        /// The name of the namespace
        /// </summary>
        public string NamespaceName { get; }    

        public NamespaceContainerDefinition(string Name, string source) : base("namespace", source)
        {
            NamespaceName = Name;
        }

        public override string ToString()
        {
            return Title + " " + NamespaceName;
        }
    }
}
