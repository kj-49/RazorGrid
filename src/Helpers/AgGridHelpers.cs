using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorGrid.Builders;
using RazorGrid.Models;
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
    public static GridBuilder<T> RenderGridScript<T>(this IHtmlHelper _, string gridId, ICollection<T> data)
    {
        return new GridBuilder<T>(gridId, data);
    }

}
