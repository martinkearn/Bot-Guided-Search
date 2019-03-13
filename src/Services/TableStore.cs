﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BasicBot.Interfaces;
using BasicBot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BasicBot.Services
{
    public class TableStore : ITableStore
    {
        // Container names
        public const string _mandatoryCategoriesContainerName = "mandatorycategories";
        public const string _mandatoryCategoriesPartitionKey = "mandatorycategorymapping";
        public const string _mappingForPropertyName = "MappingFor";

        private readonly BotServices _botServices;

        public TableStore(BotServices botServices)
        {
            _botServices = botServices;
        }

        public async Task<IEnumerable<string>> GetMandatoryCategories(string mappingFor)
        {
            try
            {
                //var mappingForLower = mappingFor.ToLower();
                //var table = await GetTableContainer(_mandatoryCategoriesContainerName);

                //TableQuery<TableEntityAdapter<MandatoryCategoryMapping>> query = new TableQuery<TableEntityAdapter<MandatoryCategoryMapping>>();
                //if (!string.IsNullOrEmpty(mappingFor))
                //{
                //    query = new TableQuery<TableEntityAdapter<MandatoryCategoryMapping>>().Where(TableQuery.GenerateFilterCondition(_mappingForPropertyName, QueryComparisons.Equal, mappingForLower));
                //}

                //// Initialize the continuation token to null to start from the beginning of the table.
                //TableContinuationToken continuationToken = null;
                //var entities = new List<TableEntityAdapter<MandatoryCategoryMapping>>();
                //do
                //{
                //    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                //    entities.AddRange(queryResult.Results);
                //    continuationToken = queryResult.ContinuationToken;

                //    // Loop until a null continuation token is received, indicating the end of the table.
                //} while (continuationToken != null);

                //// get a list of mandatory categories
                //var mandatoryCategories = new List<string>();
                //foreach (var entity in entities)
                //{
                //    var manCatsInEntity = entity.OriginalEntity.MandatoryCategories.Split(',');

                //    foreach (var manCatInEntity in manCatsInEntity)
                //    {
                //        mandatoryCategories.Add(manCatInEntity);
                //    }
                //}

                //// return
                //return mandatoryCategories;
                return null;
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
                //var table = await GetTableContainer(_mandatoryCategoriesContainerName);

                //TableEntityAdapter<MandatoryCategoryMapping> entity = new TableEntityAdapter<MandatoryCategoryMapping>(item, _mandatoryCategoriesPartitionKey, item.Id);

                //TableOperation insertOperation = TableOperation.InsertOrReplace(entity);

                //TableResult result = await table.ExecuteAsync(insertOperation);

                //return entity.OriginalEntity;
                return null;
            }
            catch
            {
                return null;
            }
        }

        //private async Task<CloudTable> GetTableContainer(string containerName)
        //{
        //    string storageConnectionString = _botServices.StorageConnectionString;

        //    if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
        //    {
        //        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        //        var cloudTable = tableClient.GetTableReference(containerName);
        //        await cloudTable.CreateIfNotExistsAsync();
        //        return cloudTable;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
