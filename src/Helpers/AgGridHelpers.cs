using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorGrid.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace RazorGrid.Helpers;

public static class AgGridHelpers
{
    public static IHtmlContent RenderGridScript<T>(this IHtmlHelper htmlHelper, string gridId, ICollection<T> data)
    {
        // Generate column definitions dynamically, taking GridOptionsAttribute into account
        var columnDefs = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Where(prop => prop.GetCustomAttribute<GridOptionsAttribute>()?.Hide != true) // Exclude hidden properties
                                  .Select(prop =>
                                  {
                                      var attribute = prop.GetCustomAttribute<GridOptionsAttribute>();
                                      return new
                                      {
                                          field = prop.Name,
                                          headerName = attribute?.HeaderName ?? prop.Name, // Custom header or default to property name
                                          sortable = attribute?.Sortable ?? true,
                                          filter = attribute?.Filterable ?? true,
                                          width = attribute?.Width, // Include width if specified
                                          cellClass = attribute?.CellClass // Include CSS class if specified
                                      };
                                  })
                                  .ToArray();

        // Serialize data and column definitions to JSON
        string dataJson = JsonSerializer.Serialize(data);
        string columnDefsJson = JsonSerializer.Serialize(columnDefs);

        // Render the script
        string script = $@"
            <script>
                document.addEventListener('DOMContentLoaded', function() {{
                    var gridOptions = {{
                        columnDefs: {columnDefsJson},
                        rowData: {dataJson},
                        defaultColDef: {{
                            flex: 1,
                            minWidth: 100,
                            sortable: true,
                            filter: true,
                        }},
                    }};
                    var eGridDiv = document.querySelector('#{gridId}');
                    agGrid.createGrid(eGridDiv, gridOptions);
                }}); 
            </script>
        ";

        return new HtmlString(script);
    }
}
