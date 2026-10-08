using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;
using RiftLingo.Models;

namespace RiftLingo.Services;

public sealed class SettingsStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("RiftLingo.Gemini.v1");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;

    public SettingsStore()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RiftLingo");
        Directory.CreateDirectory(directory);
        _settingsPath = Path.Combine(directory, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            return File.Exists(_settingsPath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath), JsonOptions) ?? new AppSettings()
                : new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings, string apiKey)
    {
        settings.EncryptedGeminiApiKey = Encrypt(apiKey);
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    public string GetApiKey(AppSettings settings)
    {
        var environmentKey = Environment.GetEnvironmentVariable("RIFTLINGO_GEMINI_API_KEY")
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(environmentKey))
        {
            return environmentKey.Trim();
        }

        if (string.IsNullOrWhiteSpace(settings.EncryptedGeminiApiKey))
        {
            return string.Empty;
        }

        try
        {
            var protectedBytes = Convert.FromBase64String(settings.EncryptedGeminiApiKey);
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string Encrypt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(value.Trim());
        return Convert.ToBase64String(ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser));
    }
}
