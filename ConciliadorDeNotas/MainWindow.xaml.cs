using Classes;
using Infraestrutura;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConciliadorDeNotas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variáveis

        OpenFileDialog fileDialog;
        DataSet ds = new DataSet();
        Nota nota = new Nota();
        List<Nota.Prod> produtosNota = new List<Nota.Prod>();

        #endregion
        public MainWindow()
        {
            InitializeComponent();

            fileDialog = new OpenFileDialog();
            ExibeComponentsDados(false);

            CarreDados();
        }

        private void btnXML_Click(object sender, RoutedEventArgs e)
        {
            produtosNota.Clear();
            ds.Clear();

            fileDialog.Filter = "Arquivos XML (*.xml)|*.xml|Todos Arquivos (*.*)|*.*";
            string fileName = "";
            try
            {
                if (fileDialog.ShowDialog() == true)
                {

                    try
                    {
                        fileName = fileDialog.FileName;
                        if (fileName.Substring(fileName.Length - 4, 4) != ".xml")
                        {
                            MessageBox.Show("Selecione um arquivo válido com a extensão XML.");
                            return;
                        }

                        ds.ReadXml(fileName);

                        var produtosXML = ds.Tables["prod"].AsEnumerable().Distinct();
                        produtosNota = ProdutoXmlToObject(produtosXML);

                        // Insere Dados
                        labelCliente.Text = String.Format("Cliente: {0}", GetValueXml("emit", "xNome", ds));
                        labelCNPJ.Text = String.Format("CNPJ: {0}", GetValueXml("emit", "CNPJ", ds));
                        labelDataEmissao.Text = String.Format("Cliente: {0}", DateTime.Parse(GetValueXml("ide", "dhEmi", ds)));
                        labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosNota.Count);

                        // Exibe Dados
                        ExibeComponentsDados(true);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n Arquivo erro: " + fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir o arquivo.\n\n" +
                                            "Mensagem : " + ex.Message + "\n\n" +
                                            "Detalhes :\n\n" + ex.StackTrace);
            }
        }

        private void btnTXT_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TXT");
        }

        private List<Nota.Prod> ProdutoXmlToObject(IEnumerable<DataRow> produtos)
        {
            var produtosReturn = new List<Nota.Prod>();

            foreach (var produto in produtos)
            {
                var produtoInstance = new Nota.Prod();
                nota.Produto.Add(produtoInstance);

                foreach (var column in produto.Table.Columns)
                {
                    var columnObj = ((DataColumn)column);

                    var property = produtoInstance.GetType().GetProperty(columnObj.ColumnName);
                    object columnValue = null;

                    if (property == null)
                        continue;

                    if (property.PropertyType.ToString() == "System.Int")
                        columnValue = produto.Field<int>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.Decimal")
                        columnValue = decimal.Parse(produto.Field<string>(columnObj.ColumnName).Replace(".", ","));
                    if (property.PropertyType.ToString() == "System.Double")
                        columnValue = produto.Field<double>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.String")
                        columnValue = produto.Field<string>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.DateTime")
                        columnValue = produto.Field<DateTime>(columnObj.ColumnName);

                    produtoInstance.GetType().GetProperty(columnObj.ColumnName).SetValue(produtoInstance, columnValue);
                }
                produtosReturn.Add(produtoInstance);
            }

            return produtosReturn;
        }

        private string GetValueXml(string table, string field, DataSet ds)
        {
            return ds.Tables[table].AsEnumerable().Distinct().FirstOrDefault().Field<string>(field);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Deseja sair?", "Sair do sistema.", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void ExibeComponentsDados(bool exibe)
        {
            if (exibe)
            {
                groupBoxDados.Visibility = Visibility.Visible;
                labelCliente.Visibility = Visibility.Visible;
                labelCNPJ.Visibility = Visibility.Visible;
                labelDataEmissao.Visibility = Visibility.Visible;
                labelProdutos.Visibility = Visibility.Visible;

                labelAnaliseFinalizada.Visibility = Visibility.Visible;
                elipseProdutosConciliados.Visibility = Visibility.Visible;
                labelProdutosConciliados.Visibility = Visibility.Visible;
                elipseProdutosNaoConciliados.Visibility = Visibility.Visible;
                labelProdutosNaoConciliados.Visibility = Visibility.Visible;

                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Hidden;

                btnVerResultados.Visibility = Visibility.Visible;
                btnSalvarProdutos.Visibility = Visibility.Visible;
            }
            else {
                groupBoxDados.Visibility = Visibility.Hidden;
                labelCliente.Visibility = Visibility.Hidden;
                labelCNPJ.Visibility = Visibility.Hidden;
                labelDataEmissao.Visibility = Visibility.Hidden;
                labelProdutos.Visibility = Visibility.Hidden;

                labelAnaliseFinalizada.Visibility = Visibility.Hidden;
                elipseProdutosConciliados.Visibility = Visibility.Hidden;
                labelProdutosConciliados.Visibility = Visibility.Hidden;
                elipseProdutosNaoConciliados.Visibility = Visibility.Hidden;
                labelProdutosNaoConciliados.Visibility = Visibility.Hidden;

                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Visible;

                btnVerResultados.Visibility = Visibility.Hidden;
                btnSalvarProdutos.Visibility = Visibility.Hidden;
            }
            
        }

        private void CarreDados()
        {
            try
            {
                Contexto context = new Contexto();

                var data = context.Produto.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }

        }

        private void btnSalvarProdutos_Click(object sender, RoutedEventArgs e)
        {
            int registrosSalvos = 0;
            int registrosNaoSalvos = 0;
            try
            {
                using (var db = new Contexto())
                {
                    foreach (var produto in produtosNota)
                    {
                        var produtoNoBanco = db.Produto.Where(c => c.cProd == produto.cProd).FirstOrDefault();

                        if(produtoNoBanco == null) {
                            db.Produto.Add(new Produto()
                            {
                                cProd = produto.cProd,
                                xProd = produto.xProd,
                                NCM = produto.NCM,
                                CEST = produto.CEST,
                                CFOP = produto.CFOP,
                                CST = "01",
                            });
                        }
                        else
                        {
                            registrosNaoSalvos++;
                        }
                    }

                    registrosSalvos += db.SaveChanges();
                    MessageBox.Show(String.Format("{0} produtos foram salvos no banco de dados e {1} já estavam salvos", registrosSalvos, registrosNaoSalvos));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            btnXML_Click(null, null);
        }
    }
}
