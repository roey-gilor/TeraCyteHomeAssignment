using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeraCyteHomeAssignment.Helpers
{
    /// <summary>
    /// POCO representing application config settings loaded from JSON.
    /// </summary>
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Singleton manager responsible for loading and holding app configuration.
    /// Reads config.private.json on startup, validates content,
    /// logs issues and fails early if config missing or invalid.
    /// </summary>
    public sealed class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
        public static ConfigManager Instance => _instance.Value;

        public AppConfig Settings { get; private set; }

        private ConfigManager()
        {
            try
            {
                string file = "config.private.json";

                if (!File.Exists(file))
                {
                    Logger.Error($"Config file not found: {file}");
                    throw new FileNotFoundException($"{file} not found. Create it based on config.example.json.");
                }
                Logger.Info($"Loading configuration from {file}");

                string json = File.ReadAllText(file);
                Settings = JsonSerializer.Deserialize<AppConfig>(json)
                           ?? throw new Exception("Failed to parse config file.");

                if (Settings == null)
                {
                    Logger.Error("Failed to parse config file: null result");
                    throw new Exception("Failed to parse config file.");
                }

                if (string.IsNullOrWhiteSpace(Settings.ApiBaseUrl))
                    Logger.Warn("Config warning: ApiBaseUrl is empty.");

                Logger.Info("Configuration loaded successfully");
            }
            catch (JsonException ex)
            {
                Logger.Error("JSON format error in configuration file", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error loading configuration", ex);
                throw;
            }
        }
    }
}
