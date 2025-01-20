using Microsoft.AspNetCore.Html;
using RazorGrid.Models;
using RazorGrid.Models.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RazorGrid.Builders;
public class GridBuilder<T>
{
    private readonly List<ColumnDefinition> _columns = new();
    private readonly ICollection<T> _data;
    private readonly string _gridId;
    private readonly GridOptions _gridOptions = new();

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

    public GridBuilder<T> HideAll()
    {
        foreach (var column in _columns)
        {
            column.Hide = true;
        }

        return this;
    }
    public GridBuilder<T> SetDomLayout(DomLayout domLayout)
    {
        _gridOptions.DomLayout = domLayout;
        return this;
    }

    public ColumnBuilder<T, TProperty> For<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var propertyInfo = GetPropertyInfo(propertyExpression);
        if (propertyInfo is null)
        {
            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
        }

        ColumnDefinition? column = _columns.FirstOrDefault(c => c.Field == propertyInfo.Name.ToCamelCase());
        if (column is null)
        {
            throw new ArgumentException($"Column for property {propertyInfo.Name} does not exist.", nameof(propertyExpression));
        }

        return new ColumnBuilder<T, TProperty>(column, this);
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
                        domLayout: '{_gridOptions.DomLayout}',
                        columnDefs: {columnDefsJson},
                        rowData: {rowDataJson},
                        defaultColDef: {{
                            flex: 1,
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

    public class GridOptions
    {
        public DomLayout DomLayout { get; set; } = DomLayout.Normal;
    }
}
