using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.SampleOnionModule.Abstractions;

namespace ThermoFisher.SampleOnionModule.Data.Sql;

public class SampleOnionRepository([FromKeyedServices("SampleOnionModule")] DbConnection connection)
    : ISampleOnionRepository
{
    public async Task<Sample> GetSampleByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var uuid = new Guid(id);
        string sql = """
            SELECT * from "sampleonionmodule"."samples" where id = @id
            """;
        var result = await connection.QuerySingleAsync(sql, new { id = uuid });
        return new Sample(
            result.id.ToString(),
            result.name,
            result.description,
            result.created_date,
            result.last_updated_date,
            new List<string>()
        );
    }

    public async Task AddSamplesAsync(IEnumerable<Sample> samples)
    {
        var sql = """
            INSERT INTO "sampleonionmodule"."samples" (id, name, description, created_date, last_updated_date)
            VALUES (gen_random_uuid (), @Name, @Description, @CreatedAt, @UpdatedAt)
            """;
        using var transaction = await connection.BeginTransactionAsync();
        foreach (var sample in samples)
        {
            await connection.ExecuteAsync(sql, sample);
        }
        await transaction.CommitAsync();
    }
}
