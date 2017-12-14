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
        
        #endregion

        public Resultados(List<Nota.det.Prod> _produtos)
        {
            InitializeComponent();

            produtos = _produtos;

            dgListagem.ItemsSource = produtos.OrderBy(c => c.STATUS).ToList();
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
    }
}
