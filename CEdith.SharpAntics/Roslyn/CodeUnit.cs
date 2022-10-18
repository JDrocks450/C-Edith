using CEdith.SharpAntics.Code;
using CEdith.SharpAntics.Code.Definitions;
using ICSharpCode.NRefactory.CSharp;

namespace CEdith.SharpAntics.Roslyn
{
    /// <summary>
    /// Represents one C# source file
    /// </summary>
    public class CodeUnit
    {        
        public const string ROOT_NAME = "Source";
        private readonly SyntaxTree tree;
        private readonly Dictionary<NodeType, List<AstNode>> typeMap = new();
        /// <summary>
        /// SharpAntics has special tokens defined for areas of importance to filter out code based on a certain type.
        /// </summary>
        private readonly Dictionary<CSharpSpecialTokens, List<CodeObjectTreeNode>> specialTokensMap = new();
        /// <summary>
        /// Returns a list of the token provided, if no list exists, creates a new list and returns the empty list created.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public IEnumerable<CodeObjectTreeNode> GetNodesBySpecialty(CSharpSpecialTokens Token)
        {
            if (specialTokensMap.TryGetValue(Token, out var nodes))
                return nodes;
            specialTokensMap.Add(Token, nodes = new List<CodeObjectTreeNode>());
            return specialTokensMap[Token];
        }

        #region NAMESPACE SUPPORT
        /// <summary>
        /// Maps namespace objects to it's contents.
        /// </summary>
        private Dictionary<NamespaceContainerDefinition, IEnumerable<TypeDefinition>> namespaceMap
        {
            get
            {
                var dict = new Dictionary<NamespaceContainerDefinition, IEnumerable<TypeDefinition>>();
                foreach (var namespec in GetNodesBySpecialty(CSharpSpecialTokens.NAMESPACE))
                    dict.Add((NamespaceContainerDefinition)namespec.BaseDefinition, 
                        namespec.Children.Select(x => x.BaseDefinition).OfType<TypeDefinition>());
                return dict;
            }
        }
        /// <summary>
        /// Gets the contents of a certain namespace based on the reference to the namespace definition.
        /// </summary>
        /// <param name="TreeNode">The treenode containing a <see cref="NamespaceContainerDefinition"/></param>
        /// <param name="Content">The content of the namespace</param>
        /// <returns>True if found</returns>
        public bool TryGetNamespaceTypes(CodeObjectTreeNode TreeNode, out IEnumerable<TypeDefinition> Content) =>
            TryGetNamespaceTypes((NamespaceContainerDefinition)TreeNode.BaseDefinition, out Content);
        /// <summary>
        /// Gets the contents of a certain namespace based on the reference to the namespace definition.
        /// </summary>
        /// <param name="Definition"></param>
        /// <param name="Content">The content of the namespace</param>
        /// <returns>True if found</returns>
        public bool TryGetNamespaceTypes(NamespaceContainerDefinition Definition, out IEnumerable<TypeDefinition> Content)
        {
            var result = namespaceMap.TryGetValue(Definition, out var list);
            Content = list; // i hate this about C# -- just box the list to ienumerable in the out statement but oh well
            return result;
        }
        #endregion

        #region TYPE CONTENT SUPPORT
        /// <summary>
        /// Maps Type objects to it's contents.
        /// </summary>
        private Dictionary<TypeDefinition, IEnumerable<TypeMemberDefinition>> typeContentMap
        {
            get
            {
                var dict = new Dictionary<TypeDefinition, IEnumerable<TypeMemberDefinition>>();
                foreach (var namespec in GetNodesBySpecialty(CSharpSpecialTokens.TYPE))
                    dict.Add((TypeDefinition)namespec.BaseDefinition,
                        namespec.Children.Select(x => x.BaseDefinition).OfType<TypeMemberDefinition>());
                return dict;
            }
        }
        /// <summary>
        /// Gets the contents of a certain type based on the reference to the type definition.
        /// </summary>
        /// <param name="TreeNode">The treenode containing a <see cref="TypeDefinition"/></param>
        /// <param name="Content">The content of the namespace</param>
        /// <returns>True if found</returns>
        public bool TryGetTypeMemberContents(CodeObjectTreeNode TreeNode, out IEnumerable<TypeMemberDefinition> Content) =>
            TryGetTypeMemberContents((TypeDefinition)TreeNode.BaseDefinition, out Content);
        /// <summary>
        /// Gets the contents of a certain type based on the reference to the type definition.
        /// </summary>
        /// <param name="Definition"></param>
        /// <param name="Content">The content of the namespace</param>
        /// <returns>True if found</returns>
        public bool TryGetTypeMemberContents(TypeDefinition Definition, out IEnumerable<TypeMemberDefinition> Content)
        {
            var result = typeContentMap.TryGetValue(Definition, out var list);
            Content = list; // i hate this about C# -- just box the list to ienumerable in the out statement but oh well
            return result;
        }
        #endregion

        /// <summary>
        /// This is the root node of this tree, always has the name defined at <see cref="ROOT_NAME"/>
        /// <para>This view hides Tokens, Identifiers, and other lower-level info</para>
        /// </summary>
        public CodeObjectTreeNode Root;
        /// <summary>
        /// Returns a list of all imports in this unit
        /// </summary>
        public IEnumerable<UsingDirectiveDefinition> Imports => 
            GetNodesBySpecialty(CSharpSpecialTokens.USING).Select(x => (UsingDirectiveDefinition)x.BaseDefinition);
        /// <summary>
        /// Returns a list of all imports in this unit
        /// </summary>
        public IEnumerable<NamespaceContainerDefinition> Namespaces => 
            GetNodesBySpecialty(CSharpSpecialTokens.NAMESPACE).Select(x => (NamespaceContainerDefinition)x.BaseDefinition);

        /// <summary>
        /// Creates a new CodeUnit from the source file provided
        /// </summary>
        /// <param name="Name">The name of this Source file</param>
        /// <param name="Path">The path to the source file</param>
        /// <param name="Tree">The parsed list of code information from this source file</param>
        public CodeUnit(string Name, string Path, SyntaxTree Tree)
        {
            this.Name = Name;
            this.Path = Path;
            tree = Tree;
            import(tree);
        }

        public string Name { get; }
        public string Path { get; }

        private void import(SyntaxTree tree)
        {            
            bool TryCreate(AstNode Node, out CodeObjectTreeNode NewNode)
            {
                NewNode = null;                
                if (Node is NewLineNode || Node is CSharpTokenNode) return false;
                if (typeMap.TryGetValue(Node.NodeType, out var list))
                    list.Add(Node);
                else
                    typeMap.Add(Node.NodeType, new List<AstNode> { Node });
                var me = Node.Translate();
                NamespaceContainerDefinition? currentNamespace = null;
                if (me.BaseDefinition != null)
                {
                    GetNodesBySpecialty(me.BaseDefinition.SpecialToken);
                    specialTokensMap[me.BaseDefinition.SpecialToken].Add(me);
                }                
                foreach (var child in Node.Children)
                {
                    if (TryCreate(child, out var newChild))                    
                        me.AddChild(newChild);
                }
                NewNode = me;
                NewNode.Hidden = Node is SimpleType or Identifier or PreProcessorDirective or MemberType;
                return true;                
            }
            Root = new CodeObjectTreeNode(ROOT_NAME, "");                
            foreach (var node in tree.Children)
            {
                if (TryCreate(node, out var newNode))
                    Root.AddChild(newNode);
            }
        }
    }
}
