using CEdith.SharpAntics.Code;

namespace CEdith.SharpAntics.Roslyn
{
    public class CodeObjectTreeNode
    {
        public CodeObjectDefinition BaseDefinition { get; }
        public string? Title => BaseDefinition?.Title ?? _title;
        public string? CSharpCode => BaseDefinition?.OriginalSource ?? _source;

        public bool Hidden { get; internal set; } = false;

        public readonly List<CodeObjectTreeNode> Children = new List<CodeObjectTreeNode>();
        private string _title;
        private string _source;

        public CodeObjectTreeNode(CodeObjectDefinition Object)
        {
            BaseDefinition = Object;
        }

        public CodeObjectTreeNode(string Title, string source)
        {
            _title = Title;
            _source = source;
        }

        internal void AddChild(CodeObjectTreeNode newChild)
        {
            Children.Add(newChild);
        }

        public override string ToString()
        {
            return BaseDefinition?.ToString() ?? Title;
        }
    }
}
