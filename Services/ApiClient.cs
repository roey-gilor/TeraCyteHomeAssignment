using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TeraCyteHomeAssignment.Helpers;
using TeraCyteHomeAssignment.Models;

namespace TeraCyteHomeAssignment.Services
{
    /// <summary>
    /// API client responsible for communicating with the backend.
    /// Handles login, token refresh, authorized requests and error logging.
    /// Used by polling service and ViewModels to retrieve protected data.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;
        private string _accessToken;
        private string _refreshToken;
        private DateTime _expiresAt;

        // Initializes HttpClient with server base URL.
        // Configuration values (URL + credentials) loaded from ConfigManager.
        public ApiClient()
        {
            // Load config
            var cfg = ConfigManager.Instance.Settings;

            // Init HTTP client
            _http = new HttpClient
            {
                BaseAddress = new Uri(cfg.ApiBaseUrl)
            };
        }

        // Performs the initial login call. Called by ViewModel on startup before polling begins.
        public async Task InitializeAsync()
        {
            var cfg = ConfigManager.Instance.Settings;
            await Login(cfg.Username, cfg.Password);
        }

        // Authenticates using username/password and retrieves access + refresh tokens.
        // Stores tokens and configures HttpClient authorization header.
        private async Task<bool> Login(string username, string password)
        {
            try
            {
                Logger.Info("Attempting login...");
                var response = await _http.PostAsJsonAsync("/api/auth/login", new { username, password });

                if (!response.IsSuccessStatusCode) {
                    Logger.Warn("Login failed");
                    return false;
                }

                var token = await response.Content.ReadFromJsonAsync<AuthResponse>();

                _accessToken = token.access_token;
                _refreshToken = token.refresh_token;

                // Renew token slightly BEFORE expiration to avoid edge-race failures
                _expiresAt = DateTime.UtcNow.AddSeconds(token.expires_in - 20);

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                Logger.Info("Login successful");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Login error", ex);
                return false;
            }
        }

        // Refreshes access token if expired or about to expire.
        // Called automatically before protected API requests.
        private async Task RefreshTokenIfNeeded()
        {
            if (DateTime.UtcNow < _expiresAt) return; // Still valid
            
            Logger.Info("Access token expired — attempting refresh...");

            var response = await _http.PostAsJsonAsync("/api/auth/refresh", new { refresh_token = _refreshToken });
            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"Token refresh failed: HTTP {response.StatusCode}");
                return;
            }

            var token = await response.Content.ReadFromJsonAsync<AuthResponse>();

            _accessToken = token.access_token;
            _refreshToken = token.refresh_token;
            _expiresAt = DateTime.UtcNow.AddSeconds(token.expires_in - 20);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            Logger.Info("Token refresh completed successfully");
        }


        // Performs authenticated GET request.
        // Handles token expiration, refresh, retry logic, and logs errors.
        // Returns default(T) on failure rather than throwing — safe for polling loops.
        public async Task<T?> GetProtected<T>(string endpoint)
        {
            try
            {
                await RefreshTokenIfNeeded();

                var res = await _http.GetAsync(endpoint);

                // Unauthorized → refresh token and retry once
                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Logger.Warn("Unauthorized — attempting token refresh...");

                    // Try refresh token and retry request
                    await RefreshTokenIfNeeded();
                    res = await _http.GetAsync(endpoint);
                }

                if (!res.IsSuccessStatusCode)
                {
                    Logger.Warn($"GET {endpoint} failed ({res.StatusCode})");
                    return default;
                }

                return await res.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Logger.Error($"GET {endpoint} exception", ex);
                return default;
            }
        }
    }
}
