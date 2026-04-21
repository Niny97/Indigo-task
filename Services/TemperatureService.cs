using System.Globalization;
using Indigo_task.Models;

namespace Indigo_task.Services;

public sealed class TemperatureService : ITemperatureService
{
    private readonly string _filePath;
    private readonly ILogger<TemperatureService> _logger;

    private Dictionary<string, CityStats> _cache =
        new Dictionary<string, CityStats>(StringComparer.OrdinalIgnoreCase);

    public TemperatureService(IConfiguration config, ILogger<TemperatureService> logger)
    {
        _filePath = config["DataFilePath"] ?? "Data/measurements.csv";
        _logger = logger;
        Recalculate();
    }

    public IReadOnlyDictionary<string, CityStats> GetAll() => _cache;

    public CityStats? GetByCity(string city) =>
        _cache.TryGetValue(city, out var stats) ? stats : null;

    public void Recalculate()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogWarning("Data file not found: {Path}", _filePath);
            return;
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Starting temperature calculation from {Path}...", _filePath);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Optimization 1: lines are parsed in parallel across CPU cores via PLINQ
        // Optimization 2: single pass — min/max/avg are derived in one GroupBy sweep,
        //                  no separate iteration for each stat
        var newCache = File.ReadAllLines(_filePath)
            .AsParallel()
            .Select(ParseLine)
            .OfType<Reading>()          // OfType filters out nulls from failed parses
            .GroupBy(r => r.City)
            .Select(g => new CityStats(
                City: g.Key,
                Min:  Math.Round(g.Min(r => r.Temp), 2),
                Max:  Math.Round(g.Max(r => r.Temp), 2),
                Avg:  Math.Round(g.Average(r => r.Temp), 2)))
            .ToDictionary(s => s.City, StringComparer.OrdinalIgnoreCase);

        stopwatch.Stop();

        // Full memory barrier — guarantees all threads see the new dictionary immediately
        Interlocked.Exchange(ref _cache, newCache);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Recalculated temperature stats for {Count} cities in {Minutes}m {Seconds}s {Milliseconds}ms from {Path}",
                newCache.Count,
                stopwatch.Elapsed.Minutes,
                stopwatch.Elapsed.Seconds,
                stopwatch.Elapsed.Milliseconds,
                _filePath);
    }

    // Row format: 1986-01-01T00:00;New York;-0.5
    private static Reading? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        var parts = line.Split(';');
        if (parts.Length < 3) return null;

        if (!double.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var temp))
            return null;

        return new Reading(parts[1].Trim(), temp);
    }

    // Private type used only during parsing/aggregation — never exposed outside this class
    private sealed record Reading(string City, double Temp);
}
