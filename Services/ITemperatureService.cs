using Indigo_task.Models;

namespace Indigo_task.Services;

public interface ITemperatureService
{
    IReadOnlyDictionary<string, CityStats> GetAll();
    CityStats? GetByCity(string city);
    void Recalculate();
}
