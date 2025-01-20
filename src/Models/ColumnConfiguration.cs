using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Models;

public class PropertyAccessor<T, TProperty>
{
    private readonly string _accessor;

    public PropertyAccessor(string accessor)
    {
        _accessor = accessor;
    }

    public override string ToString() => _accessor;
}

public static class StringExtensions
{
    public static string ToCamelCase(this string str) =>
        str.Length > 0 ? char.ToLowerInvariant(str[0]) + str[1..] : str;
}

public class ColumnConfig<T, TProperty>
{
    private readonly ColumnDefinition _column;

    public ColumnConfig(ColumnDefinition column)
    {
        _column = column;
    }

    public ColumnConfig<T, TProperty> SetHeader(string header)
    {
        _column.HeaderName = header;
        return this;
    }

    public ColumnConfig<T, TProperty> SetCellRenderer(string functionDefinition)
    {
        _column.CellRenderer = functionDefinition;
        return this;
    }
}