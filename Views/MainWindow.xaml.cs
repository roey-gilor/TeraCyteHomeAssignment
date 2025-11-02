using ScottPlot;
using ScottPlot.WPF;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeraCyteHomeAssignment.Models;
using TeraCyteHomeAssignment.ViewModels;

namespace TeraCyteHomeAssignment.Views
{
    /// <summary>
    /// Main application window.
    /// Hosts live image stream, histogram chart, connection status, and history panel.
    /// Wires ViewModel events to UI behaviors and configures ScottPlot visualization.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm; // Bind ViewModel to UI
            // Subscribe to histogram updates published by ViewModel
            _vm.HistogramUpdated += UpdateHistogram;

            ConfigureHistogramPlot();

            // Begin live polling and data flow
            _vm.Start();
        }

        // Configures ScottPlot control styling and axis labels.
        // Called once at window initialization.
        private void ConfigureHistogramPlot()
        {
            HistogramPlot.Plot.Style(
                figureBackground: System.Drawing.Color.Black,
                dataBackground: System.Drawing.Color.Black
            );

            HistogramPlot.Plot.Grid(enable: true);
            HistogramPlot.Plot.XLabel("Intensity Bin");
            HistogramPlot.Plot.YLabel("Count");

            HistogramPlot.Refresh();
        }

        // Updates histogram plot when the ViewModel publishes new histogram data.
        // UI thread ensured via Dispatcher.
        private void UpdateHistogram(int[] bins)
        {
            Dispatcher.Invoke(() =>
            {
                HistogramPlot.Plot.Clear();

                // x-axis: bin index (0-255)
                double[] x = Enumerable.Range(0, bins.Length).Select(i => (double)i).ToArray();

                // y-axis: bin counts
                double[] y = bins.Select(b => (double)b).ToArray();

                var bar = HistogramPlot.Plot.AddBar(y, x);
                bar.BarWidth = 1; // 256 bars tightly packed for grayscale histogram look

                HistogramPlot.Plot.SetAxisLimits(xMin: 0, xMax: 255); // Full histogram range (0-255)
                HistogramPlot.Plot.Margins(0.02, 0.1); // Minimal margins for full-fit compact look
                HistogramPlot.Refresh();
            });
        }

        // Invoked when user clicks a history entry.
        // Opens a detail window showing the stored frame + metadata.
        private void HistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HistoryItem item)
            {
                var win = new HistoryDetailWindow(item);
                win.Show();
            }
        }

        // Clears history list after user confirmation.
        // Prevents accidental deletion of logged results.
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Clear all history?", "Confirm",
        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (DataContext is MainViewModel vm)
                    vm.ClearHistory();
            }
        }

    }
}