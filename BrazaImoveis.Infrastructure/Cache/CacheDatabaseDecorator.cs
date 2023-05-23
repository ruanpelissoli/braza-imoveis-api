using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BrazaImoveis.Infrastructure.Cache;
public class CacheDatabaseDecorator : ISupabaseCachedClient
{
    private readonly IMemoryCache _memoryCache;
    private readonly ISupabaseCachedClient _cachedDatabaseClient;
    private readonly IDatabaseClient _databaseClient;

    private readonly MemoryCacheEntryOptions _cacheOptions;

    public CacheDatabaseDecorator(
        IMemoryCache memoryCache,
        ISupabaseCachedClient cachedDatabaseClient,
        IDatabaseClient databaseClient)
    {

        _memoryCache = memoryCache;
        _cachedDatabaseClient = cachedDatabaseClient;
        _databaseClient = databaseClient;

        _cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24)
        };
    }

    public async Task<SearchProperty?> GetPropertyById(long id)
    {
        var key = $"Property_{id}";

        var cached = _memoryCache.Get<SearchProperty>(key);

        if (cached != null)
            return cached;

        var property = await _cachedDatabaseClient.GetPropertyById(id);

        if (property != null)
            _memoryCache.Set(key, property, _cacheOptions);

        return property;
    }

    public async Task<IEnumerable<SearchProperty>> GetSimilarProperties(SearchProperty property)
    {
        var key = $"Property_{property.Id}_similar";

        var cached = _memoryCache.Get<IEnumerable<SearchProperty>>(key);

        if (cached != null)
            return cached;

        var similarProperties = await _databaseClient.FilterProperties(new PropertiesFilterRequest
        {
            Bedrooms = property.PropertyFilterBedrooms,
            StateId = property.StateId,
            CityId = property.CityId,
            Size = 6
        });

        if (similarProperties != null && similarProperties.Any())
            _memoryCache.Set(key, similarProperties, _cacheOptions);

        return similarProperties!;
    }

    public async Task<IEnumerable<State>> GetStates()
    {
        var key = "states";

        var cached = _memoryCache.Get<IEnumerable<State>>(key);

        if (cached != null)
            return cached;

        var states = await _cachedDatabaseClient.GetStates();

        _memoryCache.Set(key, states, _cacheOptions);

        return states;
    }

    public async Task<IEnumerable<City>> GetCities(long stateId)
    {
        var key = $"cities_{stateId}";

        var cached = _memoryCache.Get<IEnumerable<City>>(key);

        if (cached != null)
            return cached;

        var cities = await _cachedDatabaseClient.GetCities(stateId);

        if (cities != null && cities.Any())
            _memoryCache.Set(key, cities, _cacheOptions);

        return cities!;
    }
}
