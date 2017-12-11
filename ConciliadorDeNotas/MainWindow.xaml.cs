using Classes;
using Enums;
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
        List<Nota.det.Prod> produtosNota = new List<Nota.det.Prod>();
        List<Nota.det.Prod> produtosBanco = new List<Nota.det.Prod>();
        string[] cCstIcms = { "ICMS00", "ICMS10", "ICMS20", "ICMS30", "ICMS40", "ICMS51", "ICMS60", "ICMS70", "ICMS90", "ICMSSN101", "ICMSSN102", "ICMSSN201", "ICMSSN202", "ICMSSN500", "ICMSSN900" };

        #endregion
        public MainWindow()
        {
            InitializeComponent();

            fileDialog = new OpenFileDialog();
            ExibeComponentsDados(false);

            PingBanco();
            //DeletaProdutos();
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

                        // Det
                        DataTable det = !ds.Tables.Contains("det") ? null : ds.Tables["det"];
                        DataColumnCollection detCol = det != null ? det.Columns : null;

                        // Produto
                        DataTable prod = !ds.Tables.Contains("prod") ? null : ds.Tables["prod"];
                        DataColumnCollection prodCol = prod != null ? prod.Columns : null;

                        // Importo
                        DataTable imposto = !ds.Tables.Contains("imposto") ? null : ds.Tables["imposto"];
                        DataColumnCollection impostoCol = imposto != null ? imposto.Columns : null;

                        // Det JOIN Produtos
                        var detProdutos = (det == null ? null :
                               from dt in det.AsEnumerable()
                               join p in prod.AsEnumerable() on dt.Field<int>("det_id") equals p.Field<int>("det_id")
                               select new
                               {
                                   det = dt,
                                   prod = p
                               }).OrderBy(p => p.prod.Field<string>("cProd")).ToList();

                        foreach (var detProduto in detProdutos)
                        {
                            var detInstance = new Nota.det();
                            nota.detList.Add(detInstance);

                            var produtoInstance = prod.AsEnumerable().Where(c => c.Field<int>("det_id") == detProduto.det.Field<int> ("det_id"));
                            detInstance.prod = ProdutoXmlToObject(produtoInstance).FirstOrDefault();

                            var impostoInstance = imposto.AsEnumerable().Where(c => c.Field<int>("det_id") == detProduto.det.Field<int>("det_id"));
                            detInstance.imposto = ImpostoXmlToObject(impostoInstance).FirstOrDefault();

                            if (impostoInstance != null)
                            {
                                foreach (var item in cCstIcms)
                                {
                                    DataTable icms = !ds.Tables.Contains("ICMS") ? null : ds.Tables["ICMS"];
                                    DataColumnCollection icmsCol = imposto != null ? imposto.Columns : null;

                                    var icmsInstance = icms.AsEnumerable().Where(c => c.Field<int>("imposto_id") == impostoInstance.FirstOrDefault().Field<int>("imposto_id")).FirstOrDefault();

                                    if (ds.Tables.Contains(item))
                                    {
                                        DataTable cstIcms = ds.Tables[item];
                                        DataColumnCollection cstIcmsCol = cstIcms != null ? cstIcms.Columns : null;

                                        var cstInstance = cstIcms.AsEnumerable().Where(c => c.Field<int>("icms_id") == icmsInstance.Field<int>("icms_id")).FirstOrDefault();

                                        detInstance.prod.CST = cstIcmsCol.Contains("CST") ? cstInstance.Field<string>("CST") : cstIcmsCol.Contains("CSOSN") ? cstInstance.Field<string>("CSOSN") : "";
                                    }

                                }
                            }
                        }

                        PingBanco();
                        ConciliarProdutos();

                        // Insere Dados
                        labelCliente.Text = String.Format("Cliente: {0}", GetValueXml("emit", "xNome", ds));
                        labelCNPJ.Text = String.Format("CNPJ: {0}", GetValueXml("emit", "CNPJ", ds));
                        labelDataEmissao.Text = String.Format("Cliente: {0}", DateTime.Parse(GetValueXml("ide", "dhEmi", ds)));
                        labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosNota.Count);

                        labelProdutosNaoConciliados.Text = String.Format("{0} produtos não conciliados.", produtosNota.Where(c => c.STATUS == STATUS.NaoConciliado).Count());
                        labelProdutosValidos.Text = String.Format("{0} produtos Válidos.", produtosNota.Where(c => c.STATUS == STATUS.Valido).Count());
                        labelProdutosInvalidos.Text = String.Format("{0} produtos Inválidos.", produtosNota.Where(c => c.STATUS == STATUS.Invalido).Count());

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

        private List<Nota.det.Prod> ProdutoXmlToObject(IEnumerable<DataRow> produtos)
        {
            var produtosReturn = new List<Nota.det.Prod>();

            foreach (var produto in produtos)
            {
                var produtoInstance = new Nota.det.Prod();
                produtosNota.Add(produtoInstance);

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

        private List<Nota.det.Imposto> ImpostoXmlToObject(IEnumerable<DataRow> impostos)
        {
            var impostoReturn = new List<Nota.det.Imposto>();

            foreach (var imposto in impostos)
            {
                var impostoInstance = new Nota.det.Imposto();
                //produtosNota.Add(produtoInstance);

                foreach (var column in imposto.Table.Columns)
                {
                    var columnObj = ((DataColumn)column);

                    var property = impostoInstance.GetType().GetProperty(columnObj.ColumnName);
                    object columnValue = null;

                    if (property == null)
                        continue;

                    if (property.PropertyType.ToString() == "System.Int")
                        columnValue = imposto.Field<int>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.Decimal")
                        columnValue = decimal.Parse(imposto.Field<string>(columnObj.ColumnName).Replace(".", ","));
                    if (property.PropertyType.ToString() == "System.Double")
                        columnValue = imposto.Field<double>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.String")
                        columnValue = imposto.Field<string>(columnObj.ColumnName);
                    if (property.PropertyType.ToString() == "System.DateTime")
                        columnValue = imposto.Field<DateTime>(columnObj.ColumnName);

                    impostoInstance.GetType().GetProperty(columnObj.ColumnName).SetValue(impostoInstance, columnValue);
                }
                impostoReturn.Add(impostoInstance);
            }

            return impostoReturn;
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

                elipseProdutosNaoConciliados.Visibility = Visibility.Visible;
                labelProdutosNaoConciliados.Visibility = Visibility.Visible;
                elipseProdutosValidos.Visibility = Visibility.Visible;
                labelProdutosValidos.Visibility = Visibility.Visible;
                elipseProdutosInvalidos.Visibility = Visibility.Visible;
                labelProdutosInvalidos.Visibility = Visibility.Visible;

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

                elipseProdutosNaoConciliados.Visibility = Visibility.Hidden;
                labelProdutosNaoConciliados.Visibility = Visibility.Hidden;
                elipseProdutosValidos.Visibility = Visibility.Hidden;
                labelProdutosValidos.Visibility = Visibility.Hidden;
                elipseProdutosInvalidos.Visibility = Visibility.Hidden;
                labelProdutosInvalidos.Visibility = Visibility.Hidden;

                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Visible;

                btnVerResultados.Visibility = Visibility.Hidden;
                btnSalvarProdutos.Visibility = Visibility.Hidden;
            }
            
        }

        private void PingBanco()
        {
            try
            {
                Contexto context = new Contexto();

                produtosBanco = context.Produto.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar no banco: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }

        }

        private void DeletaProdutos()
        {
            try
            {
                Contexto context = new Contexto();
                var produtos = context.Produto.ToList();

                context.Produto.RemoveRange(produtos);

                context.SaveChanges();
                produtosBanco.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao deletar produtos: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }
        }

        private void ConciliarProdutos()
        {
            foreach (var produto in produtosNota)
            {
                var produtoBanco = produtosBanco.Where(c => c.cProd == produto.cProd && c.xProd == produto.xProd).FirstOrDefault();

                if (produtoBanco == null)
                {
                    produto.STATUS = STATUS.NaoConciliado;
                    continue;
                }

                if(produto.NCM != produtoBanco.NCM)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.listaErros.Add(String.Format("NCM - Atual: {0}, Correto: {1}", produto.NCM, produtoBanco.NCM));
                }

                if (produto.CFOP != produtoBanco.CFOP)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.listaErros.Add(String.Format("CFOP - Atual: {0}, Correto: {1}", produto.CFOP, produtoBanco.CFOP));
                }

                if (produto.CEST != produtoBanco.CEST)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.listaErros.Add(String.Format("CEST - Atual: {0}, Correto: {1}", produto.CEST, produtoBanco.CEST));
                }

                if (produto.CST != produtoBanco.CST)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.listaErros.Add(String.Format("CST - Atual: {0}, Correto: {1}", produto.CST, produtoBanco.CST));
                }

                if(produto.listaErros.Count == 0)
                {
                    produto.STATUS = STATUS.Valido;
                }
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
                            db.Produto.Add(new Nota.det.Prod()
                            {
                                cProd = produto.cProd,
                                xProd = produto.xProd,
                                NCM = produto.NCM,
                                CEST = produto.CEST,
                                CFOP = produto.CFOP,
                                CST = produto.CST,
                            });
                        }
                        else
                        {
                            registrosNaoSalvos++;
                        }
                    }

                    registrosSalvos += db.SaveChanges();
                    if(registrosNaoSalvos == 0)
                        MessageBox.Show(String.Format("{0} produtos foram salvos no banco de dados e {1} já estavam salvos!", registrosSalvos, registrosNaoSalvos));
                    else
                        MessageBox.Show(String.Format("{0} produtos foram salvos no banco de dados!", registrosSalvos));
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

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            CadastroDeProdutos cadastroDeProdutos = new CadastroDeProdutos(Content);
            this.Content = cadastroDeProdutos;
        }
    }
}
