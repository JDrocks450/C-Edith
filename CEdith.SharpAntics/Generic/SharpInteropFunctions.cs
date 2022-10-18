using CEdith.SharpAntics.Code;
using CEdith.SharpAntics.Code.Definitions;
using CEdith.SharpAntics.Roslyn;
using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Generic
{
    internal class SharpInteropFunctions
    {
        /// <summary>
        /// A function to convert data found from Ast into SharpAntics types
        /// </summary>
        /// <param name="Translation"></param>
        /// <returns></returns>
        public static CodeObjectTreeNode? TranslateAstNode(in AstNode Translation)
        {
            if (Translation is UsingDeclaration import)
                return new CodeObjectTreeNode(new UsingDirectiveDefinition(import.Import.ToString(), import.ToString()));
            else if (Translation is NamespaceDeclaration namespec)
                return ProcNamespace(Translation, namespec);
            else if (Translation is TypeDeclaration typespec)
                return ProcType(Translation, typespec);
            else if (Translation is EntityDeclaration entitydecl)
                return ProcEntity(Translation, entitydecl);
            else if (Translation is BlockStatement blockstmt)
                return ProcFunc(Translation, blockstmt);
            return new CodeObjectTreeNode(Translation.GetType().Name, Translation.ToString());
        }

        private static CodeObjectTreeNode? ProcFunc(AstNode translation, BlockStatement statement)
        {
            CodeBlockDefinition blockDef = new CodeBlockDefinition(statement, translation.ToString());
            return new CodeObjectTreeNode(blockDef);
        }

        private static CodeObjectTreeNode? ProcEntity(AstNode translation, EntityDeclaration propdecl)
        {
            string name = propdecl.Name;
            if (propdecl is FieldDeclaration fielddecl)
                name = fielddecl.Name;
            var block = (BlockStatement?)propdecl.Children.FirstOrDefault(x => x is BlockStatement);
            var parameters = propdecl.Children.OfType<ParameterDeclaration>().ToArray();
            var type = new TypeMemberDefinition(
                (TypeMemberDefinition.TypeMemberSymbols)(byte)propdecl.SymbolKind,
                name, propdecl.ReturnType.ToString(), block, parameters
                );
            return new CodeObjectTreeNode(type);
        }

        private static CodeObjectTreeNode? ProcType(AstNode translation, TypeDeclaration typespec)
        {
            var type = new TypeDefinition(
                (TypeDefinition.ClassTypes)((int)typespec.ClassType), 
                typespec.Name);
            return new CodeObjectTreeNode(type);
        }

        private static CodeObjectTreeNode? ProcNamespace(in AstNode Node, NamespaceDeclaration namespec)
        {
            var def = new NamespaceContainerDefinition(namespec.Name, namespec.ToString());
            return new CodeObjectTreeNode(def);                
        }
    }
}
