using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BrazaImoveis.Infrastructure.Cache;
public class CacheDatabaseDecorator : ISupabaseCachedClient
{
    private readonly IMemoryCache _memoryCache;
    private readonly ISupabaseCachedClient _databaseClient;

    public CacheDatabaseDecorator(IMemoryCache memoryCache, ISupabaseCachedClient databaseClient)
    {

        _memoryCache = memoryCache;
        _databaseClient = databaseClient;
    }

    public async Task<SearchProperty?> GetPropertyById(long id)
    {
        var key = $"Property_{id}";

        var cached = _memoryCache.Get<SearchProperty>(key);

        if (cached != null)
            return cached;

        var property = await _databaseClient.GetPropertyById(id);

        if (property != null)
            _memoryCache.Set(key, property);

        return property;
    }

    public async Task<IEnumerable<State>> GetStates()
    {
        var key = "states";

        var cached = _memoryCache.Get<IEnumerable<State>>(key);

        if (cached != null)
            return cached;

        var states = await _databaseClient.GetStates();

        _memoryCache.Set(key, states);

        return states;
    }

    public async Task<IEnumerable<City>> GetCities(long stateId)
    {
        var key = $"cities_{stateId}";

        var cached = _memoryCache.Get<IEnumerable<City>>(key);

        if (cached != null)
            return cached;

        var cities = await _databaseClient.GetCities(stateId);

        if (cities != null && cities.Any())
            _memoryCache.Set(key, cities);

        return cities!;
    }
}
