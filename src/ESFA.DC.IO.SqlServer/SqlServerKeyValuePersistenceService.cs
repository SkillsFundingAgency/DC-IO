using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.SqlServer.Config.Interfaces;
using ESFA.DC.IO.SqlServer.Model;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.IO.SqlServer
{
    public class SqlServerKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private const string SqlSet =
            "Merge [dbo].[DataExchange] as [Target] Using (Select @JobId as JobId, @Item as Item, @Actor as Actor) as [Source] on [Target].[Job_Id] = [Source].[JobId] And [Target].[Item] = [Source].[Item] And [Target].[ActorId] = [Source].[Actor] When Matched Then Update Set [Target].[Value] = @Value, [Modified_By] = SUSER_SNAME(), [Modified_On] = GETUTCDATE() When Not Matched Then Insert ([Job_Id], [Item], [ActorId], [Value], [Created_By], [Created_On], [Modified_By], [Modified_On]) Values (@JobId, @Item, @Actor, @Value, SUSER_SNAME(), GETUTCDATE(), SUSER_SNAME(), GETUTCDATE());";

        private const string SqlGet =
            "SELECT [DataExchange_Id], [Job_Id], [Item], [ActorId], [Value] FROM [dbo].[DataExchange] WHERE [Job_Id] = @JobId AND [Item] = @Item AND (@Actor IS NULL OR [ActorId] = @Actor);";

        private const string SqlRemove =
            "DELETE FROM [dbo].[DataExchange] WHERE [Job_Id] = @JobId AND [Item] = @Item;";

        private readonly ISqlServerKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly ILogger _logger;

        public SqlServerKeyValuePersistenceService(ISqlServerKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig, ILogger logger)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
            _logger = logger;
        }

        public async Task SaveAsync(string key, string value)
        {
            // _logger.LogWarning($"{key} Sql Set");
            using (new TimedLogger(_logger, "Sql Set"))
            {
                SqlKey sqlKey = new SqlKey(key);
                using (SqlConnection connection = new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
                {
                    await connection.ExecuteAsync(SqlSet, new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor, Value = value });
                    //await connection.ExecuteAsync(
                    //    "[dbo].[usp_Set_DataExchangeJobValues]",
                    //    new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor, Value = value },
                    //    commandType: CommandType.StoredProcedure);
                }
            }
        }

        public async Task<string> GetAsync(string key)
        {
            // _logger.LogWarning($"{key} Sql Get");
            using (new TimedLogger(_logger, "Sql Get"))
            {
                SqlKey sqlKey = new SqlKey(key);
                using (SqlConnection connection =
                    new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
                {
                    return (await connection.QueryAsync<DataExchange>(SqlGet, new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor })).Single().Value;
                    //return (await connection.QueryAsync<DataExchange>(
                    //    "[dbo].[usp_Get_DataExchangeKeyValue]",
                    //    new { sqlKey.JobId, sqlKey.Item, sqlKey.Actor },
                    //    commandType: CommandType.StoredProcedure)).Single().Value;
                }
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Sql Remove"))
            {
                SqlKey sqlKey = new SqlKey(key);
                using (SqlConnection connection = new SqlConnection(_keyValuePersistenceServiceConfig.ConnectionString))
                {
                    await connection.ExecuteAsync(SqlRemove, new { sqlKey.JobId, sqlKey.Item });
                    //await connection.ExecuteAsync(
                    //    "[dbo].[usp_Remove_DataExchangeJobValue]",
                    //    new { sqlKey.JobId, sqlKey.Item },
                    //    commandType: CommandType.StoredProcedure);
                }
            }
        }
    }
}
