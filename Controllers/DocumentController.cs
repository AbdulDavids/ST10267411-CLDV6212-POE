using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using Microsoft.Extensions.Configuration;
using CLDV6212_POE.Models;

namespace CLDV6212_POE.Controllers
{
    public class DocumentController : Controller
    {
        private readonly CloudFileShare _fileShare;

        public DocumentController(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=st10267411;AccountKey=YbxBY2L0rBJdR3DWMu7xspuF6wWROTKaNw/2sjcR5ZNJSY4EuU7WWpwR6XOQ35YgNs+2wc9Ph9gP+AStQxV6Vw==;EndpointSuffix=core.windows.net");
            var fileClient = storageAccount.CreateCloudFileClient();
            _fileShare = fileClient.GetShareReference("documents");
            _fileShare.CreateIfNotExists();
        }

        public async Task<IActionResult> Index()
        {
            var documents = new List<Document>();
            var rootDir = _fileShare.GetRootDirectoryReference();
            var files = await rootDir.ListFilesAndDirectoriesSegmentedAsync(null);
            foreach (var fileItem in files.Results.OfType<CloudFile>())
            {
                await fileItem.FetchAttributesAsync();
                documents.Add(new Document { FileName = fileItem.Name, Size = fileItem.Properties.Length, UploadDate = fileItem.Properties.LastModified?.DateTime ?? DateTime.MinValue });
            }
            return View(documents);
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
                var rootDir = _fileShare.GetRootDirectoryReference();
                var cloudFile = rootDir.GetFileReference(file.FileName);
                await cloudFile.UploadFromStreamAsync(file.OpenReadStream());
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(string fileName)
        {
            var rootDir = _fileShare.GetRootDirectoryReference();
            var cloudFile = rootDir.GetFileReference(fileName);
            var memoryStream = new MemoryStream();
            await cloudFile.DownloadToStreamAsync(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/octet-stream", fileName);
        }
    }
}
