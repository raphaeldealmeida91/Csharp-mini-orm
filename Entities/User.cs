// File: Entities/User.cs
using MiniOrm.Mapping;

namespace MiniOrm.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("age")]
        public int Age { get; set; }
    }
}