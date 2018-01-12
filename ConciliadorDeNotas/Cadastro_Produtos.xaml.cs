using Classes;
using Infraestrutura;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Interaction logic for Cadastro_Produtos.xaml
    /// </summary>
    
    public partial class Cadastro_Produtos : Window
    {
        #region Variáveis
        public enum Modo
        {
            Adicao,
            Edicao,
            Nenhum
        }

        string textoTextBoxAtual = "";
        List<Nota.det.Prod> produtos = new List<Nota.det.Prod>();
        Nota.det.Prod produto = new Nota.det.Prod();
        Modo modoCRUD = Modo.Nenhum;

        #endregion

        public Cadastro_Produtos(List<Nota.det.Prod> _produtos)
        {
            InitializeComponent();

            produtos = _produtos;
            dgListagem.ItemsSource = produtos.OrderBy(c => c.xProd).ToList().ToList();

            dgListagem.SelectedItem = ((List<Nota.det.Prod>)dgListagem.ItemsSource).FirstOrDefault();

            labelQuantidadeProdutos.Text = produtos.Count.ToString() + " produtos encontrados.";
        }

        private void btnVoltar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void GotFocusTextoBox(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textoTextBoxAtual = textBox.ToolTip.ToString();

            if (!string.IsNullOrEmpty(textBox.Text) && textBox.FontStyle == FontStyles.Italic)
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
                textBox.FontStyle = FontStyles.Normal;
            }
        }

        private void LostFocusTextoBox(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = textoTextBoxAtual;
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
                textBox.FontStyle = FontStyles.Italic;
            }
        }

        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            foreach (object control in gridCadastroProdutos.Children)
            {
                if (control is TextBox)
                {
                    var textBox = control as TextBox;
                    textBox.Text = textBox.ToolTip.ToString();
                    textBox.Foreground = new SolidColorBrush(Colors.Gray);
                    textBox.FontStyle = FontStyles.Italic;
                }
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            modoCRUD = Modo.Edicao;

            txtCProd.Text = produto.cProd;
            txtXProd.Text = produto.xProd;
            txtvProd.Text = produto.vProd;
            txtNCM.Text = string.IsNullOrEmpty(produto.NCM) ? txtNCM.ToolTip.ToString(): produto.NCM;
            txtCEST.Text = string.IsNullOrEmpty(produto.NCM) ? txtCEST.ToolTip.ToString() : produto.CEST;
            txtCFOP.Text = string.IsNullOrEmpty(produto.NCM) ? txtCFOP.ToolTip.ToString() : produto.CFOP;
            txtCST.Text = string.IsNullOrEmpty(produto.NCM) ? txtCST.ToolTip.ToString() : produto.CST;
            txtCST_PIS.Text = string.IsNullOrEmpty(produto.NCM) ? txtCST_PIS.ToolTip.ToString() : produto.CST_PIS;
            txtCST_COFINS.Text = string.IsNullOrEmpty(produto.NCM) ? txtCST_COFINS.ToolTip.ToString() : produto.CST_COFINS;

            foreach (object control in gridCadastroProdutos.Children)
            {
                if (control is TextBox)
                {
                    var textBox = control as TextBox;
                    if (textBox.Text != textBox.ToolTip.ToString())
                    {
                        textBox.Foreground = new SolidColorBrush(Colors.Black);
                        textBox.FontStyle = FontStyles.Normal;
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string produtoDescricao = produto.xProd;

                Contexto context = new Contexto();
                //var produtoContext = context.Produto.Where(c => c.xProd == produto.xProd && c.cProd == produto.cProd).FirstOrDefault();
                var produtoContext = context.Produto.Where(c => c.xProd == produto.xProd).FirstOrDefault();

                if (produtoContext != null)
                {
                    context.Produto.Remove(produtoContext);
                }

                //context.Produto.Attach(produto);

                //context.Produto.Remove(produto);
                //context.Entry(produto).State = EntityState.Deleted;

                context.SaveChanges();

                //dgListagem.ItemsSource = ((List<Nota.det.Prod>)dgListagem.ItemsSource).Where(c => c.cProd != produto.cProd && c.xProd != produto.xProd);
                produtos.Remove(produto);
                dgListagem.ItemsSource = new List<Nota.det.Prod>();
                dgListagem.ItemsSource = produtos.OrderBy(c => c.xProd).ToList(); ;

                MessageBox.Show(String.Format("O produto {0} foi removido com sucesso!", produtoDescricao));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao deletar produtos: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            // Variáveis
            int registrosSalvos = 0;

            // Validações
            if (txtCProd.Text == "" || txtCProd.Text == txtCProd.ToolTip.ToString() ||
                txtXProd.Text == "" || txtXProd.Text == txtXProd.ToolTip.ToString() ||
                txtvProd.Text == "" || txtvProd.Text == txtvProd.ToolTip.ToString() ||
                txtNCM.Text == "" || txtNCM.Text == txtNCM.ToolTip.ToString() ||
                txtCEST.Text == "" || txtCEST.Text == txtCEST.ToolTip.ToString() ||
                txtCFOP.Text == "" || txtCFOP.Text == txtCFOP.ToolTip.ToString() ||
                txtCST.Text == "" || txtCST.Text == txtCST.ToolTip.ToString() ||
                txtCST_PIS.Text == "" || txtCST_PIS.Text == txtCST_PIS.ToolTip.ToString() ||
                txtCST_COFINS.Text == "" || txtCST_COFINS.Text == txtCST_COFINS.ToolTip.ToString()
                    )
            {
                MessageBox.Show("Todos os campos são obrigatórios");
                return;
            }

            try
            {
                using (var db = new Contexto())
                {
                    var produtoNoBanco = db.Produto.Where(c => c.xProd == txtXProd.Text).FirstOrDefault();

                    var produtoInstance = new Nota.det.Prod()
                    {
                        cProd = txtCProd.Text,
                        xProd = txtXProd.Text,
                        vProd = txtvProd.Text,
                        NCM = txtNCM.Text,
                        CEST = txtCEST.Text,
                        CFOP = txtCFOP.Text,
                        CST = txtCST.Text,
                        CST_PIS = txtCST_PIS.Text,
                        CST_COFINS = txtCST_COFINS.Text
                    };

                    if (produtoNoBanco == null && modoCRUD.ToString() != Modo.Edicao.ToString())
                    {
                        db.Produto.Add(produtoInstance);

                        produtos.Add(produtoInstance);
                        dgListagem.ItemsSource = new List<Nota.det.Prod>();
                        dgListagem.ItemsSource = produtos.OrderBy(c => c.xProd).ToList();

                        registrosSalvos += db.SaveChanges();
                        MessageBox.Show(String.Format("O produto {0} foi salvo no banco de dados!", txtXProd.Text));
                        btnLimpar_Click(null, null);
                    }
                    else if (produtoNoBanco != null && modoCRUD.ToString() == Modo.Edicao.ToString())
                    {
                        //db.Produto.Attach(produtoNoBanco);
                        //db.Entry(produtoNoBanco).State = EntityState.Modified;

                        produtoInstance.Id = produtoNoBanco.Id;
                        db.Entry(produtoNoBanco).CurrentValues.SetValues(produtoInstance);

                        registrosSalvos += db.SaveChanges();

                        produtos.Remove(produto);
                        produtos.Add(produtoInstance);

                        MessageBox.Show(String.Format("O produto {0} foi salvo no banco de dados!", txtXProd.Text));
                        btnLimpar_Click(null, null);

                        dgListagem.ItemsSource = new List<Nota.det.Prod>();
                        dgListagem.ItemsSource = produtos.OrderBy(c => c.xProd).ToList();
                    }
                    else
                    {
                        if(modoCRUD.ToString() != Modo.Edicao.ToString())
                            MessageBox.Show(String.Format("O produto {0} já existe no banco de dados", txtXProd.Text));
                        else if(modoCRUD.ToString() == Modo.Edicao.ToString())
                            MessageBox.Show(String.Format("O produto {0} já existe não foi encontrado no banco de dados", txtXProd.Text));
                        return;
                    }

                    modoCRUD = Modo.Nenhum;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar produto: " + ex.Message + "\nInner: " + ex?.InnerException?.Message);
            }
        }

        private void dgListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            produto = (Nota.det.Prod)dgListagem.SelectedItem;
        }

        private void txtCST_COFINS_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //btnSalvar_Click(null, null);
            }
        }
    }
}
