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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConciliadorDeNotas
{
    /// <summary>
    /// Interaction logic for CadastroDeProdutos.xaml
    /// </summary>
    public partial class CadastroDeProdutos : Page
    {
        object aa;
        public CadastroDeProdutos(object c)
        {
            InitializeComponent();

            aa = c;
        }

        private void btnVoltar_Click(object sender, RoutedEventArgs e)
        {
            this.Content = aa;
        }
    }
}
