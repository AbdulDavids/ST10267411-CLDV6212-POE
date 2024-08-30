using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using CLDV6212_POE.Models;

namespace CLDV6212_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CloudTable _table;

        public CustomerController()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=st10267411;AccountKey=YbxBY2L0rBJdR3DWMu7xspuF6wWROTKaNw/2sjcR5ZNJSY4EuU7WWpwR6XOQ35YgNs+2wc9Ph9gP+AStQxV6Vw==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _table = tableClient.GetTableReference("Customers");
            _table.CreateIfNotExists();
        }

        public async Task<ActionResult> Index()
        {
            var query = new TableQuery<Customer>();
            var customers = await _table.ExecuteQuerySegmentedAsync(query, null);
            return View(customers.Results);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Customer customer)
        {
            customer.PartitionKey = "Customer";
            customer.RowKey = Guid.NewGuid().ToString();
            var insertOperation = TableOperation.Insert(customer);
            await _table.ExecuteAsync(insertOperation);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(retrieveOperation);
            var customer = result.Result as Customer;
            return View(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Customer customer)
        {
            var replaceOperation = TableOperation.Replace(customer);
            await _table.ExecuteAsync(replaceOperation);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(retrieveOperation);
            var customer = result.Result as Customer;
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(retrieveOperation);
            var customer = result.Result as Customer;

            if (customer != null)
            {
                var deleteOperation = TableOperation.Delete(customer);
                await _table.ExecuteAsync(deleteOperation);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Search(string searchTerm)
        {
            var query = new TableQuery<Customer>().Where(TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, searchTerm));
            var customers = await _table.ExecuteQuerySegmentedAsync(query, null);
            return View("Index", customers.Results);
        }
    }
}
