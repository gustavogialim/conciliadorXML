using Classes;
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
    /// Interaction logic for Cadastro_Produtos.xaml
    /// </summary>
    
    public partial class Cadastro_Produtos : Window
    {
        #region Variáveis

        string textoTextBoxAtual = "";
        List<Nota.det.Prod> produtos = new List<Nota.det.Prod>();

        #endregion

        public Cadastro_Produtos(List<Nota.det.Prod> _produtos)
        {
            InitializeComponent();

            produtos = _produtos;
            dgListagem.ItemsSource = produtos;
        }

        private void btnVoltar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void GotFocusTextoBox(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textoTextBoxAtual = textBox.Text;

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
    }
}
