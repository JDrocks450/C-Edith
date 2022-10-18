using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code.Definitions
{
    public class CodeBlockDefinition : CodeObjectDefinition
    {
        public BlockStatement Reference { get; }

        public override CSharpSpecialTokens SpecialToken => CSharpSpecialTokens.BLOCK;
        public CodeBlockDefinition(BlockStatement Reference, string source) : base("Scope", source)
        {
            this.Reference = Reference;
        }
    }
}
