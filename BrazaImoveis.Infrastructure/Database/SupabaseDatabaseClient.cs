using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Infrastructure.Models;
using Postgrest;
using System.Linq.Expressions;
using static Postgrest.QueryOptions;

namespace BrazaImoveis.Infrastructure.Database;

public interface IDatabaseClient
{
    Task<TModel?> GetById<TModel>(long id) where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<TModel>> GetAll<TModel>() where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<TModel>> GetAll<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new();
    Task<TModel> Insert<TModel>(TModel model) where TModel : BaseDatabaseModel, new();
    Task Insert<TModel>(IEnumerable<TModel> models) where TModel : BaseDatabaseModel, new();
    Task Delete<TModel>(long id) where TModel : BaseDatabaseModel, new();
    Task Update<TModel>(TModel model) where TModel : BaseDatabaseModel, new();
    Task<IEnumerable<Property>> FilterProperties(PropertiesFilterRequest filter);
}


internal class SupabaseDatabaseClient : IDatabaseClient
{
    private readonly Supabase.Client _supabaseClient;

    public SupabaseDatabaseClient(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<TModel?> GetById<TModel>(long id) where TModel : BaseDatabaseModel, new()
    {
        var dbResponse = await _supabaseClient.From<TModel>()
            .Where(t => t.Enabled && t.Id == id)
            .Get();

        return dbResponse.Models.FirstOrDefault();
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

    public async Task Insert<TModel>(IEnumerable<TModel> models) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>().Insert(models.ToList());
    }

    public async Task Delete<TModel>(long id) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>()
             .Where(t => t.Id == id)
             .Delete();
    }

    public async Task Update<TModel>(TModel model) where TModel : BaseDatabaseModel, new()
    {
        await _supabaseClient.From<TModel>().Update(model);
    }

    public async Task<IEnumerable<Property>> FilterProperties(PropertiesFilterRequest filter)
    {
        var db = _supabaseClient.From<Property>().Where(f => f.Enabled == true);

        if (filter.Bedrooms.HasValue)
        {
            db = db.Where(x => x.FilterBedrooms == filter.Bedrooms.Value);
        }

        if (filter.Bathrooms.HasValue)
        {
            db = db.Where(x => x.FilterBathrooms == filter.Bathrooms.Value);
        }

        if (filter.GarageSpaces.HasValue)
        {
            db = db.Where(x => x.FilterGarageSpaces == filter.GarageSpaces.Value);
        }

        if (filter.Price.HasValue)
        {
            db = db.Where(x => x.FilterCost <= filter.Price.Value);
        }

        if (filter.SquareFoot.HasValue)
        {
            db = db.Where(x => x.FilterSquareFoot <= filter.SquareFoot.Value);
        }

        var (from, to) = GetPagination(filter.Page, filter.Size);

        var result = await db.Range(from, to).Get();

        return result.Models;
    }

    private (int page, int size) GetPagination(int? page, int? size)
    {
        var limite = size.HasValue ? +size : 3;
        var from = page.HasValue ? page.Value * limite : 0;
        var to = page.HasValue ? from + size!.Value - 1 : size!.Value - 1;

        return (from.Value, to.Value);
    }
}
