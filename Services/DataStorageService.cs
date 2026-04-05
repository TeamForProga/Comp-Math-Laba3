using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using laba3.Models;

namespace laba3.Services;

public record AppState
{
    public List<Polynomia> Polynomia { get; init; } = [];
    public List<ApproxLagrangeFunc> ApproxLagrangeFunc { get; init; } = [];
    public List<ApproxNewtonFunc> ApproxNewtonFunc { get; init; } = [];
    public List<LeastSquares> LeastSquares { get; init; } = [];
    public List<Coord> Coord { get; init; } = [];

}

public static class DataStorageService
{
    private static readonly string DirectoryPath = AppContext.BaseDirectory;

    private static readonly string JsonFilePath = Path.Combine(DirectoryPath, "state.json");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task SaveAsync(AppState state)
    {
        Directory.CreateDirectory(DirectoryPath);
        await using var fs = File.Create(JsonFilePath);
        await JsonSerializer.SerializeAsync(fs, state, Options);
    }

    public static async Task<AppState> LoadAsync()
    {
        try
        {
            await using var fs = File.OpenRead(JsonFilePath);
            var state = await JsonSerializer.DeserializeAsync<AppState>(fs, Options);

            return state ?? new AppState();
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"Hello {e}  ");
            
            return new AppState();

        }
    }
}
