using Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;

namespace Classes
{
    public class Nota
    {
        public List<det> detList { get; set; } = new List<det>();

        public class det
        {
            public Prod prod { get; set; }
            public Imposto imposto { get; set; }

            public det()
            {
                prod = new Prod();
                imposto = new Imposto();
            }

            [Table(name: "produtos")]
            public class Prod : INotifyPropertyChanged
            {
                [Key]
                public int Id { get; set; }
                /// <summary>
                /// Código do produto ou serviço
                /// </summary>
                public string cProd { get; set; }
                /// <summary>
                /// GTIN (Global Trade Item Number) do produto, antigo código EAN ou código de barras
                /// </summary>
                [NotMapped]
                public string cEAN { get; set; }
                /// <summary>
                /// Descrição do produto ou serviço
                /// </summary>
                public string xProd { get; set; }
                /// <summary>
                /// Código NCM com 8 dígitos 
                /// </summary>
                public string NCM { get; set; }
                /// <summary>
                /// Codificação NVE - Nomenclatura de Valor Aduaneiro e Estatística
                /// </summary>
                [NotMapped]
                public string NVE { get; set; }
                /// <summary>
                /// Código CEST 
                /// </summary>
                public string CEST { get; set; }
                /// <summary>
                /// Indicador de Escala Relevante
                /// </summary>
                [NotMapped]
                public string indEscala { get; set; }
                /// <summary>
                /// CNPJ do Fabricante da Mercadoria
                /// </summary>
                [NotMapped]
                public string CNPJFab { get; set; }
                /// <summary>
                /// Código de Benefício Fiscal na UF aplicado ao item
                /// </summary>
                [NotMapped]
                public string cBenef { get; set; }
                /// <summary>
                /// EX_TIPI
                /// </summary>
                [NotMapped]
                public string EXTIPI { get; set; }
                /// <summary>
                /// Código Fiscal de Operações e Prestações
                /// </summary>
                public string CFOP { get; set; }
                /// <summary>
                /// Unidade Comercial 
                /// </summary>
                [NotMapped]
                public string uCom { get; set; }
                /// <summary>
                /// Quantidade Comercial
                /// </summary>
                [NotMapped]
                public decimal qCom { get; set; }
                /// <summary>
                /// Valor Unitário de Comercialização
                /// </summary>
                [NotMapped]
                public decimal vUnCom { get; set; }
                /// <summary>
                /// Valor Total Bruto dos Produtos ou Serviços
                /// </summary>
                public string vProd { get; set; }
                /// <summary>
                /// GTIN (Global Trade Item Number) da unidade tributável, antigo código EAN ou código de barras
                /// </summary>
                [NotMapped]
                public string cEANTrib { get; set; }
                /// <summary>
                /// Unidade Tributável
                /// </summary>
                [NotMapped]
                public string uTrib { get; set; }
                /// <summary>
                /// Quantidade Tributável
                /// </summary>
                [NotMapped]
                public decimal qTrib { get; set; }
                /// <summary>
                /// Valor Unitário de tributação
                /// </summary>
                [NotMapped]
                public decimal vUnTrib { get; set; }
                /// <summary>
                /// Valor Total do Frete
                /// </summary>
                [NotMapped]
                public decimal vFrete { get; set; }
                /// <summary>
                /// Valor Total do Seguro
                /// </summary>
                [NotMapped]
                public decimal vSeg { get; set; }
                /// <summary>
                /// Valor do Desconto
                /// </summary>
                [NotMapped]
                public decimal vDesc { get; set; }
                /// <summary>
                /// Outras despesas acessórias
                /// </summary>
                [NotMapped]
                public decimal vOutro { get; set; }
                /// <summary>
                /// Indica se valor do Item (vProd) entra no valor total da NF-e(vProd)
                /// </summary>
                [NotMapped]
                public int indTot { get; set; }
                /// <summary>
                /// CST ICMS
                /// </summary>
                public string CST { get; set; }
                /// <summary>
                /// CST PIS
                /// </summary>
                public string CST_PIS { get; set; }
                /// <summary>
                /// CST COFINS
                /// </summary>
                public string CST_COFINS { get; set; }
                /// <summary>
                /// Conciliado - Define se o produto da nota foi conciliado  com o do banco
                /// </summary>
                [NotMapped]
                public STATUS STATUS { get; set; }
                /// <summary>
                /// listaErros - Lista de erros da conciliação
                /// </summary>
                [NotMapped]
                public List<Error> listaErros { get; set; } = new List<Error>();
                /// <summary>
                /// quantidadeErros - Quantidade de erros da conciliação
                /// </summary>
                [NotMapped]
                public int quantidadeErros { get; set; } = 0;
                /// <summary>
                /// CorStatus - Cor do status do produto
                /// </summary>
                [NotMapped]
                public string CorStatus { get; set; }

                [NotMapped]
                private Visibility visibility;

                [NotMapped]
                public Visibility Visibility
                {
                    get
                    {
                        return visibility;
                    }
                    set
                    {
                        visibility = value;

                        OnPropertyChanged("Visibility");
                    }
                }

                private void OnPropertyChanged(string info)
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(info));
                    }
                }

                public event PropertyChangedEventHandler PropertyChanged;

                public class Error
                {
                    public string Message { get; set; }
                }
            }

            public class Imposto
            {
                public decimal vTotTrib { get; set; }
                public ICMS ICMSPropery { get; set; }

                public class ICMS
                {
                }
            }

        }
    }
}
