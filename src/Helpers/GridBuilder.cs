using Microsoft.AspNetCore.Html;
using RazorGrid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private class SerializableColumnDefinition
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new();

        private static readonly string CellRendererPropertyName =
            nameof(ColumnDefinition.CellRenderer).ToCamelCase();

        public static SerializableColumnDefinition From(ColumnDefinition column)
        {
            var columnJson = JsonSerializer.Serialize(column, JsonOptions);
            var serializableColumn = new SerializableColumnDefinition();

            serializableColumn.AdditionalProperties = JsonSerializer
                .Deserialize<Dictionary<string, JsonElement>>(columnJson, JsonOptions)
                ?? throw new InvalidOperationException("Failed to serialize column definition");

            serializableColumn.AdditionalProperties.Remove(CellRendererPropertyName);

            return serializableColumn;
        }

        public string ToJson(string? cellRenderer)
        {
            var json = JsonSerializer.Serialize(this, JsonOptions);

            if (string.IsNullOrEmpty(cellRenderer))
            {
                return json;
            }

            return json.Insert(json.Length - 1, $",\"{CellRendererPropertyName}\":{cellRenderer}");
        }
    }

    public GridBuilder(string gridId, ICollection<T> data)
    {
        _gridId = gridId;
        _data = data;
        InitializeColumns();
    }
    private void InitializeColumns()
    {
        // Initialize columns with default settings (can add more defaults here if needed)
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var column = new ColumnDefinition
            {
                Field = prop.Name.ToCamelCase(),
                HeaderName = prop.Name
            };
            _columns.Add(column);
        }
    }

    public GridBuilder<T> Column<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        Action<ColumnConfig<T, TProperty>>? configure = null)
    {
        PropertyInfo? prop = GetPropertyInfo(propertyExpression);

        if (prop is null)
        {
            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
        }

        var column = new ColumnDefinition
        {
            Field = prop.Name.ToCamelCase(),
            HeaderName = prop.Name
        };

        if (configure is not null)
        {
            var config = new ColumnConfig<T, TProperty>(column);
            configure(config);
        }

        _columns.Add(column);
        return this;
    }

    private static PropertyInfo? GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var member = propertyExpression.Body as MemberExpression;
        return member?.Member as PropertyInfo;
    }

    public IHtmlContent Build()
    {
        var columnDefsJson = SerializeColumnDefinitions();
        var rowDataJson = JsonSerializer.Serialize(_data, JsonOptions);

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

    private string SerializeColumnDefinitions()
    {
        var serializedColumns = _columns
            .Select(column => SerializableColumnDefinition
                .From(column)
                .ToJson(column.CellRenderer));

        return $"[{string.Join(",", serializedColumns)}]";
    }
}
