using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Classes
{
    [Table(name: "produtos")]
    public class Produto
    {
        [Key]
        public int Id { get; set; }
        public string cProd { get; set; }
        public string xProd { get; set; }
        public string NCM { get; set; }
        public string CEST { get; set; }
        public string CFOP { get; set; }
        public string CST { get; set; }
    }
}
