using Microsoft.WindowsAzure.Storage.Table;

namespace GuidedSearchBot.Models
{
    public class MandatoryCategoryMapping : TableEntity
    {
        public MandatoryCategoryMapping(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public MandatoryCategoryMapping() { }

        public string MappingFor { get; set; }

        public string MandatoryCategories { get; set; }
    }
}
