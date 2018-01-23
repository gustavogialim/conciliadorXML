using Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Classes.Nota.det.Prod;

namespace Classes
{
    [Table(name: "produtos")]
    public class Produto
    {
        [Key]
        public int Id { get; set; }
        public int cProd { get; set; }
        public string xProd { get; set; }
        public decimal vProd { get; set; }
        public int NCM { get; set; }
        public int CEST { get; set; }
        public int CFOP { get; set; }
        public int CST { get; set; }
        public STATUS STATUS { get; set; }
        public List<Error> listaErros { get; set; } = new List<Error>();
        public int quantidadeErros { get; set; } = 0;
        public bool? isManofasico { get; set; }

        public override string ToString()
        {
            return $"{xProd} ({isManofasico})";
        }
    }
}
