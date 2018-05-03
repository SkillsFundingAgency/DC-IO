using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.SqlServer.Config.Interfaces;
using ESFA.DC.IO.SqlServer.Model;

namespace ESFA.DC.IO.SqlServer
{
    public class SqlServerKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private const string SqlSet =
            "MERGE [dbo].[DataExchange] AS [Target] USING (Select @JobId AS JobId, @Item AS Item, @Actor AS Actor) AS [Source] ON [Target].[Job_Id] = [Source].[JobId] AND [Target].[Item] = [Source].[Item] AND [Target].[ActorId] = [Source].[Actor] WHEN MATCHED THEN UPDATE SET [Target].[Value] = @Value, [Modified_By] = SUSER_SNAME(), [Modified_On] = GETUTCDATE() WHEN NOT MATCHED THEN INSERT ([Job_Id], [Item], [ActorId], [Value], [Created_By], [Created_On], [Modified_By], [Modified_On]) Values (@JobId, @Item, @Actor, @Value, SUSER_SNAME(), GETUTCDATE(), SUSER_SNAME(), GETUTCDATE());";

        private const string SqlGet =
            "SELECT [DataExchange_Id], [Job_Id], [Item], [ActorId], [Value] FROM [dbo].[DataExchange] WHERE [Job_Id] = @JobId AND [Item] = @Item AND (@Actor IS NULL OR [ActorId] = @Actor);";

        private const string SqlRemove =
            "DELETE FROM [dbo].[DataExchange] WHERE [Job_Id] = @JobId AND [Item] = @Item;";

        private const string SqlContains =
            "IF EXISTS(SELECT 1 FROM [DataService].[dbo].[DataExchange] de WHERE de.Job_Id = @JobId AND de.Item = @Item AND de.ActorId = @Actor) SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)";

        private readonly ISqlServerKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        public SqlServerKeyValuePersistenceService(ISqlServerKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value)
        {
            SqlKey sqlKey = new SqlKey(key);
            using (SqlConnection connection = new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
            {
                await connection.ExecuteAsync(SqlSet, new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor, Value = value });
            }
        }

        public async Task<string> GetAsync(string key)
        {
            SqlKey sqlKey = new SqlKey(key);
            using (SqlConnection connection = new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
            {
                DataExchange[] res = (await connection.QueryAsync<DataExchange>(SqlGet, new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor })).ToArray();
                if (!res.Any())
                {
                    throw new KeyNotFoundException($"Key '{key}' was not found in the store");
                }

                return res.Single().Value;
            }
        }

        public async Task RemoveAsync(string key)
        {
            SqlKey sqlKey = new SqlKey(key);
            using (SqlConnection connection = new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
            {
                await connection.ExecuteAsync(SqlRemove, new { sqlKey.JobId, sqlKey.Item });
            }
        }

        public async Task<bool> ContainsAsync(string key)
        {
            SqlKey sqlKey = new SqlKey(key);
            using (SqlConnection connection =
                new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
            {
                return (await connection.QueryAsync<bool>(SqlContains, new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor })).Single();
            }
        }
    }
}
