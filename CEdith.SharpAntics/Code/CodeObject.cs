using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Code
{
    public enum CSharpSpecialTokens
    {
        NONE,
        USING,
        NAMESPACE,
        TYPE,
        TYPE_MEMBER,
        BLOCK,
        STATEMENT,
        EXPRESSION
    }
    /// <summary>
    /// The base class for abstracting a language component into something users can eventually manipulate and edit.
    /// </summary>
    public abstract class CodeObjectDefinition
    {
        public abstract CSharpSpecialTokens SpecialToken { get; }
        public string Title { get; protected internal set; }
        public string Description { get; protected internal set; }
        public string OriginalSource { get; protected internal set; }

        protected CodeObjectDefinition()
        {

        }

        protected CodeObjectDefinition(string Title, string Source) : this()
        {
            this.Title = Title;
            this.OriginalSource = Source;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
