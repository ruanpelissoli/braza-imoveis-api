using BrazaImoveis.Infrastructure.Models;

namespace BrazaImoveis.Infrastructure.Database;

public interface ISupabaseCachedClient
{
    Task<SearchProperty?> GetPropertyById(long id);
    Task<IEnumerable<State>> GetStates();
    Task<IEnumerable<City>> GetCities(long stateId);
}

public class SupabaseCachedClient : ISupabaseCachedClient
{
    private readonly Supabase.Client _supabaseClient;

    public SupabaseCachedClient(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }
    public async Task<SearchProperty?> GetPropertyById(long id)
    {
        var dbResponse = await _supabaseClient.From<SearchProperty>()
            .Where(t => t.Enabled == true && t.Id == id)
            .Get();

        return dbResponse.Models.FirstOrDefault();
    }

    public async Task<IEnumerable<State>> GetStates()
    {
        var dbResponse = await _supabaseClient.From<State>()
          .Where(t => t.Enabled == true)
          .Get();

        return dbResponse.Models;
    }

    public async Task<IEnumerable<City>> GetCities(long stateId)
    {
        var dbResponse = await _supabaseClient.From<City>()
         .Where(t => t.Enabled == true && t.StateId == stateId)
         .Get();

        return dbResponse.Models;
    }
}
