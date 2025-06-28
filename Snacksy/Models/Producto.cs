using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Snacksy.Models
{
    [Table("productos")]
    public class Producto : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("nombre")]
        public string Name { get; set; } = string.Empty;

        [Column("precio")]
        public string Price { get; set; } = string.Empty;

        [Column("stock")]
        public string Stock { get; set; } = string.Empty;
    }
}
