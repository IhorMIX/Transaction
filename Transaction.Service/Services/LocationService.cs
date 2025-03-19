using System.Text.Json;
using Microsoft.Extensions.Options;
using Transaction.Core.Entities;
using Transaction.Service.Options;
using Transaction.Service.Services.Interfaces;

namespace Transaction.Service.Services;

public class LocationService(HttpClient httpClient,IOptions<TimeZoneOption> options) : ILocationService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TimeZoneOption _options = options.Value;
    
    /// <summary>
    /// We return timezone by latitude and longitude
    /// </summary>
    public async Task<string> GetTimeZoneByLocation(string clientLocation, CancellationToken cancellationToken = default)
    {
        var coordinates = clientLocation.Split(',');
        if (coordinates.Length != 2)
        {
            throw new ArgumentException("Invalid coordinates format. Expected format: 'latitude,longitude'");
        }

        var latitude = coordinates[0].Trim();
        var longitude = coordinates[1].Trim();

        var requestUrl = $"{_options.BaseUrl}?apiKey={_options.ApiKey}&lat={latitude}&long={longitude}";
    
        var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
    
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(content);
    
        return jsonDoc.RootElement.GetProperty("timezone").GetString() ?? "Unknown";
    }
}