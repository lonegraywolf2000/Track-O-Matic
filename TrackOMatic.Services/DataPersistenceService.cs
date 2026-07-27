using System.Runtime.CompilerServices;
using System.Text.Json;

using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;

[assembly: InternalsVisibleTo("TrackOMatic.Services.Test")]

namespace TrackOMatic.Services;

/// <summary>
/// Handles file I/O and JSON serialization for game progress data.
/// Pure data persistence layer; does not mutate UI or game state.
/// </summary>
public class DataPersistenceService : IDataPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public event EventHandler<SavedProgressChangedEventArgs>? SaveCompleted;
    public event EventHandler<string>? SaveFailed;

    /// <summary>
    /// Load saved progress from a JSON file.
    /// </summary>
    public SavedProgress? Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var jsonString = File.ReadAllText(filePath);
            var savedData = JsonSerializer.Deserialize<SavedProgress>(jsonString, JsonOptions);
            return savedData;
        }
        catch (JsonException ex)
        {
            var errorMsg = $"Failed to deserialize {filePath}: {ex.Message}";
            SaveFailed?.Invoke(this, errorMsg);
            return null;
        }
        catch (IOException ex)
        {
            var errorMsg = $"Failed to read {filePath}: {ex.Message}";
            SaveFailed?.Invoke(this, errorMsg);
            return null;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Unexpected error loading {filePath}: {ex.Message}";
            SaveFailed?.Invoke(this, errorMsg);
            return null;
        }
    }

    /// <summary>
    /// Save game progress to a JSON file.
    /// </summary>
    public void Save(SavedProgress progress, string filePath)
    {
        if (progress == null)
        {
            SaveFailed?.Invoke(this, "Cannot save null SavedProgress");
            return;
        }

        try
        {
            var jsonString = JsonSerializer.Serialize(progress, JsonOptions);
            File.WriteAllText(filePath, jsonString);
            SaveCompleted?.Invoke(this, new SavedProgressChangedEventArgs
            {
                FilePath = filePath,
                Success = true
            });
        }
        catch (IOException ex)
        {
            var errorMsg = $"Failed to write {filePath}: {ex.Message}";
            SaveFailed?.Invoke(this, errorMsg);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Unexpected error saving to {filePath}: {ex.Message}";
            SaveFailed?.Invoke(this, errorMsg);
        }
    }

    /// <summary>
    /// Clear in-memory state (if needed for reset scenarios).
    /// </summary>
    public void Reset()
    {
        // Service is stateless; nothing to reset
        // Placeholder for future cache management if needed
    }
}
