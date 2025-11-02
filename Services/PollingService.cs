using System;
using System.Threading.Tasks;
using TeraCyteHomeAssignment.Helpers;
using TeraCyteHomeAssignment.Models;

namespace TeraCyteHomeAssignment.Services
{
    /// <summary>
    /// Represents client connection state to the backend.
    /// Used by UI to display connection status (Connected / Reconnecting / Failed).
    /// </summary>
    public enum ConnectionState
    {
        Connected,
        Reconnecting,
        Failed
    }

    // Continuously polls the backend for new image frames and inference results.
    // Handles transient network failures and token expiration gracefully.
    // Notifies ViewModel about new frames and connection state changes.
    public class PollingService
    {
        private readonly ApiClient _client;

        // Event fired when a new image + results pair is available
        public event Action<ImageResponse, ResultsResponse> OnNewFrame;

        // Notifies UI when connection state changes (for reconnect UI banner)
        public event Action<ConnectionState> OnConnectionStateChanged;

        // Stores last seen image ID to detect new frames
        private string _lastImageId = "";

        // Tracks connection status to trigger reconnect UI only once
        private bool _wasConnected = false, isReconnect = false;

        public PollingService(ApiClient client) => _client = client;

        // Starts endless polling loop.
        // Fetches image → if changed, fetches results.
        // Applies retry logic with backoff and logs all events.
        public async Task Start()
        {
            while (true)
            {
                try
                {
                    // Request latest image from server
                    var img = await _client.GetProtected<ImageResponse>("/api/image");

                    if (img == null)
                    {
                        Logger.Warn("Received null image");
                        NotifyReconnect();
                        await Task.Delay(1000);
                        continue;
                    }

                    // Only fetch results when frame changed
                    if (img.image_id != _lastImageId)
                    {
                        var results = await _client.GetProtected<ResultsResponse>("/api/results");

                        if (results == null)
                        {
                            Logger.Warn("Received null results");
                            _lastImageId = img.image_id; // skip but advance pointer
                            NotifyReconnect();
                            await Task.Delay(1000);
                            continue;
                        }

                        // We have a valid frame + results — deliver to UI
                        _lastImageId = img.image_id;
                        OnNewFrame?.Invoke(img, results);
                        NotifyConnected();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Polling loop failed", ex);
                    NotifyFailed();
                    // short backoff to avoid tight retry loop
                    await Task.Delay(1500);
                }

                // Polling interval — camera is “live-feed style” not event-driven
                await Task.Delay(1000);
            }
        }
        private void NotifyReconnect()
        {
            if (_wasConnected && !isReconnect)
            {
                _wasConnected = false;
                isReconnect = true;
                OnConnectionStateChanged?.Invoke(ConnectionState.Reconnecting);
            }
        }

        private void NotifyConnected()
        {
            if (!_wasConnected)
            {
                _wasConnected = true;
                OnConnectionStateChanged?.Invoke(ConnectionState.Connected);
            }
        }

        private void NotifyFailed()
        {
            if (_wasConnected && isReconnect)
            {
                _wasConnected = false;
                isReconnect = false;
                OnConnectionStateChanged?.Invoke(ConnectionState.Failed);
            }
        }

    }
}
