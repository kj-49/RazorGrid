using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        // Generate column definitions dynamically
        var columnDefs = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Select(prop => new
                                  {
                                      field = prop.Name,
                                      headerName = prop.Name,
                                      sortable = true,
                                      filter = true
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
