using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class GridOptionsAttribute : Attribute
{
    public bool Hide { get; set; } = false; 
    public string? HeaderName { get; set; }
    public int? Width { get; set; }
    public string? CellClass { get; set; }
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
}
