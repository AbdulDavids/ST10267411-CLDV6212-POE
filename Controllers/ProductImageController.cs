using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using CLDV6212_POE.Models;

namespace CLDV6212_POE.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly CloudBlobContainer _blobContainer;

        public ProductImageController(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=st10267411;AccountKey=YbxBY2L0rBJdR3DWMu7xspuF6wWROTKaNw/2sjcR5ZNJSY4EuU7WWpwR6XOQ35YgNs+2wc9Ph9gP+AStQxV6Vw==;EndpointSuffix=core.windows.net");
            var blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("productimages");
            _blobContainer.CreateIfNotExists();
        }

        public async Task<IActionResult> Index()
        {
            var blobs = new List<ProductImage>();
            BlobContinuationToken continuationToken = null;
            do
            {
                var resultSegment = await _blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = resultSegment.ContinuationToken;
                foreach (var blobItem in resultSegment.Results.OfType<CloudBlockBlob>())
                {
                    blobs.Add(new ProductImage
                    {
                        FileName = blobItem.Name,
                        UploadDate = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                        Url = blobItem.Uri.ToString() // Generate URL for the image
                    });
                }
            } while (continuationToken != null);

            return View(blobs);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var blob = _blobContainer.GetBlockBlobReference(file.FileName);
                await blob.UploadFromStreamAsync(file.OpenReadStream());
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string fileName)
        {
            var blob = _blobContainer.GetBlockBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
            return RedirectToAction("Index");
        }
    }
}
