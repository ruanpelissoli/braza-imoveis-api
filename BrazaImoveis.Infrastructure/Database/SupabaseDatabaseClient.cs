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
            .Range(1, 50)
            .Get();

        return dbResponse.Models;
    }

    public async Task<IEnumerable<TModel>> GetAll<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : BaseDatabaseModel, new()
    {
        Expression<Func<TModel, bool>> enabledExpression = x => x.Enabled == true;

        var concatenated = Expression.Lambda<Func<TModel, bool>>(
            Expression.AndAlso(
                predicate.Body,
                enabledExpression.Body
            ),
            predicate.Parameters
        );

        var dbResponse = await _supabaseClient.From<TModel>()
            .Where(concatenated)
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
}
