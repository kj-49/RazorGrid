using Microsoft.AspNetCore.Html;
using RazorGrid.Models;
using RazorGrid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RazorGrid.Helpers;
public class GridBuilder<T>
{
    private readonly List<ColumnDefinition> _columns = new();
    private readonly ICollection<T> _data;
    private readonly string _gridId;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new RawJavaScriptConverter() }
    };


    public GridBuilder(string gridId, ICollection<T> data)
    {
        _gridId = gridId;
        _data = data;
    }

    public GridBuilder<T> Column<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        Action<ColumnConfig<T, TProperty>> configure = null)
    {
        var prop = GetPropertyInfo(propertyExpression);
        var column = new ColumnDefinition
        {
            Field = prop.Name.ToCamelCase(),
            HeaderName = prop.Name
        };

        if (configure != null)
        {
            var config = new ColumnConfig<T, TProperty>(column);
            configure(config);
        }

        _columns.Add(column);
        return this;
    }

    private static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var member = propertyExpression.Body as MemberExpression;
        return member?.Member as PropertyInfo;
    }

    public IHtmlContent Build()
    {
        var columnDefsJson = new StringBuilder();
        var rowDataJson = JsonSerializer.Serialize(_data, JsonOptions);

        // Manually build column definitions (serialize other properties, handle cellRenderer separately)
        columnDefsJson.Append("[");
        foreach (var column in _columns)
        {
            columnDefsJson.Append("{");

            // Serialize properties normally
            columnDefsJson.Append($"\"field\":\"{column.Field}\",");
            columnDefsJson.Append($"\"headerName\":\"{column.HeaderName}\",");
            columnDefsJson.Append($"\"sortable\":{column.Sortable.ToString().ToLower()},");
            columnDefsJson.Append($"\"filter\":{column.Filter.ToString().ToLower()},");

            // If cellRenderer is not null or empty, insert it as raw JavaScript (no quotes)
            if (!string.IsNullOrEmpty(column.CellRenderer))
            {
                columnDefsJson.Append($"\"cellRenderer\": {column.CellRenderer},"); // Insert raw JavaScript
            }

            columnDefsJson.Length--; // Remove last comma
            columnDefsJson.Append("},");
        }

        columnDefsJson.Length--; // Remove last comma
        columnDefsJson.Append("]");

        // Build the script with all the properties serialized and cellRenderer handled manually
        var script = $@"
        <script>
            document.addEventListener('DOMContentLoaded', () => {{
                const gridOptions = {{
                    columnDefs: {columnDefsJson},
                    rowData: {rowDataJson},
                    defaultColDef: {{
                        flex: 1,
                        minWidth: 100,
                        sortable: true,
                        filter: true
                    }}
                }};
                agGrid.createGrid(document.querySelector('#{_gridId}'), gridOptions);
            }}); 
        </script>";

        return new HtmlString(script);
    }

}
