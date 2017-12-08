using System.Collections.Generic;

namespace Classes
{
    public class Nota
    {
        public List<Prod> Produto { get; set; } = new List<Nota.Prod>();
        public class Prod
        {
            /// <summary>
            /// Código do produto ou serviço
            /// </summary>
            public string cProd { get; set; }
            /// <summary>
            /// GTIN (Global Trade Item Number) do produto, antigo código EAN ou código de barras
            /// </summary>
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
            public string NVE { get; set; }
            /// <summary>
            /// Código CEST 
            /// </summary>
            public string CEST { get; set; }
            /// <summary>
            /// Indicador de Escala Relevante
            /// </summary>
            public string indEscala { get; set; }
            /// <summary>
            /// CNPJ do Fabricante da Mercadoria
            /// </summary>
            public string CNPJFab { get; set; }
            /// <summary>
            /// Código de Benefício Fiscal na UF aplicado ao item
            /// </summary>
            public string cBenef { get; set; }
            /// <summary>
            /// EX_TIPI
            /// </summary>
            public string EXTIPI { get; set; }
            /// <summary>
            /// Código Fiscal de Operações e Prestações
            /// </summary>
            public string CFOP { get; set; }
            /// <summary>
            /// Unidade Comercial 
            /// </summary>
            public string uCom { get; set; }
            /// <summary>
            /// Quantidade Comercial
            /// </summary>
            public decimal qCom { get; set; }
            /// <summary>
            /// Valor Unitário de Comercialização
            /// </summary>
            public decimal vUnCom { get; set; }
            /// <summary>
            /// Valor Total Bruto dos Produtos ou Serviços
            /// </summary>
            public decimal vProd { get; set; }
            /// <summary>
            /// GTIN (Global Trade Item Number) da unidade tributável, antigo código EAN ou código de barras
            /// </summary>
            public string cEANTrib { get; set; }
            /// <summary>
            /// Unidade Tributável
            /// </summary>
            public string uTrib { get; set; }
            /// <summary>
            /// Quantidade Tributável
            /// </summary>
            public decimal qTrib { get; set; }
            /// <summary>
            /// Valor Unitário de tributação
            /// </summary>
            public decimal vUnTrib { get; set; }
            /// <summary>
            /// Valor Total do Frete
            /// </summary>
            public decimal vFrete { get; set; }
            /// <summary>
            /// Valor Total do Seguro
            /// </summary>
            public decimal vSeg { get; set; }
            /// <summary>
            /// Valor do Desconto
            /// </summary>
            public decimal vDesc { get; set; }
            /// <summary>
            /// Outras despesas acessórias
            /// </summary>
            public decimal vOutro { get; set; }
            /// <summary>
            /// Indica se valor do Item (vProd) entra no valor total da NF-e(vProd)
            /// </summary>
            public int indTot { get; set; }
            /// <summary>
            /// CST
            /// </summary>
            public string CST { get; set; }
        }
    }
}
