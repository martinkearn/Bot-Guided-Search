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

        private readonly BotServices _botServices;

        public TableStore(BotServices botServices)
        {
            _botServices = botServices;
        }

        public async Task<MandatoryCategoryMapping> UpsertMandatoryCategoryMapping(MandatoryCategoryMapping item)
        {
            // Call this as folows:
            // ***
            //var catMap = new MandatoryCategoryMapping()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    MappingFor = "Surface",
            //    MandatoryCategories = "Product,Colour",
            //};

            //await _tableStore.UpsertMandatoryCategoryMapping(catMap);
            // ***


            try
            {
                var table = await GetTableContainer(_mandatoryCategoriesContainerName);

                TableEntityAdapter<MandatoryCategoryMapping> entity = new TableEntityAdapter<MandatoryCategoryMapping>(item, _mandatoryCategoriesPartitionKey, item.Id);

                TableOperation insertOperation = TableOperation.InsertOrReplace(entity);

                TableResult result = await table.ExecuteAsync(insertOperation);

                return entity.OriginalEntity;
            }
            catch
            {
                return null;
            }
        }

        private async Task<CloudTable> GetTableContainer(string containerName)
        {
            string storageConnectionString = _botServices.StorageConnectionString;

            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
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
