using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Infrastructure.Models;
using Postgrest;
using System.Linq.Expressions;
using static Postgrest.Constants;
using static Postgrest.QueryOptions;

namespace BrazaImoveis.Infrastructure.Database;

public interface IDatabaseClient
{
    Task<IEnumerable<TModel>> GetAll<TModel>() where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<TModel>> GetAll<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new();
    Task<TModel> Insert<TModel>(TModel model) where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<TModel>> Insert<TModel>(IEnumerable<TModel> models) where TModel : BaseDatabaseModel, new();
    Task Delete<TModel>(long id) where TModel : BaseDatabaseModel, new();
    Task Delete<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new();
    Task Update<TModel>(TModel model) where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<SearchProperty>> FilterProperties(PropertiesFilterRequest filter);
}


internal class SupabaseDatabaseClient : IDatabaseClient
{
    private readonly Supabase.Client _supabaseClient;

    public SupabaseDatabaseClient(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<IEnumerable<TModel>> GetAll<TModel>() where TModel : BaseDatabaseModel, new()
    {
        var dbResponse = await _supabaseClient.From<TModel>()
            .Where(t => t.Enabled == true)
            .Get();

        return dbResponse.Models;
    }

    public async Task<IEnumerable<TModel>> GetAll<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new()
    {
        var dbResponse = await _supabaseClient.From<TModel>()
            .Where(x => x.Enabled == true)
            .Where(predicate)
            .Get();

        return dbResponse.Models;
    }

    public async Task<TModel> Insert<TModel>(TModel model) where TModel : BaseDatabaseModel, new()
    {
        var result = await _supabaseClient.From<TModel>().Insert(model, new QueryOptions { Returning = ReturnType.Representation });

        return result.Models.First();
    }

    public async Task<IEnumerable<TModel>> Insert<TModel>(IEnumerable<TModel> models) where TModel : BaseDatabaseModel, new()
    {
        var result = await _supabaseClient.From<TModel>().Insert(models.ToList());

        return result.Models;
    }

    public async Task Delete<TModel>(long id) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>()
             .Where(t => t.Id == id)
             .Delete();
    }

    public async Task Delete<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>()
            .Where(predicate)
            .Delete();
    }

    public async Task Update<TModel>(TModel model) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>().Update(model);
    }

    public async Task<IEnumerable<SearchProperty>> FilterProperties(PropertiesFilterRequest filter)
    {
        var db = _supabaseClient.From<SearchProperty>().Where(f => f.Enabled == true);

        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var t = filter.Type.ToUpper();
            db = db.Where(x => x.PropertyFilterType == t);
        }

        if (filter.Bedrooms.HasValue)
            db = db.Where(x => x.PropertyFilterBedrooms == filter.Bedrooms.Value);

        if (filter.Bathrooms.HasValue)
            db = db.Where(x => x.PropertyFilterBathrooms == filter.Bathrooms.Value);

        if (filter.GarageSpaces.HasValue)
            db = db.Where(x => x.PropertyFilterGarageSpaces == filter.GarageSpaces.Value);

        if (filter.Price.HasValue)
            db = db.Where(x => x.PropertyFilterPrice <= filter.Price.Value);

        if (filter.SquareFoot.HasValue)
            db = db.Where(x => x.PropertyFilterSquareFoot <= filter.SquareFoot.Value);

        if (filter.StateId.HasValue)
            db = db.Where(x => x.StateId == filter.StateId.Value);

        if (filter.CityId.HasValue)
            db = db.Where(x => x.CityId == filter.CityId.Value);

        var (from, to) = GetPagination(filter.Page, filter.Size);

        var result = await db
            .Order(o => o.CreatedAt, Ordering.Descending)
            .Range(from, to)
            .Get();

        return result.Models;
    }

    private static (int page, int size) GetPagination(int? page, int? size)
    {
        var limite = size.HasValue ? +size : 3;
        var from = page.HasValue ? page.Value * limite : 0;
        var to = page.HasValue ? from + size!.Value - 1 : size!.Value - 1;

        return (from.Value, to.Value);
    }
}
