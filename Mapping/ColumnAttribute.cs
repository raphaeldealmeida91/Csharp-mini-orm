// File: Mapping/ColumnAttribute.cs
namespace MiniOrm.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}