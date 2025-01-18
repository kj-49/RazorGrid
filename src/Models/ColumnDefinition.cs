using RazorGrid.Attributes;
using RazorGrid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RazorGrid.Models;
public class ColumnDefinition
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("headerName")]
    public string HeaderName { get; set; }

    [JsonPropertyName("sortable")]
    public bool Sortable { get; set; } = true;

    [JsonPropertyName("filter")]
    public bool Filter { get; set; } = true;

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("cellClass")]
    public string? CellClass { get; set; }

    [JsonPropertyName("cellRenderer")]
    public string CellRenderer { get; set; }

    public static ColumnDefinition FromPropertyInfo(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<GridOptionsAttribute>();
        return new ColumnDefinition
        {
            Field = prop.Name,
            HeaderName = attr?.HeaderName ?? prop.Name,
            Sortable = attr?.Sortable ?? true,
            Filter = attr?.Filterable ?? true,
            Width = attr?.Width,
            CellClass = attr?.CellClass
        };
    }
}
