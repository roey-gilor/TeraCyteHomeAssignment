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
using TeraCyteHomeAssignment.Models;

namespace TeraCyteHomeAssignment.Views
{
    /// <summary>
    /// Interaction logic for HistoryDetailWindow.xaml
    /// </summary>
    public partial class HistoryDetailWindow : Window
    {
        public HistoryDetailWindow(HistoryItem item)
        {
            InitializeComponent();
            DataContext = item;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
