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
using Microsoft.Reporting.WinForms;
using System.IO;

namespace ConciliadorDeNotas
{
    /// <summary>
    /// Interaction logic for Relatorio.xaml
    /// </summary>
    public partial class Relatorio : Window
    {
        List<Produto> produtos = new List<Produto>();
        List<Produto> produtosMonofasicos = new List<Produto>();
        List<Produto> todosProdutos = new List<Produto>();
        string empresa = "";

        public Relatorio(List<Nota.det.Prod> _produtos, List<Empresa> _listaDeEmpresa)
        {
            InitializeComponent();

            produtos = ConverterProdToProduto(_produtos);
            produtos.OrderBy(c => c.STATUS).OrderBy(c => c.xProd);

            todosProdutos = produtos;
            produtosMonofasicos = produtos.Where(c => c.isManofasico == true).ToList();
            produtos = produtos.Where(c => c.isManofasico == false || c.isManofasico == null).ToList();

            if (_listaDeEmpresa.Count == 1)
            {
                empresa = $"CNPJ: {_listaDeEmpresa.First().cnpj}, Razão Social: {_listaDeEmpresa.First().empresaRazaoSocial}";
            }
            else
            {
                empresa = "Não foi possível detectar a empresa emitente.";
            }
        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {
            var dataSource = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetR", produtos);
            var dataSourceMonofasico = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetMonofasicos", produtosMonofasicos);
            ReportViewer.LocalReport.DataSources.Add(dataSource);
            ReportViewer.LocalReport.DataSources.Add(dataSourceMonofasico);
            ReportViewer.LocalReport.ReportEmbeddedResource = "ConciliadorDeNotas.RelatorioResultados.rdlc";
            ReportViewer.LocalReport.SetParameters(new ReportParameter("EMPRESA", empresa));

            ReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(SubreportEventHandler);

            ReportViewer.RefreshReport();
        }

        private void SubreportEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            try
            {

                int codigoProduto = int.Parse(e.Parameters.Where(c => c.Name == "CodigoProduto").First().Values[0]);
                string descricaoProduto = e.Parameters.Where(c => c.Name == "DescricaoProduto").First().Values[0];
                var erros = new List<Nota.det.Prod.Error>();
            try
            {
                erros = todosProdutos.Where(c => c.Id == codigoProduto && c.xProd == descricaoProduto).First().listaErros;
            }
            catch { }

            var dataSource = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetErros", erros);

            e.DataSources.Add(dataSource);
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<Produto> ConverterProdToProduto(List<Nota.det.Prod> produtos)
        {
            List<Produto> produtosReturn = new List<Produto>();

            foreach (var produto in produtos)
            {
                produtosReturn.Add(new Produto() {
                    cProd = produto.cProd,
                    xProd = produto.xProd,
                    NCM = string.IsNullOrEmpty(produto.NCM) ? 0 : int.Parse(produto.NCM),
                    CST = string.IsNullOrEmpty(produto.CST) ? 0 : int.Parse(produto.CST),
                    CFOP = string.IsNullOrEmpty(produto.CFOP) ? 0 : int.Parse(produto.CFOP),
                    CEST = string.IsNullOrEmpty(produto.CEST) ? 0 : int.Parse(produto.CEST),
                    //vProd = string.IsNullOrEmpty(produto.vProd) ? 0 : decimal.Parse(produto.vProd.Replace(".", ",")),
                    vProd = produto.vProdTotal,
                    STATUS = produto.STATUS,
                    quantidadeErros = produto.quantidadeErros,
                    listaErros = produto.listaErros,
                    isManofasico = produto.isManofasico
                });
            }

            return produtosReturn;
        }
    }
}
