using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using CLDV6212_POE.Models;

namespace CLDV6212_POE.Controllers
{
    public class OrderController : Controller
    {
        private readonly CloudQueue _orderQueue;
        private readonly CloudTable _customerTable;

        private const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=st10267411;AccountKey=YbxBY2L0rBJdR3DWMu7xspuF6wWROTKaNw/2sjcR5ZNJSY4EuU7WWpwR6XOQ35YgNs+2wc9Ph9gP+AStQxV6Vw==;EndpointSuffix=core.windows.net";

        public OrderController()
        {
            try
            {
                // Create separate storage account variables for Table and Queue storage
                var queueStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(StorageConnectionString);
                var queueClient = queueStorageAccount.CreateCloudQueueClient();
                _orderQueue = queueClient.GetQueueReference("orders");
                _orderQueue.CreateIfNotExists();

                var tableStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(StorageConnectionString);
                var tableClient = tableStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
                _customerTable = tableClient.GetTableReference("customers");
                _customerTable.CreateIfNotExists();

                Console.WriteLine("Storage accounts initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing storage accounts: {ex.Message}");
                throw;
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await _orderQueue.PeekMessagesAsync(32);
                var orders = messages.Select(m => JsonConvert.DeserializeObject<Order>(m.AsString)).ToList();

                // Fetch customer names for each order
                foreach (var order in orders)
                {
                    var customer = await GetCustomerById(order.CustomerId);
                    if (customer != null)
                    {
                        order.CustomerName = customer.Name;
                    }
                }

                Console.WriteLine($"Fetched {orders.Count} orders from queue.");
                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders from queue: {ex.Message}");
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Customers = await GetCustomers();
                Console.WriteLine("Fetched customers for order creation.");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customers: {ex.Message}");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order, List<string> productNames, List<decimal> productPrices)
        {
            if (true)
            {
                try
                {
                    // Add products to the order
                    for (int i = 0; i < productNames.Count; i++)
                    {
                        var product = new Product
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = productNames[i],
                            Price = productPrices[i]
                        };
                        order.Products.Add(product);
                    }

                    // Generate the Id for the order
                    order.Id = Guid.NewGuid().ToString();
                    order.TimeQueued = DateTime.UtcNow;

                    var message = new CloudQueueMessage(JsonConvert.SerializeObject(order));
                    await _orderQueue.AddMessageAsync(message);

                    Console.WriteLine($"Order created and added to queue: {order.Id}");
                    Console.WriteLine($"Order details: {JsonConvert.SerializeObject(order)}");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating order: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                }
            }

            try
            {
                ViewBag.Customers = await GetCustomers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customers: {ex.Message}");
            }

            return View(order);
        }

        public async Task<IActionResult> Process()
        {
            try
            {
                var messages = await _orderQueue.GetMessagesAsync(32);
                foreach (var message in messages)
                {
                    // Process the message
                    await _orderQueue.DeleteMessageAsync(message);
                }
                Console.WriteLine($"Processed {messages.Count()} messages.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing orders: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        private async Task<List<Customer>> GetCustomers()
        {
            var query = new TableQuery<Customer>();
            var customers = new List<Customer>();
            TableContinuationToken token = null;

            do
            {
                var resultSegment = await _customerTable.ExecuteQuerySegmentedAsync(query, token);
                customers.AddRange(resultSegment.Results);
                token = resultSegment.ContinuationToken;
            } while (token != null);

            return customers;
        }

        private async Task<Customer> GetCustomerById(string customerId)
        {
            var retrieveOperation = TableOperation.Retrieve<Customer>("Customer", customerId);
            var result = await _customerTable.ExecuteAsync(retrieveOperation);
            return result.Result as Customer;
        }

    }
}
