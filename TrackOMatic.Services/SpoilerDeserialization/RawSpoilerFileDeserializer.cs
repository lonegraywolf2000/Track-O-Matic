using System.Text.Json;
using System.Text.Json.Serialization;

using TrackOMatic.Logic.Models.Spoilers;

namespace TrackOMatic.Services.SpoilerDeserialization;

/// <summary>
/// Internal implementation for deserializing raw spoiler JSON files into
/// strongly-typed <see cref="RawSpoilerFile"/> objects.
/// </summary>
/// <remarks>
/// This is used internally by <see cref="ISpoilerService"/> implementations.
/// End-users should use <see cref="ISpoilerService"/> instead.
/// </remarks>
internal class RawSpoilerFileDeserializer
{
    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = null,  // Don't apply naming policy; use [JsonPropertyName] mappings
        PropertyNameCaseInsensitive = false,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        WriteIndented = false
    };

    /// <summary>
    /// Deserializes a spoiler file from a file path asynchronously.
    /// </summary>
    /// <param name="filePath">The full path to the spoiler JSON file.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.
    /// </returns>
    public async Task<DeserializationResult<RawSpoilerFile>> DeserializeFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return DeserializationResult<RawSpoilerFile>.Failure("File path cannot be null or empty.");
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                return DeserializationResult<RawSpoilerFile>.Failure($"File not found: {filePath}");
            }

            // Read file asynchronously
            string jsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Deserialize the JSON content
            return await DeserializeFromStringAsync(jsonContent, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return DeserializationResult<RawSpoilerFile>.Failure("Deserialization operation was cancelled.");
        }
        catch (IOException ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"IO error reading file: {ex.Message}");
        }
        catch (Exception ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error reading file: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes a spoiler from a JSON string asynchronously.
    /// </summary>
    /// <param name="jsonContent">The raw JSON string content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.
    /// </returns>
    public async Task<DeserializationResult<RawSpoilerFile>> DeserializeFromStringAsync(
        string jsonContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return DeserializationResult<RawSpoilerFile>.Failure("JSON content cannot be null or empty.");
            }

            // Run deserialization on a thread pool thread to avoid blocking
            var result = await Task.Run(() =>
            {
                try
                {
                    var spoilerFile = JsonSerializer.Deserialize<RawSpoilerFile>(jsonContent, _JsonOptions);

                    if (spoilerFile == null)
                    {
                        return DeserializationResult<RawSpoilerFile>.Failure("Deserialization resulted in null object. Check JSON structure.");
                    }

                    return DeserializationResult<RawSpoilerFile>.Success(spoilerFile);
                }
                catch (JsonException ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Invalid JSON format: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Invalid argument during deserialization: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error during deserialization: {ex.Message}");
                }
            }, cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            return DeserializationResult<RawSpoilerFile>.Failure("Deserialization operation was cancelled.");
        }
        catch (Exception ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}
