using Microsoft.Azure.Cosmos.Table;

namespace CLDV6212_POE.Models
{
    public class Customer : TableEntity
    {
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public string ProductData { get; set; }

        public Customer() { }

        public Customer(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}