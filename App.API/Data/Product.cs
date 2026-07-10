using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.API.Data
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        [Column(TypeName = "vector(1536)")] public SqlVector<float>? NameEmbedding { get; set; }
    }
}
