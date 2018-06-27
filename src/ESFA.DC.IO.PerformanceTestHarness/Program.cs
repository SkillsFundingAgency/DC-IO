using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureCosmos;
using ESFA.DC.IO.AzureCosmos.Config.Interfaces;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Compressed;
using ESFA.DC.IO.AzureStorage.Compressed.Config.Interfaces;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.AzureTableStorage;
using ESFA.DC.IO.AzureTableStorage.Config.Interfaces;
using ESFA.DC.IO.Dictionary;
using ESFA.DC.IO.FileSystem;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using ESFA.DC.IO.PerformanceTestHarness.Model;
using ESFA.DC.IO.Redis;
using ESFA.DC.IO.Redis.Config.Interfaces;
using ESFA.DC.IO.SqlServer;
using ESFA.DC.IO.SqlServer.Config.Interfaces;
using Moq;

namespace ESFA.DC.IO.PerformanceTestHarness
{
    public static class Program
    {
        private const int Runs = 250;

        private static string data;

        private static Random random;

        private static List<GetSetRemove> azureStorage;

        private static List<GetSetRemove> azureStorageCompressed;

        private static List<GetSetRemove> fileSystem;

        private static List<GetSetRemove> redis;

        private static List<GetSetRemove> sqlServer;

        private static List<GetSetRemove> dictionary;

        private static List<GetSetRemove> tableStorage;

        private static List<GetSetRemove> azureCosmos;

        private static AzureCosmosKeyValuePersistenceService azureCosmosUnitTest;

        private static AzureTableStorageKeyValuePersistenceService azureTableStorageUnitTest;

        private static SqlServerKeyValuePersistenceService sqlServerUnitTest;

        private static RedisKeyValuePersistenceService redisUnitTest;

        private static FileSystemKeyValuePersistenceService fileSystemUnitTest;

        private static AzureStorageKeyValuePersistenceService azureStorageUnitTest;

        private static AzureStorageCompressedKeyValuePersistenceService azureStorageCompressedUnitTest;

        private static bool _failedStorage;
        private static bool _failedStorageCompressed;
        private static bool _failedFileSystem;
        private static bool _failedRedis;
        private static bool _failedSql;
        private static bool _failedDictionary;
        private static bool _failedAzureTable;
        private static bool _failedCosmos;

        private static PerformanceCounter cpuCounter;

