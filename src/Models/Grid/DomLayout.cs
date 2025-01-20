using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Models.Grid;
public class DomLayout
{
    private DomLayout(string layout)
    {
        Layout = layout;
    }
    public string Layout { get; private set; }

    public static DomLayout Normal { get; } = new DomLayout("normal");
    public static DomLayout AutoHeight { get; } = new DomLayout("autoHeight");
    public static DomLayout Print { get; } = new DomLayout("print");
    public override string ToString()
    {
        return Layout;
    }
}
