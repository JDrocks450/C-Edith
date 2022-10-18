using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code.Definitions
{
    /// <summary>
    /// Abstracts multiple different member declarations of a type down to one base class
    /// </summary>
    public class TypeMemberDefinition : CodeObjectDefinition
    {
        public override CSharpSpecialTokens SpecialToken => CSharpSpecialTokens.TYPE_MEMBER;
        /// <summary>
        /// The name of this type member
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The return type of this particular member
        /// </summary>
        public string ReturnType { get; }
        /// <summary>
        /// The type of member this symbol represents
        /// </summary>
        public TypeMemberSymbols SymbolKind { get; }

        public IEnumerable<Statement> Block { get; } = new List<Statement>();
        public ParameterDeclaration[]? Parameters { get; }
        
        /// <summary>
        /// Creates a type member definition
        /// </summary>
        /// <param name="symbolKind"></param>
        /// <param name="name"></param>
        /// <param name="returnType"></param>
        public TypeMemberDefinition(TypeMemberSymbols symbolKind, string name, string returnType, IEnumerable<Statement> block, params ParameterDeclaration[] Parameters) : base(symbolKind.ToString(), "")
        {
            Name = name;
            ReturnType = returnType;
            Block = block;
            this.Parameters = Parameters;
            SymbolKind = symbolKind;
        }

        public override string ToString()
        {
            return $"{Title} \'{Name}\' ({ReturnType})"; 
        }

        public enum TypeMemberSymbols : byte
        {
            None,
            TypeDefinition,
            Field,
            Property,
            Indexer,
            Event,
            Method,
            Operator,
            Constructor,
            Destructor,
            Accessor,
            Namespace,
            Variable,
            Parameter,
            TypeParameter
        }        
    }
}
