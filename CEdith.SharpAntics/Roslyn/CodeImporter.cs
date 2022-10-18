using CEdith.SharpAntics.Generic;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEdith.SharpAntics.Roslyn
{

    public abstract class CodeImporter : IParser
    {
        public static CodeUnit? ImportProject(string projectPath)
        {
            using (var fs = File.OpenRead(projectPath))
            {
                using (var TextReader = new StreamReader(fs))
                {
                    CSharpParser parser = new CSharpParser();
                    return new CodeUnit("Test", projectPath, parser.Parse(TextReader));                    
                }
            }
        }
    }
}
