// File: Mapping/ColumnAttribute.cs
namespace MiniOrm.Mapping
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}