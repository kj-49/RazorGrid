using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Serialization;
public class RawJavaScript
{
    public string Script { get; }

    public RawJavaScript(string script)
    {
        Script = script;
    }

    public override string ToString() => Script;
}
