using RazorGrid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Models;

public class CellRendererParams<T>
{
    private readonly string _code = "";
    private bool _isFunction;

    public DataParams<T> data => new();
    public ValueParam value => new();

    public string Build() => _isFunction ? _code : $"params => {_code}";

    public static implicit operator string(CellRendererParams<T> renderer) => renderer.Build();

    public class DataParams<TModel>
    {
        public PropertyAccessor<TModel, TProperty> Field<TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var memberExp = expression.Body as MemberExpression;
            var propertyName = memberExp?.Member.Name.ToCamelCase();
            return new PropertyAccessor<TModel, TProperty>($"params.data.{propertyName}");
        }
    }

    public class ValueParam
    {
        public override string ToString() => "params.value";
    }
}

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

    public ColumnConfig<T, TProperty> SetCellRenderer(Func<CellRendererParams<T>, string> renderer)
    {
        var p = new CellRendererParams<T>();
        var rendererResult = renderer(p);
        _column.CellRenderer = $"params => {{ return `{rendererResult.Replace("params.value", "${params.value}")}`; }}";
        return this;
    }


    // ... other configuration methods
}