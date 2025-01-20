using RazorGrid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Builders;
public class ColumnBuilder<T, TProperty>
{
    private readonly ColumnDefinition _column;
    private readonly GridBuilder<T> _gridBuilder;

    public ColumnBuilder(ColumnDefinition column, GridBuilder<T> gridBuilder)
    {
        _column = column;
        _gridBuilder = gridBuilder;
    }

    public GridBuilder<T> BuildColumn()
    {
        return _gridBuilder;
    }

    public ColumnBuilder<T, TProperty> SetVisible(bool visible = true)
    {
        _column.Hide = !visible;
        return this;
    }   

    public ColumnBuilder<T, TProperty> SetHeader(string header)
    {
        _column.HeaderName = header;
        return this;
    }

    public ColumnBuilder<T, TProperty> SetCellRenderer(string functionDefinition)
    {
        _column.CellRenderer = functionDefinition;
        return this;
    }
}