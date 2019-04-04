using System.Collections.Generic;
using System.Threading.Tasks;
using GuidedSearchBot.Interfaces;
using GuidedSearchBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GuidedSearchBot.Services
{
    public class TableStore : ITableStore
    {
        public readonly string _mandatoryCategoriesContainerName ;
        public readonly string _mandatoryCategoriesPartitionKey;
        public readonly string _mappingForPropertyName;
        private readonly string _storageConnectionString;

        public TableStore(IConfiguration configuration)
        {
            _mandatoryCategoriesContainerName = configuration["MandatoryCategoriesContainer"];
            _mandatoryCategoriesPartitionKey = configuration["MandatoryCategoriesPartitionKey"];
            _mappingForPropertyName = configuration["MandatoryCategoriesMappingForProperty"];
            _storageConnectionString = configuration["StorageConnectionString"];
        }

        public async Task<IEnumerable<string>> GetMandatoryCategories(string mappingFor)
        {
            try
            {
                var mappingForLower = mappingFor.ToLower();
                var table = await GetTableContainer(_mandatoryCategoriesContainerName);

                TableQuery<MandatoryCategoryMapping> query = new TableQuery<MandatoryCategoryMapping>();
                if (!string.IsNullOrEmpty(mappingFor))
                {
                    query = new TableQuery<MandatoryCategoryMapping>().Where(TableQuery.GenerateFilterCondition(_mappingForPropertyName, QueryComparisons.Equal, mappingForLower));
                }

                // Initialize the continuation token to null to start from the beginning of the table.
                TableContinuationToken continuationToken = null;
                var entities = new List<MandatoryCategoryMapping>();
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    entities.AddRange(queryResult.Results);
                    continuationToken = queryResult.ContinuationToken;

                    // Loop until a null continuation token is received, indicating the end of the table.
                } while (continuationToken != null);

                // get a list of mandatory categories
                var mandatoryCategories = new List<string>();
                foreach (var entity in entities)
                {
                    var manCatsInEntity = entity.MandatoryCategories.Split(',');

                    foreach (var manCatInEntity in manCatsInEntity)
                    {
                        mandatoryCategories.Add(manCatInEntity);
                    }
                }

                // return
                return mandatoryCategories;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<MandatoryCategoryMapping> UpsertMandatoryCategoryMapping(MandatoryCategoryMapping item)
        {
            try
            {
                var table = await GetTableContainer(_mandatoryCategoriesContainerName);

                TableOperation insertOperation = TableOperation.InsertOrReplace(item);

                TableResult result = await table.ExecuteAsync(insertOperation);

                return item;
            }
            catch
            {
                return null;
            }
        }

        private async Task<CloudTable> GetTableContainer(string containerName)
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out CloudStorageAccount storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                var cloudTable = tableClient.GetTableReference(containerName);
                await cloudTable.CreateIfNotExistsAsync();
                return cloudTable;
            }
            else
            {
                return null;
            }
        }
    }
}
