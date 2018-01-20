using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Classes;

namespace ConciliadorDeNotas
{
    /// <summary>
    /// Interaction logic for Relatorio.xaml
    /// </summary>
    public partial class Relatorio : Window
    {
        List<Produto> produtos = new List<Produto>();

        public Relatorio(List<Nota.det.Prod> _produtos)
        {
            InitializeComponent();

            produtos = ConverterProdToProduto(_produtos);
        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {
            var dataSource = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetR", produtos);
            ReportViewer.LocalReport.DataSources.Add(dataSource);
            ReportViewer.LocalReport.ReportEmbeddedResource = "ConciliadorDeNotas.RelatorioResultados.rdlc";

            ReportViewer.RefreshReport();
        }

        private List<Produto> ConverterProdToProduto(List<Nota.det.Prod> produtos)
        {
            List<Produto> produtosReturn = new List<Produto>();

            foreach (var produto in produtos)
            {
                produtosReturn.Add(new Produto() {
                    cProd = int.Parse(produto.cProd),
                    xProd = produto.xProd,
                    NCM = int.Parse(produto.NCM),
                    CST = int.Parse(produto.CST),
                    CFOP = int.Parse(produto.CFOP),
                    CEST = int.Parse(produto.CEST),
                    vProd = decimal.Parse(produto.vProd.Replace(".",","))
                    ,
                });
            }

            return produtosReturn;
        }
    }
}
