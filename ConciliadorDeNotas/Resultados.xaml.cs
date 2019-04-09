using Classes;
using Enums;
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

namespace ConciliadorDeNotas
{
    /// <summary>
    /// Interaction logic for Resultados.xaml
    /// </summary>
    public partial class Resultados : Window
    {
        #region Variáveis

        List<Nota.det.Prod> produtos = new List<Nota.det.Prod>();
        List<Empresa> listaDeEmpresa = new List<Empresa>();
        decimal totalProdutosNota = 0;

        #endregion

        public Resultados(List<Nota.det.Prod> _produtos, List<Empresa> _listaDeEmpresa)
        {
            InitializeComponent();

            // Pega produtos clonando
            foreach (var produto in _produtos)
            {
                produtos.Add(ObjectCopier.Clone(produto));
            }

            listaDeEmpresa = _listaDeEmpresa;

            // Formata vProd
            try {
                foreach (var produto in produtos)
                {
                    produto.vProd = decimal.Parse(produto.vProd.Replace(".", ",")).ToString("##,###,##0.00");
                }
            }
            catch (Exception ex)
            {

            }

            dgListagem.ItemsSource = produtos.OrderBy(c => c.STATUS).ThenBy(c => c.xProd).ToList();

            //totalProdutosNota = produtos.Sum(c => decimal.Parse(c.vProd.Replace(".",",")));
            totalProdutosNota = produtos.Sum(c => c.vProdTotal);
            labelTotalValorNotas.Text = "Valor total dos produtos: R$ " + totalProdutosNota.ToString("#,###,##0.00");

            if (listaDeEmpresa.Count == 1)
            {
                labelCNPJRazaoSocial.Text = $"CNPJ: {listaDeEmpresa.First().cnpj}, Razão Social: {listaDeEmpresa.First().empresaRazaoSocial}";
            }
            else
            {
                labelCNPJRazaoSocial.Text = "Não foi possível detectar a empresa emitente.";
            }
        }

        private void HandleExpandCollapseForAll(object sender, RoutedEventArgs e)
        {
            Button expandCollapseButtonAll = (Button)sender;

            if (null != expandCollapseButtonAll && "+" == expandCollapseButtonAll.Content.ToString())
            {
                dgListagem.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
                expandCollapseButtonAll.Content = "-";
            }
            else
            {
                dgListagem.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                expandCollapseButtonAll.Content = "+";
            }
        }

        private void HandleExpandCollapseForRow(object sender, RoutedEventArgs e)
        {
            Button expandCollapseButton = (Button)sender;
            DataGridRow selectedRow = DataGridRow.GetRowContainingElement(expandCollapseButton);

            if (null != expandCollapseButton && "+" == expandCollapseButton.Content.ToString() && ((Nota.det.Prod)selectedRow.Item).STATUS == STATUS.Invalido)
            {
                selectedRow.DetailsVisibility = Visibility.Visible;
                expandCollapseButton.Content = "-";
            }
            else
            {
                selectedRow.DetailsVisibility = Visibility.Collapsed;
                expandCollapseButton.Content = "+";
            }
        }

        private void btnRelatorio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Relatorio result = new Relatorio(produtos, listaDeEmpresa);
                result.WindowState = WindowState;
                result.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir relatório.\nErro Interno: " + ex.Message + "\nInner" + ex?.InnerException?.Message);
            }
        }
    }
}
