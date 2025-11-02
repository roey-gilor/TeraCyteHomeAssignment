using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;
using TeraCyteHomeAssignment.Helpers;
using TeraCyteHomeAssignment.Models;
using TeraCyteHomeAssignment.Services;

namespace TeraCyteHomeAssignment.ViewModels
{
    /// <summary>
    /// Main application ViewModel.
    /// Coordinates UI state, API client initialization, polling, 
    /// history storage, and histogram event flow.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        // Event raised when histogram values from a frame should be plotted.
        // UI (MainWindow) subscribes to this for live histogram updates.
        public event Action<int[]> HistogramUpdated;

        // Stores recent inference results for history panel.
        // Limited to last 50 frames to avoid memory bloating.
        public ObservableCollection<HistoryItem> History { get; set; } = new();

        private BitmapImage _currentImage;
        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        private ResultsResponse _results;
        public ResultsResponse Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        private string _connectionStatus = "Connecting...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        // Initializes API client, establishes login, starts polling loop.
        // Updates UI and history per incoming frames.
        public async void Start()
        {
            var api = new ApiClient();
            await api.InitializeAsync();
            var poller = new PollingService(api);

            // Handle connection state changes (UI banner feedback)
            poller.OnConnectionStateChanged += state =>
            {
                ConnectionStatus = state switch
                {
                    Services.ConnectionState.Connected => "✅ Connected",
                    Services.ConnectionState.Reconnecting => "🔄 Reconnecting...",
                    Services.ConnectionState.Failed => "❌ Failed to connect",
                    _ => "Unknown"
                };
            };

            // Handle new incoming frames from PollingService
            poller.OnNewFrame += (img, res) =>
            {
                if (img == null || res == null)
                    return; // skip frame safely

                // UI update on dispatcher (WPF thread affinity)
                App.Current.Dispatcher.Invoke(() =>
                {
                    CurrentImage = Base64ToBitmap(img.image_data_base64);
                    Results = res;

                    // Notify histogram listener
                    HistogramUpdated?.Invoke(res.histogram);
                });

                // Store in history (up to 50 items to avoid UI memory explosion)
                if (History.Count <= 50)
                {
                    History.Insert(0, new HistoryItem
                    {
                        Thumbnail = CurrentImage,
                        Classification = res.classification_label,
                        IntensityAvg = res.intensity_average,
                        FocusScore = res.focus_score,
                        Timestamp = DateTime.Now
                    });
                }
            };

            // Begin continuous polling loop
            _ = poller.Start();
        }

        // Converts Base64 string returned from server into WPF-friendly BitmapImage
        private BitmapImage Base64ToBitmap(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            var bmp = new BitmapImage();

            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.CacheOption = BitmapCacheOption.OnLoad; // Fully load into memory
            bmp.EndInit();

            return bmp;
        }

        // Clears history list (UI button handler uses this)
        public void ClearHistory()
        {
            History.Clear();
        }

    }
}
