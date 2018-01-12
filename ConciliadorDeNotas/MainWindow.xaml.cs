using Classes;
using Enums;
using Infraestrutura;
using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

        string fileName = "";
        string fileExtension = "";
        string temp = "";
        List<string> files = new List<string>();
        string xmlsComErro = "";
        int countFilesComErro = 0;

        string fileNameTxt = "";
        string fileExtensionTxt = "";
        List<string> filesTxt = new List<string>();
        int countFilesComErroTxt = 0;

        Thread threadPrincipal;

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
            files.Clear();
            countFilesComErro = 0;
            ds = new DataSet();

            fileDialog.Multiselect = false;
            fileDialog.Filter = "Arquivos XML/ZIP (*.xml;*.zip)|*.xml;*.zip|Todos Arquivos (*.*)|*.*";

            try
            {
                if (fileDialog.ShowDialog() == true)
                {

                    try
                    {
                        fileName = fileDialog.FileName;
                        fileExtension = fileName.Substring(fileName.Length - 4, 4);
                        if (fileExtension != ".xml" && fileExtension != ".zip")
                        {
                            MessageBox.Show("Selecione um arquivo válido com a extensão XML ou ZIP.");
                            return;
                        }


                        if (fileExtension == ".zip")
                        {
                            temp = System.IO.Path.GetTempPath();
                            temp = temp + "zipXML";

                            if (Directory.Exists(temp))
                            {
                                Directory.Delete(temp, true);
                            }
                            Directory.CreateDirectory(temp);

                            if (File.Exists(fileName))
                            {
                                using (ZipFile zip = new ZipFile(fileName))
                                {
                                    if (Directory.Exists(temp))
                                    {
                                        try
                                        {
                                            zip.ExtractAll(temp);

                                            foreach (var entri in zip.EntryFileNames)
                                            {
                                                files.Add(temp + "\\" + entri);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("Erro ao extrair ZIP: " + ex.Message);
                                            Directory.Delete(temp, true);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("O diretório destino não foi criado, contato o suporte.");
                                        Directory.Delete(temp, true);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("O Arquivo ZIP não foi encontrado.");
                                Directory.Delete(temp, true);
                                return;
                            }
                        }
                        else
                        {
                            files.Add(fileName);
                        }

                        // Zera ProgressBar
                        labelDadosResultadoAnaliseMensagem.Text = "Carregando..";

                        progress.Value = 0;
                        progress.Maximum = files.Count;
                        progress.Visibility = Visibility.Visible;
                        labelProgress.Visibility = Visibility.Visible;

                        threadPrincipal = new Thread(ProcessaXML);
                        threadPrincipal.Start();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n Erro na leitura do Arquivo: " + fileName);
                        try
                        {
                            Directory.Delete(temp, true);
                        }
                        catch { }

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

        private void ProcessaXML()
        {
            foreach (var file in files)
            {
                string Extension = file.Substring(file.Length - 4, 4);
                if (Extension != ".xml")
                {
                    countFilesComErro++;

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progress.Value++;
                        labelProgress.Text = progress.Value + "/" + progress.Maximum;
                    }));
                    continue;
                }

                try
                {
                    //ds.ReadXml(fileName);
                    ds = new DataSet();
                    ds.ReadXml(file);

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progress.Value++;
                        labelProgress.Text = progress.Value + "/" + progress.Maximum;
                    }));
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("XML Inválido, selecione um xml de nota válido!\n\n" + "Execeção interna: " + ex.Message);
                    if (xmlsComErro != "")
                        xmlsComErro += "\n";

                    countFilesComErro++;
                    xmlsComErro += "Xml Inválido: " + file.Split('\\').Last();

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progress.Value++;
                        labelProgress.Text = progress.Value + "/" + progress.Maximum;
                    }));
                    continue;
                    //return;
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

                if (det == null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("XML Inválido, este xml não é uma nota ou não contém nenhum produto.");
                    }));
                    continue;
                    //return;
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
                    try
                    {
                        var detInstance = new Nota.det();

                        var produtoInstance = prod.AsEnumerable().Where(c => c.Field<int>("det_id") == detProduto.det.Field<int>("det_id"));
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

                                    if (cstInstance == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (cstInstance != null)
                                        {
                                            detInstance.prod.CST = cstIcmsCol.Contains("CST") ? cstInstance.Field<string>("CST") : cstIcmsCol.Contains("CSOSN") ? cstInstance.Field<string>("CSOSN") : "";
                                        }
                                        break;
                                    }
                                }

                            }

                            // CST PIS 
                            DataTable pis = !ds.Tables.Contains("PIS") ? null : ds.Tables["PIS"];
                            DataColumnCollection pisCol = pis != null ? pis.Columns : null;

                            var pisInstance = pis.AsEnumerable().Where(c => c.Field<int>("imposto_id") == impostoInstance.FirstOrDefault().Field<int>("imposto_id")).FirstOrDefault();

                            if (pisInstance != null)
                            {
                                DataTable PisAliq = ds.Tables["PISAliq"];
                                DataColumnCollection PisAliqCol = PisAliq != null ? PisAliq.Columns : null;

                                if (PisAliq != null)
                                {
                                    var PisAliqInstance = PisAliq.AsEnumerable().Where(c => c.Field<int>("pis_id") == pisInstance.Field<int>("pis_id")).FirstOrDefault();

                                    detInstance.prod.CST_PIS = PisAliqCol.Contains("CST") ? PisAliqInstance.Field<string>("CST") : "";
                                }
                                else
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

                        var produtoExiste = produtosNota.Where(c => c.xProd == detInstance.prod.xProd);
                        if (produtoExiste.Count() == 0)
                        {
                            nota.detList.Add(detInstance);
                            produtosNota.Add(detInstance.prod);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            try
            {
                Directory.Delete(temp, true);
            }
            catch { }

            PingBanco();
            ConciliarProdutos();

            //if (xmlsComErro != "")
            //{
            //    MessageBox.Show(xmlsComErro);
            //}

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Insere Dados
                labelCliente.Text = String.Format("Notas encontradas: {0}", files.Count);
                labelCNPJ.Text = String.Format("Notas inválidas: {0}", countFilesComErro);
                labelDataEmissao.Text = String.Format("Notas válidas: {0}", files.Count - countFilesComErro);
                labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosNota.Count);

                labelProdutosNaoConciliados.Text = String.Format("{0} produtos não conciliados.", produtosNota.Where(c => c.STATUS == STATUS.NaoConciliado).Count());
                labelProdutosValidos.Text = String.Format("{0} produtos Válidos.", produtosNota.Where(c => c.STATUS == STATUS.Valido).Count());
                labelProdutosInvalidos.Text = String.Format("{0} produtos Inválidos.", produtosNota.Where(c => c.STATUS == STATUS.Invalido).Count());

                // Exibe Dados
                ExibeComponentsDados(true);
            }));
        }

        private void btnTXT_Click(object sender, RoutedEventArgs e)
        {
            importacao = IMPORTACAO.TXT;
            produtosTxt.Clear();
            filesTxt.Clear();
            linhasTXT.Clear();
            countFilesComErroTxt = 0;

            fileDialog.Multiselect = false;
            fileDialog.Filter = "Todos Arquivos (*.*)|*.*";
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    fileNameTxt = fileDialog.FileName;
                    fileExtensionTxt = fileNameTxt.Substring(fileNameTxt.Length - 4, 4);

                    if (fileExtensionTxt.Contains("F") && fileExtensionTxt != ".zip")
                    {
                        MessageBox.Show("Selecione um arquivo válido.");
                        return;
                    }

                    #region ZIP
                    if (fileExtensionTxt == ".zip")
                    {
                        temp = System.IO.Path.GetTempPath();
                        temp = temp + "zipTXT";

                        if (Directory.Exists(temp))
                        {
                            Directory.Delete(temp, true);
                        }
                        Directory.CreateDirectory(temp);

                        if (File.Exists(fileNameTxt))
                        {
                            using (ZipFile zip = new ZipFile(fileNameTxt))
                            {
                                if (Directory.Exists(temp))
                                {
                                    try
                                    {
                                        zip.ExtractAll(temp);

                                        foreach (var entri in zip.EntryFileNames)
                                        {
                                            filesTxt.Add(temp + "\\" + entri);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Erro ao extrair ZIP do TXT: " + ex.Message);
                                        Directory.Delete(temp, true);
                                        return;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("O diretório destino não foi criado, contato o suporte.");
                                    Directory.Delete(temp, true);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("O Arquivo ZIP não foi encontrado.");
                            Directory.Delete(temp, true);
                            return;
                        }
                    }
                    else
                    {
                        filesTxt.Add(fileNameTxt);
                    }
                    #endregion

                    // Zera ProgressBar
                    labelDadosResultadoAnaliseMensagem.Text = "Carregando..";

                    progress.Value = 0;
                    progress.Maximum = filesTxt.Count;
                    progress.Visibility = Visibility.Visible;
                    labelProgress.Visibility = Visibility.Visible;

                    threadPrincipal = new Thread(ProcessaTXT);
                    threadPrincipal.Start();

                    #region TXT Unique
                    //try
                    //{
                    //    Stream file = fileDialog.OpenFile();
                    //    StreamReader fileReader = new StreamReader(file);

                    //    while (fileReader.EndOfStream == false)
                    //    {
                    //        linhasTXT.Add(fileReader.ReadLine());
                    //    }

                    //    linhasTXT = linhasTXT.Where(c => c.Contains("E15")).ToList();

                    //    produtosTxt.Clear();

                    //    foreach (var linha in linhasTXT)
                    //    {
                    //        string cProd = linha.Substring(61, 14).Trim(' ').ToString();
                    //        string xProd = linha.Substring(75, 100).TrimEnd(' ').ToString();
                    //        string vProd = linha.Substring(185, 8).Trim(' ').ToString();

                    //        vProd = vProd.Substring(0, 6).TrimStart('0') + "." + vProd.Substring(6, 2);

                    //        var produtoInstance = new Nota.det.Prod()
                    //        {
                    //            cProd = cProd,
                    //            xProd = xProd,
                    //            vProd = vProd,
                    //            STATUS = STATUS.Valido
                    //        };
                    //        var produtoExiste = produtosTxt.Where(c => c.xProd == produtoInstance.xProd); //c.cProd == produtoInstance.cProd && c.vProd == produtoInstance.vProd
                    //        if (produtoExiste.Count() == 0)
                    //            produtosTxt.Add(produtoInstance);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    //MessageBox.Show("Arquivo Inválido, selecione um arquivo válido!\n\n" + "Execeção interna: " + ex.Message);
                    //    //return;
                    //    countFilesComErroTxt++;
                    //}


                    //// Insere Dados
                    ////labelCliente.Text = String.Format("Cliente: {0}", GetValueXml("emit", "xNome", ds));
                    ////labelCNPJ.Text = String.Format("CNPJ: {0}", GetValueXml("emit", "CNPJ", ds));
                    ////labelDataEmissao.Text = String.Format("Cliente: {0}", DateTime.Parse(GetValueXml("ide", "dhEmi", ds)));
                    //labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosTxt.Count);
                    //labelProdutos.Visibility = Visibility.Visible;

                    ////labelProdutosNaoConciliados.Text = String.Format("{0} produtos não conciliados.", produtosNota.Where(c => c.STATUS == STATUS.NaoConciliado).Count());
                    //labelProdutosValidos.Text = String.Format("{0} produtos encontrados.", produtosTxt.Count);
                    //labelProdutosValidos.Visibility = Visibility.Visible;
                    //elipseProdutosValidos.Visibility = Visibility.Visible;

                    //groupBoxDados.Visibility = Visibility.Visible;
                    //labelAnaliseFinalizada.Visibility = Visibility.Visible;
                    //btnSalvarProdutos.Visibility = Visibility.Visible;
                    //labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Collapsed;

                    //btnVerResultados.Visibility = Visibility.Visible;
                    //labelProdutosInvalidos.Text = String.Format("{0} produtos Inválidos.", produtosNota.Where(c => c.STATUS == STATUS.Invalido).Count());

                    // Exibe Dados
                    //ExibeComponentsDados(true);
                    #endregion

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n Erro na leitura do arquivo: " + fileName);
                }
            }
        }

        private void ProcessaTXT()
        {
            #region Processa Arquivos TXT

            foreach (var file in filesTxt)
            {
                try
                {
                    string Extension = file.Substring(file.Length - 4, 4);
                    if (!Extension.Contains("F"))
                    {
                        countFilesComErroTxt++;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progress.Value++;
                            labelProgress.Text = progress.Value + "/" + progress.Maximum;
                        }));
                        continue;
                    }

                    Stream fileStream = File.Open(file, FileMode.Open);
                    StreamReader fileReader = new StreamReader(fileStream);

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progress.Value++;
                        labelProgress.Text = progress.Value + "/" + progress.Maximum;
                    }));

                    while (fileReader.EndOfStream == false)
                    {
                        linhasTXT.Add(fileReader.ReadLine());
                    }

                    // Fecha arquivo
                    fileStream.Dispose();

                    linhasTXT = linhasTXT.Where(c => c.Contains("E15")).ToList();

                    produtosTxt.Clear();

                    foreach (var linha in linhasTXT)
                    {
                        string cProd = linha.Substring(61, 14).Trim(' ').ToString();
                        string xProd = linha.Substring(75, 100).TrimEnd(' ').ToString();
                        string vProd = linha.Substring(185, 8).Trim(' ').ToString();

                        vProd = vProd.Substring(0, 6).TrimStart('0') + "." + vProd.Substring(6, 2);

                        decimal vProdDecimal = decimal.Parse(vProd.Replace(".", ","));

                        var produtoInstance = new Nota.det.Prod()
                        {
                            cProd = cProd,
                            xProd = xProd,
                            vProd = vProdDecimal.ToString("#,###,##0.00"),
                            STATUS = STATUS.Valido,
                            CorStatus = "#FF3AAE3A",
                            Visibility = Visibility.Collapsed
                    };
                        var produtoExiste = produtosTxt.Where(c => c.xProd == produtoInstance.xProd); //c.cProd == produtoInstance.cProd && c.vProd == produtoInstance.vProd
                        if (produtoExiste.Count() == 0)
                            produtosTxt.Add(produtoInstance);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Arquivo Inválido, selecione um arquivo válido!\n\n" + "Execeção interna: " + ex.Message);
                    //return;
                    countFilesComErroTxt++;
                }
            }

            try
            {
                Directory.Delete(temp, true);
            }
            catch { }


            // Insere Dados
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                labelCliente.Text = String.Format("Arquivos encontradas: {0}", filesTxt.Count);
                labelCNPJ.Text = String.Format("Arquivos inválidos: {0}", countFilesComErroTxt);
                labelDataEmissao.Text = String.Format("Arquivos válidos: {0}", filesTxt.Count - countFilesComErroTxt);
                labelProdutos.Text = String.Format("Produtos Encontrados: {0}", produtosTxt.Count);

                labelCliente.Visibility = Visibility.Visible;
                labelCNPJ.Visibility = Visibility.Visible;
                labelDataEmissao.Visibility = Visibility.Visible;
                labelProdutos.Visibility = Visibility.Visible;

                //labelProdutosNaoConciliados.Text = String.Format("{0} produtos não conciliados.", produtosNota.Where(c => c.STATUS == STATUS.NaoConciliado).Count());
                labelProdutosValidos.Text = String.Format("{0} produtos encontrados.", produtosTxt.Count);
                labelProdutosValidos.Visibility = Visibility.Visible;
                elipseProdutosValidos.Visibility = Visibility.Visible;

                groupBoxDados.Visibility = Visibility.Visible;
                labelAnaliseFinalizada.Visibility = Visibility.Visible;
                btnSalvarProdutos.Visibility = Visibility.Visible;
                labelDadosResultadoAnaliseMensagem.Visibility = Visibility.Collapsed;

                btnVerResultados.Visibility = Visibility.Visible;
            }));
            //labelProdutosInvalidos.Text = String.Format("{0} produtos Inválidos.", produtosNota.Where(c => c.STATUS == STATUS.Invalido).Count());

            // Exibe Dados
            //ExibeComponentsDados(true);
            #endregion
        }

        private List<Nota.det.Prod> ProdutoXmlToObject(IEnumerable<DataRow> produtos)
        {
            var produtosReturn = new List<Nota.det.Prod>();

            foreach (var produto in produtos)
            {
                var produtoInstance = new Nota.det.Prod();
                //produtosNota.Add(produtoInstance);

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

                labelProgress.Visibility = Visibility.Visible;
                progress.Visibility = Visibility.Visible;
            }
            else
            {
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

                labelProgress.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Collapsed;
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
                //var produtoBanco = produtosBanco.Where(c => c.cProd == produto.cProd && c.xProd == produto.xProd).FirstOrDefault();
                var produtoBanco = produtosBanco.Where(c => c.xProd == produto.xProd).FirstOrDefault();

                if (produtoBanco == null)
                {
                    produto.STATUS = STATUS.NaoConciliado;
                    produto.CorStatus = "#FF008BFF";
                    produto.Visibility = Visibility.Collapsed;

                    continue;
                }

                if (produto.NCM != produtoBanco.NCM)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";
                    produto.Visibility = Visibility.Visible;

                    produto.listaErros.Add(new Nota.det.Prod.Error()
                    {
                        Message = String.Format("NCM - XML: {0}, Banco: {1}",
                            string.IsNullOrEmpty(produto.NCM) ? "Vazio" : produto.NCM,
                            string.IsNullOrEmpty(produtoBanco.NCM) ? "Vazio" : produtoBanco.NCM)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CFOP != produtoBanco.CFOP)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";
                    produto.Visibility = Visibility.Visible;

                    produto.listaErros.Add(new Nota.det.Prod.Error()
                    {
                        Message = String.Format("CFOP - XML: {0}, Banco: {1}",
                            string.IsNullOrEmpty(produto.CFOP) ? "Vazio" : produto.CFOP,
                            string.IsNullOrEmpty(produtoBanco.CFOP) ? "Vazio" : produtoBanco.CFOP)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CEST != produtoBanco.CEST)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";
                    produto.Visibility = Visibility.Visible;

                    produto.listaErros.Add(new Nota.det.Prod.Error()
                    {
                        Message = String.Format("CEST - XML: {0}, Banco: {1}",
                            string.IsNullOrEmpty(produto.CEST) ? "Vazio" : produto.CEST,
                             string.IsNullOrEmpty(produtoBanco.CEST) ? "Vazio" : produtoBanco.CEST)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CST_PIS != produtoBanco.CST_PIS)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";
                    produto.Visibility = Visibility.Visible;

                    produto.listaErros.Add(new Nota.det.Prod.Error()
                    {
                        Message = String.Format("CST_PIS - XML: {0}, Banco: {1}",
                            string.IsNullOrEmpty(produto.CST_PIS) ? "Vazio" : produto.CST_PIS,
                            string.IsNullOrEmpty(produtoBanco.CST_PIS) ? "Vazio" : produtoBanco.CST_PIS)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.CST_COFINS != produtoBanco.CST_COFINS)
                {
                    produto.STATUS = STATUS.Invalido;
                    produto.CorStatus = "#FFDC5F5F";
                    produto.Visibility = Visibility.Visible;

                    produto.listaErros.Add(new Nota.det.Prod.Error()
                    {
                        Message = String.Format("CST_COFINS - XML: {0}, Banco: {1}",
                            string.IsNullOrEmpty(produto.CST_COFINS) ? "Vazio" : produto.CST_COFINS,
                            string.IsNullOrEmpty(produtoBanco.CST_COFINS) ? "Vazio" : produtoBanco.CST_COFINS)
                    });
                    produto.quantidadeErros++;
                }

                if (produto.listaErros.Count == 0)
                {
                    produto.STATUS = STATUS.Valido;
                    produto.CorStatus = "#FF3AAE3A";
                    produto.Visibility = Visibility.Collapsed;
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
                        var produtoNoBanco = db.Produto.Where(c => c.xProd == produto.xProd).FirstOrDefault();

                        if (produtoNoBanco == null)
                        {
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
                    if (registrosNaoSalvos != 0)
                        MessageBox.Show(String.Format("{0} produtos(s) foram salvos no banco de dados e {1} já estavam salvos!", registrosSalvos, registrosNaoSalvos));
                    else
                        MessageBox.Show(String.Format("{0} produto(s) foram salvos no banco de dados!", registrosSalvos));
                }
                PingBanco();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar os produtos: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
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

        private void btnVerResultados_Click(object sender, RoutedEventArgs e)
        {
            var produtosASeremExibidos = importacao == IMPORTACAO.XML ? produtosNota : produtosTxt;

            Resultados result = new Resultados(produtosASeremExibidos);
            result.Show();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            btnTXT_Click(null, null);
        }
    }
}
