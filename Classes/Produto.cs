﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    }
}
