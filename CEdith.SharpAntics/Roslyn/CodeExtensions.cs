using CEdith.SharpAntics.Generic;
using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Roslyn
{
    public static class CodeExtensions
    {
        /// <summary>
        /// A function to convert data found from Ast into SharpAntics types
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public static CodeObjectTreeNode? Translate(this AstNode Node) => SharpInteropFunctions.TranslateAstNode(Node);
    }
}