        public static void Main(string[] args)
        {
            random = new Random();
            //logger = new SeriLogger(new ApplicationLoggerSettings
            //{
            //    ApplicationLoggerOutputSettingsCollection = new List<IApplicationLoggerOutputSettings>()
            //    {
            //        new ConsoleApplicationLoggerOutputSettings
            //        {
            //            MinimumLogLevel = LogLevel.Verbose
            //        }
            //    }
            //}, new ExecutionContext
            //{
            //    JobId = "A", TaskKey = "B "
            //});
            // const int StringLength = 10000;
            // data = GetRandomString(StringLength);
            data = GetFileAsString(@"C:\Users\DevUser\source\repos\DC-ILR-1819-DataStore\src\ESFA.DC.ILR1819.DataStore.PersistData.Test\ALBOutput1000.json");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // Baseline

            azureStorage = new List<GetSetRemove>();
            azureStorageCompressed = new List<GetSetRemove>();
            fileSystem = new List<GetSetRemove>();
            redis = new List<GetSetRemove>();
            sqlServer = new List<GetSetRemove>();
            dictionary = new List<GetSetRemove>();
            tableStorage = new List<GetSetRemove>();
            azureCosmos = new List<GetSetRemove>();

            ConsoleKeyInfo key;
            do
            {
                Console.WriteLine("[s]ingle or [m]ulti threaded");
                key = Console.ReadKey();
            } while (key.KeyChar != 's' && key.KeyChar != 'm');

            bool multi = key.KeyChar == 'm';

            Console.WriteLine($"Runs: {Runs}; String Length: {data.Length}; Multi: {multi}; Please wait...");

            var azureCosmosConfig = new Mock<IAzureCosmosKeyValuePersistenceServiceConfig>();
            string uri = ConfigurationManager.AppSettings["UriAzureCosmos"];
            string primaryKey = ConfigurationManager.AppSettings["PrimaryKeyAzureCosmos"];
            azureCosmosConfig.SetupGet(x => x.EndpointUrl).Returns(uri);
            azureCosmosConfig.SetupGet(x => x.AuthKeyOrResourceToken).Returns(primaryKey);
            azureCosmosUnitTest = new AzureCosmosKeyValuePersistenceService(azureCosmosConfig.Object);

            var azureTableStorageConfig = new Mock<IAzureTableStorageKeyValuePersistenceServiceConfig>();
            string azureTableStorageConnectionString = ConfigurationManager.AppSettings["ConnectionStringAzureTableStorage"];
            azureTableStorageConfig.SetupGet(x => x.ConnectionString).Returns(azureTableStorageConnectionString);
            azureTableStorageUnitTest = new AzureTableStorageKeyValuePersistenceService(azureTableStorageConfig.Object);

            var sqlServerConfig = new Mock<ISqlServerKeyValuePersistenceServiceConfig>();
            sqlServerConfig.SetupGet(x => x.ConnectionString).Returns(ConfigurationManager.AppSettings["ConnectionStringSqlServer"]);
            sqlServerUnitTest = new SqlServerKeyValuePersistenceService(sqlServerConfig.Object);

            var redisConfig = new Mock<IRedisKeyValuePersistenceServiceConfig>();
            redisConfig.SetupGet(x => x.ConnectionString).Returns(ConfigurationManager.AppSettings["ConnectionStringRedis"]);
            redisUnitTest = new RedisKeyValuePersistenceService(redisConfig.Object);

            var fileSystemConfig = new Mock<IFileSystemKeyValuePersistenceServiceConfig>();
            fileSystemConfig.SetupGet(x => x.Directory).Returns(Directory.GetCurrentDirectory);
            fileSystemUnitTest = new FileSystemKeyValuePersistenceService(fileSystemConfig.Object);

            var azureStorageConfig = new Mock<IAzureStorageKeyValuePersistenceServiceConfig>();
            string azureStorageConnectionString = ConfigurationManager.AppSettings["ConnectionStringAzureStorage"];
            azureStorageConfig.SetupGet(x => x.ConnectionString).Returns(azureStorageConnectionString);
            azureStorageUnitTest = new AzureStorageKeyValuePersistenceService(azureStorageConfig.Object);

            var azureStorageCompressedConfig = new Mock<IAzureStorageCompressedKeyValuePersistenceServiceConfig>();
            string azureStorageCompressedConnectionString = ConfigurationManager.AppSettings["ConnectionStringAzureStorage"];
            azureStorageCompressedConfig.SetupGet(x => x.ConnectionString).Returns(azureStorageCompressedConnectionString);
            azureStorageCompressedConfig.SetupGet(x => x.ValueEncoding).Returns(Encoding.UTF8);
            azureStorageCompressedUnitTest = new AzureStorageCompressedKeyValuePersistenceService(azureStorageCompressedConfig.Object);

            if (multi)
            {
                Parallel.For(0, Runs/*, new ParallelOptions() { MaxDegreeOfParallelism = 4 }*/, (i, loopState) =>
                {
                    try
                    {
                        RunTests(i).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        loopState.Break();
                    }
                });
            }
            else
            {
                for (int i = 0; i < Runs; i++)
                {
                    try
                    {
                        RunTests(i).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            List<Result> results = new List<Result>
            {
                new Result("Azure Storage", azureStorage, _failedStorage),
                new Result("Azure Storage Compressed", azureStorageCompressed, _failedStorageCompressed),
                new Result("File System", fileSystem, _failedFileSystem),
                new Result("Redis", redis, _failedRedis),
                new Result("SQL Server", sqlServer, _failedSql),
                new Result("Dictionary", dictionary, _failedDictionary),
                new Result("Azure Table", tableStorage, _failedAzureTable),
                new Result("Azure Cosmos", azureCosmos, _failedCosmos)
            };

            results.Sort();

            foreach (Result result in results)
            {
                Console.WriteLine(result);
            }

            Console.ReadKey();
        }

        private static string GetFileAsString(string filename)
        {
            return File.ReadAllText(filename);
        }

        private static async Task RunTests(int i)
        {
            try
            {
                if (!_failedStorage)
                {
                    await TestAzureStorage(i);
                }
            }
            catch (Exception ex)
            {
                _failedStorage = true;
            }

            try
            {
                if (!_failedStorageCompressed)
                {
                    await TestAzureStorageCompressed(i);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _failedStorageCompressed = true;
            }

            //try
            //{
            //    if (!_failedFileSystem)
            //    {
            //        await TestFileSystem(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _failedFileSystem = true;
            //}

            //try
            //{
            //    if (!_failedRedis)
            //    {
            //        await TestRedis(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _failedRedis = true;
            //}

            //try
            //{
            //    if (!_failedSql)
            //    {
            //        await TestSqlServer(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    _failedSql = true;
            //}

            //try
            //{
            //    if (!_failedDictionary)
            //    {
            //        await TestDictionary(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _failedDictionary = true;
            //}

            //try
            //{
            //    if (!_failedAzureTable)
            //    {
            //        await TestAzureTable(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _failedAzureTable = true;
            //}

            //try
            //{
            //    if (!_failedCosmos)
            //    {
            //        await TestAzureCosmos(i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    _failedCosmos = true;
            //}
        }

        private static async Task TestAzureCosmos(int i)
        {
            string Key = $"1_2_{i}";

            cpuCounter.NextValue();
            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await azureCosmosUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureCosmosUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureCosmosUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            getSetRemove.CpuCount = cpuCounter.NextValue();
            azureCosmos.Add(getSetRemove);
        }

        private static async Task TestAzureTable(int i)
        {
            string Key = $"1_2_{i}";

            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await azureTableStorageUnitTest.SaveAsync(Key, data);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureTableStorageUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureTableStorageUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            tableStorage.Add(getSetRemove);
        }

        private static async Task TestSqlServer(int i)
        {
            string Key = $"1_{i}_{i}";

            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await sqlServerUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await sqlServerUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await sqlServerUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            sqlServer.Add(getSetRemove);
        }

        private static async Task TestRedis(int i)
        {
            string Key = $"1_2_{i}";

            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await redisUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await redisUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await redisUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            redis.Add(getSetRemove);
        }

        private static async Task TestFileSystem(int i)
        {
            string Key = $"1_2_{i}";

            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await fileSystemUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await fileSystemUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await fileSystemUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            fileSystem.Add(getSetRemove);
        }

        private static async Task TestAzureStorage(int i)
        {
            string Key = $"1_2_{i}";

            cpuCounter.NextValue();
            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await azureStorageUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureStorageUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureStorageUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            getSetRemove.CpuCount = cpuCounter.NextValue();
            azureStorage.Add(getSetRemove);
        }

        private static async Task TestAzureStorageCompressed(int i)
        {
            string Key = $"1_2_{i}";

            cpuCounter.NextValue();
            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await azureStorageCompressedUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureStorageCompressedUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await azureStorageCompressedUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            getSetRemove.CpuCount = cpuCounter.NextValue();
            azureStorageCompressed.Add(getSetRemove);
        }

        private static async Task TestDictionary(int i)
        {
            string Key = $"1_2_{i}";
            var dictionaryUnitTest = new DictionaryKeyValuePersistenceService();

            GetSetRemove getSetRemove = new GetSetRemove();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await dictionaryUnitTest.SaveAsync(Key, data);
            getSetRemove.Set = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await dictionaryUnitTest.GetAsync(Key);
            getSetRemove.Get = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            await dictionaryUnitTest.RemoveAsync(Key);
            getSetRemove.Remove = stopwatch.ElapsedMilliseconds;
            dictionary.Add(getSetRemove);
        }

        private static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

//mRuns: 250; String Length: 10000; Multi: True; Please wait...(S2)
//Azure Storage: 769375; Average: 3077.5
//File System: 23056; Average: 92.224
//Redis: 123682; Average: 494.728
//SQL Server: 93336; Average: 373.344
//Dictionary: 59; Average: 0.236
//Azure Table: 496381; Average: 1985.524

//mRuns: 250; String Length: 10000; Multi: True; Please wait... (P1)
//Azure Storage: 780452; Average: 3121.808
//File System: 18243; Average: 72.972
//Redis: 161507; Average: 646.028
//SQL Server: 76928; Average: 307.712
//Dictionary: 31; Average: 0.124
//Azure Table: 485657; Average: 1942.628

//mRuns: 250; String Length: 10000; Multi: True; Please wait...
//Azure Storage: 831081; Average: 3324.324
//File System: 17565; Average: 70.26
//Redis: 112943; Average: 451.772
//SQL Server: 42310; Average: 169.24
//Dictionary: 21; Average: 0.084
//Azure Table: 602788; Average: 2411.152

//mRuns: 250; String Length: 10000; Multi: True; Please wait... (ServicePoint endpoint increase)
//Azure Storage: 836095; Average: 3344.38
//File System: 12128; Average: 48.512
//Redis: 102444; Average: 409.776
//SQL Server: 49858; Average: 199.432
//Dictionary: 21; Average: 0.084
//Azure Table: 134171; Average: 536.684

//mRuns: 250; String Length: 100; Multi: True; Please wait...
//Azure Storage: 2081425; Average: 8325.7
//File System: 8992; Average: 35.968
//Redis: 116247; Average: 464.988
//SQL Server: 44855; Average: 179.42
//Dictionary: 38; Average: 0.152
//Azure Table: 217997; Average: 871.988