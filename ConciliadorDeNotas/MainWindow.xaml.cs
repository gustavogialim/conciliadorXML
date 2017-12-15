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
        List<Nota.det.Prod> produtosTxt = new List<Nota.det.Prod>();
        List<Nota.det.Prod> produtosBanco = new List<Nota.det.Prod>();
        string[] cCstIcms = { "ICMS00", "ICMS10", "ICMS20", "ICMS30", "ICMS40", "ICMS51", "ICMS60", "ICMS70", "ICMS90", "ICMSSN101", "ICMSSN102", "ICMSSN201", "ICMSSN202", "ICMSSN500", "ICMSSN900" };
        List<string> linhasTXT = new List<string>();
        IMPORTACAO importacao = IMPORTACAO.XML;

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
            importacao = IMPORTACAO.XML;
            produtosNota.Clear();
            ds = new DataSet();

            fileDialog.Multiselect = false;
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

                        try
                        {
                            ds.ReadXml(fileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("XML Inválido, selecione um xml de nota válido!\n\n" + "Execeção interna: " + ex.Message);
                            return;
                        }

                        // Det
                        DataTable det = !ds.Tables.Contains("det") ? null : ds.Tables["det"];
                        DataColumnCollection detCol = det != null ? det.Columns : null;

                        // Produto
                        DataTable prod = !ds.Tables.Contains("prod") ? null : ds.Tables["prod"];
                        DataColumnCollection prodCol = prod != null ? prod.Columns : null;

                        // Imposto
                        DataTable imposto = !ds.Tables.Contains("imposto") ? null : ds.Tables["imposto"];
                        DataColumnCollection impostoCol = imposto != null ? imposto.Columns : null;

                        if (det == null) {
                            MessageBox.Show("XML Inválido, este xml não é uma nota ou não contém nenhum produto.");
                            return;
                        }

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
                                    DataColumnCollection icmsCol = icms != null ? icms.Columns : null;

                                    var icmsInstance = icms.AsEnumerable().Where(c => c.Field<int>("imposto_id") == impostoInstance.FirstOrDefault().Field<int>("imposto_id")).FirstOrDefault();

                                    if (ds.Tables.Contains(item))
                                    {
                                        DataTable cstIcms = ds.Tables[item];
                                        DataColumnCollection cstIcmsCol = cstIcms != null ? cstIcms.Columns : null;

                                        var cstInstance = cstIcms.AsEnumerable().Where(c => c.Field<int>("icms_id") == icmsInstance.Field<int>("icms_id")).FirstOrDefault();

                                        detInstance.prod.CST = cstIcmsCol.Contains("CST") ? cstInstance.Field<string>("CST") : cstIcmsCol.Contains("CSOSN") ? cstInstance.Field<string>("CSOSN") : "";
                                    }

                                }

                                // CST PIS 
                                DataTable pis = !ds.Tables.Contains("PIS") ? null : ds.Tables["PIS"];
                                DataColumnCollection pisCol = pis != null ? pis.Columns : null;

                                var pisInstance = pis.AsEnumerable().Where(c => c.Field<int>("imposto_id") == impostoInstance.FirstOrDefault().Field<int>("imposto_id")).FirstOrDefault();

                                if(pisInstance != null)
                                {
                                    DataTable PisAliq = ds.Tables["PISAliq"];
                                    DataColumnCollection PisAliqCol = PisAliq != null ? PisAliq.Columns : null;

                                    if(PisAliq != null)
                                    {
                                        var PisAliqInstance = PisAliq.AsEnumerable().Where(c => c.Field<int>("pis_id") == pisInstance.Field<int>("pis_id")).FirstOrDefault();

                                        detInstance.prod.CST_PIS = PisAliqCol.Contains("CST") ? PisAliqInstance.Field<string>("CST") : "";
                                    }else
                                    {
                                        DataTable PisOutr = ds.Tables["PisOutr"];
                                        DataColumnCollection PisOutrCol = PisOutr != null ? PisOutr.Columns : null;

                                        var PisAliqInstance = PisOutr.AsEnumerable().Where(c => c.Field<int>("pis_id") == pisInstance.Field<int>("pis_id")).FirstOrDefault();

                                        detInstance.prod.CST_PIS = PisOutrCol.Contains("CST") ? PisAliqInstance.Field<string>("CST") : "";
                                    }
                                }

                                // CST COFINS 
                                DataTable cofins = !ds.Tables.Contains("COFINS") ? null : ds.Tables["COFINS"];
                                DataColumnCollection cofinsCol = pis != null ? pis.Columns : null;

                                var confinsInstance = cofins.AsEnumerable().Where(c => c.Field<int>("imposto_id") == impostoInstance.FirstOrDefault().Field<int>("imposto_id")).FirstOrDefault();

                                if (confinsInstance != null)
                                {
                                    DataTable CofinsAliq = ds.Tables["COFINSAliq"];
                                    DataColumnCollection CofinsAliqCol = CofinsAliq != null ? CofinsAliq.Columns : null;

                                    if (CofinsAliq != null)
                                    {
                                        var CofinsAliqInstance = CofinsAliq.AsEnumerable().Where(c => c.Field<int>("cofins_id") == confinsInstance.Field<int>("cofins_id")).FirstOrDefault();

                                        detInstance.prod.CST_COFINS = CofinsAliqCol.Contains("CST") ? CofinsAliqInstance.Field<string>("CST") : "";
                                    }
                                    else
                                    {
                                        DataTable CofinsOutr = ds.Tables["CofinsOutr"];
                                        DataColumnCollection CofinsOutrCol = CofinsOutr != null ? CofinsOutr.Columns : null;

                                        var CofinsAliqInstance = CofinsOutr.AsEnumerable().Where(c => c.Field<int>("cofins_id") == confinsInstance.Field<int>("cofins_id")).FirstOrDefault();

                                        detInstance.prod.CST_COFINS = CofinsOutrCol.Contains("CST") ? CofinsAliqInstance.Field<string>("CST") : "";
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
                        MessageBox.Show(ex.Message + "\n Erro na leitura do XML: " + fileName);
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
            importacao = IMPORTACAO.TXT;

            fileDialog.Multiselect = false;
            fileDialog.Filter = "Todos Arquivos (*.*)|*.*";
            string fileName = "";
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    fileName = fileDialog.FileName;
                    if (fileName.Substring(fileName.Length - 4, 4).Contains(".F"))
                    {
                        MessageBox.Show("Selecione um arquivo válido.");
                        return;
                    }

                    try
                    {
                        Stream file = fileDialog.OpenFile();
                        StreamReader fileReader = new StreamReader(file);

                        while (fileReader.EndOfStream == false)
                        {
                            linhasTXT.Add(fileReader.ReadLine());
                        }

                        linhasTXT = linhasTXT.Where(c => c.Contains("E15")).ToList();

                        produtosTxt.Clear();

                        foreach (var linha in linhasTXT)
                        {
                            string cProd = linha.Substring(61, 14).Trim(' ').ToString();
                            string xProd = linha.Substring(75, 100).TrimEnd(' ').ToString();
                            string vProd = linha.Substring(185, 8).Trim(' ').ToString();

                            vProd = vProd.Substring(0, 6).TrimStart('0') + "." + vProd.Substring(6,2);

                            var produtoInstance = new Nota.det.Prod()
                            {
                                cProd = cProd,
                                xProd = xProd,
                                vProd = vProd
                            };
                            var produtoExiste = produtosTxt.Where(c => c.cProd == produtoInstance.cProd && c.xProd == produtoInstance.xProd && c.vProd == produtoInstance.vProd);
                            if (produtoExiste.Count() == 0)
                                produtosTxt.Add(produtoInstance);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Arquivo Inválido, selecione um arquivo válido!\n\n" + "Execeção interna: " + ex.Message);
                        return;
                    }


                    // Insere Dados
                    //labelCliente.Text = String.Format("Cliente: {0}", GetValueXml("emit", "xNome", ds));
                    //labelCNPJ.Text = String.Format("CNPJ: {0}", GetValueXml("emit", "CNPJ", ds));
                    //labelDataEmissao.Text = String.Format("Cliente: {0}", DateTime.Parse(GetValueXml("ide", "dhEmi", ds)));
                    labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosTxt.Count);
                    labelProdutos.Visibility = Visibility.Visible;

                    //labelProdutosNaoConciliados.Text = String.Format("{0} produtos não conciliados.", produtosNota.Where(c => c.STATUS == STATUS.NaoConciliado).Count());
                    labelProdutosValidos.Text = String.Format("{0} produtos encontrados.", produtosTxt.Count);
                    labelProdutosValidos.Visibility = Visibility.Visible;
                    elipseProdutosValidos.Visibility = Visibility.Visible;

                    groupBoxDados.Visibility = Visibility.Visible;
                    labelAnaliseFinalizada.Visibility = Visibility.Visible;
                    btnSalvarProdutos.Visibility = Visibility.Visible;
                    labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Collapsed;

                    //labelProdutosInvalidos.Text = String.Format("{0} produtos Inválidos.", produtosNota.Where(c => c.STATUS == STATUS.Invalido).Count());

                    // Exibe Dados
                    //ExibeComponentsDados(true);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n Erro na leitura do arquivo: " + fileName);
                }
            }
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

                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Collapsed;

                btnVerResultados.Visibility = Visibility.Visible;
                btnSalvarProdutos.Visibility = Visibility.Visible;
            }
            else {
                groupBoxDados.Visibility = Visibility.Collapsed;
                labelCliente.Visibility = Visibility.Collapsed;
                labelCNPJ.Visibility = Visibility.Collapsed;
                labelDataEmissao.Visibility = Visibility.Collapsed;
                labelProdutos.Visibility = Visibility.Collapsed;

                labelAnaliseFinalizada.Visibility = Visibility.Collapsed;

                elipseProdutosNaoConciliados.Visibility = Visibility.Collapsed;
                labelProdutosNaoConciliados.Visibility = Visibility.Collapsed;
                elipseProdutosValidos.Visibility = Visibility.Collapsed;
                labelProdutosValidos.Visibility = Visibility.Collapsed;
                elipseProdutosInvalidos.Visibility = Visibility.Collapsed;
                labelProdutosInvalidos.Visibility = Visibility.Collapsed;

                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Visible;

                btnVerResultados.Visibility = Visibility.Collapsed;
                btnSalvarProdutos.Visibility = Visibility.Collapsed;
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
                    produto.CorStatus = "#FF008BFF";

                    continue;
                }

                if(produto.NCM != produtoBanco.NCM)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";

                    produto.listaErros.Add(new Nota.det.Prod.Error() {
                        Message = String.Format("NCM - Atual: {0}, Correto: {1}", produto.NCM, produtoBanco.NCM)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CFOP != produtoBanco.CFOP)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";

                    produto.listaErros.Add(new Nota.det.Prod.Error() {
                        Message = String.Format("CFOP - Atual: {0}, Correto: {1}", produto.CFOP, produtoBanco.CFOP) }
                    );
                    produto.quantidadeErros++;
                }

                if (produto.CEST != produtoBanco.CEST)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";

                    produto.listaErros.Add(new Nota.det.Prod.Error() {
                        Message = String.Format("CEST - Atual: {0}, Correto: {1}", produto.CEST, produtoBanco.CEST)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CST_PIS != produtoBanco.CST_PIS)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";

                    produto.listaErros.Add(new Nota.det.Prod.Error() {
                        Message = String.Format("CST_PIS - Atual: {0}, Correto: {1}", produto.CST_PIS, produtoBanco.CST_PIS)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CST_COFINS != produtoBanco.CST_COFINS)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";

                    produto.listaErros.Add(new Nota.det.Prod.Error() {
                        Message = String.Format("CST_COFINS - Atual: {0}, Correto: {1}", produto.CST_COFINS, produtoBanco.CST_COFINS)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.listaErros.Count == 0)
                {
                    produto.STATUS = STATUS.Valido;
                    produto.CorStatus = "#FF3AAE3A";
                }
            }
        }

        private void btnSalvarProdutos_Click(object sender, RoutedEventArgs e)
        {
            int registrosSalvos = 0;
            int registrosNaoSalvos = 0;
            var produtosASerSalvos = importacao == IMPORTACAO.XML ? produtosNota : produtosTxt;

            try
            {
                using (var db = new Contexto())
                {
                    foreach (var produto in produtosASerSalvos)
                    {
                        var produtoNoBanco = db.Produto.Where(c => c.cProd == produto.cProd && c.xProd == produto.xProd).FirstOrDefault();

                        if(produtoNoBanco == null) {
                            db.Produto.Add(new Nota.det.Prod()
                            {
                                cProd = produto.cProd,
                                xProd = produto.xProd,
                                vProd = produto.vProd,
                                NCM = produto.NCM,
                                CEST = produto.CEST,
                                CFOP = produto.CFOP,
                                CST = produto.CST,
                                CST_PIS = produto.CST_PIS,
                                CST_COFINS = produto.CST_COFINS
                            });
                            produtosBanco.Add(produto);
                        }
                        else
                        {
                            registrosNaoSalvos++;
                        }
                    }

                    registrosSalvos += db.SaveChanges();
                    if(registrosNaoSalvos != 0)
                        MessageBox.Show(String.Format("{0} produtos(s) foram salvos no banco de dados e {1} já estavam salvos!", registrosSalvos, registrosNaoSalvos));
                    else
                        MessageBox.Show(String.Format("{0} produto(s) foram salvos no banco de dados!", registrosSalvos));
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
            Cadastro_Produtos cad = new Cadastro_Produtos(produtosBanco);
            cad.ShowDialog();

            cad.Closed += new EventHandler(Cadastro_Produtos_Close);
        }

        private void Cadastro_Produtos_Close(object sender, EventArgs e)
        {
            PingBanco();
        }

        private void btnVoltar_Click(object sender, RoutedEventArgs e)
        {
            Resultados result = new Resultados(produtosNota);
            result.Show();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            btnTXT_Click(null, null);
        }
    }
}
