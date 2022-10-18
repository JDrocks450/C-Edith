using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code.Definitions
{
    public sealed class UsingDirectiveDefinition : CodeObjectDefinition
    {
        public override CSharpSpecialTokens SpecialToken => CSharpSpecialTokens.USING;
        public string Namespace { get; }
        /// <summary>
        /// Represents a using directive (an import)
        /// </summary>
        /// <param name="Namespace"></param>
        public UsingDirectiveDefinition(string Namespace, string Source) : base("using", Source)
        {            
            this.Namespace = Namespace;
            this.Description = Namespace;
        }

        public override string ToString()
        {
            return Title + " " + Namespace;
        }
    }
}
