using System.Text.Json;
using System.Text.Json.Serialization;

using TrackOMatic.Logic.Models.Spoilers;

namespace TrackOMatic.Services.SpoilerDeserialization;

/// <summary>
/// Provides methods to deserialize spoiler JSON files into strongly-typed <see cref="RawSpoilerFile"/> objects.
/// </summary>
/// <remarks>
/// This service handles the conversion from raw JSON to strongly-typed C# objects.
/// It uses <see cref="JsonSerializerOptions"/> to respect <see cref="JsonPropertyNameAttribute"/> mappings.
/// </remarks>
public interface IRawSpoilerFileDeserializer
{
    /// <summary>
    /// Deserializes a spoiler file from a file path asynchronously.
    /// </summary>
    /// <param name="filePath">The full path to the spoiler JSON file.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.</returns>
    Task<DeserializationResult<RawSpoilerFile>> DeserializeFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes a spoiler from a JSON string asynchronously.
    /// </summary>
    /// <param name="jsonContent">The raw JSON string content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.</returns>
    Task<DeserializationResult<RawSpoilerFile>> DeserializeFromStringAsync(string jsonContent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a deserialization operation.
/// </summary>
/// <typeparam name="T">The type of data that was deserialized.</typeparam>
public record DeserializationResult<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorMessage,
    Exception? Exception
)
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static DeserializationResult<T> Success(T data) =>
        new(IsSuccess: true, Data: data, ErrorMessage: null, Exception: null);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static DeserializationResult<T> Failure(string errorMessage, Exception? exception = null) =>
        new(IsSuccess: false, Data: default, ErrorMessage: errorMessage, Exception: exception);
}

/// <summary>
/// Implementation of <see cref="IRawSpoilerFileDeserializer"/> that handles JSON deserialization.
/// </summary>
public class RawSpoilerFileDeserializer : IRawSpoilerFileDeserializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
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
    /// <returns>A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.</returns>
    public async Task<DeserializationResult<RawSpoilerFile>> DeserializeFromFileAsync(string filePath, CancellationToken cancellationToken = default)
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
        catch (OperationCanceledException ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure("Deserialization operation was cancelled.", ex);
        }
        catch (IOException ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"IO error reading file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error reading file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a spoiler from a JSON string asynchronously.
    /// </summary>
    /// <param name="jsonContent">The raw JSON string content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="DeserializationResult{T}"/> containing the deserialized data or error information.</returns>
    public async Task<DeserializationResult<RawSpoilerFile>> DeserializeFromStringAsync(string jsonContent, CancellationToken cancellationToken = default)
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
                    var spoilerFile = JsonSerializer.Deserialize<RawSpoilerFile>(jsonContent, JsonOptions);

                    if (spoilerFile == null)
                    {
                        return DeserializationResult<RawSpoilerFile>.Failure("Deserialization resulted in null object. Check JSON structure.");
                    }

                    return DeserializationResult<RawSpoilerFile>.Success(spoilerFile);
                }
                catch (JsonException ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Invalid JSON format: {ex.Message}", ex);
                }
                catch (ArgumentException ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Invalid argument during deserialization: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error during deserialization: {ex.Message}", ex);
                }
            }, cancellationToken);

            return result;
        }
        catch (OperationCanceledException ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure("Deserialization operation was cancelled.", ex);
        }
        catch (Exception ex)
        {
            return DeserializationResult<RawSpoilerFile>.Failure($"Unexpected error: {ex.Message}", ex);
        }
    }
}
