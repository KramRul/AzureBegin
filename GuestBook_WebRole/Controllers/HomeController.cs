using GuestBook_Data;
using GuestBook_WebRole.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GuestBook_WebRole.Controllers
{
    public class HomeController : Controller
    {
        private static bool storageInitialized = false;
        private static object gate = new Object();
        private static CloudBlobClient blobStorage;
        private static CloudQueueClient queueStorage;

        // GET: Home
        public ActionResult Index()
        {
            GuestBookEntryDataSource ds = new GuestBookEntryDataSource();
            return View(new UploadFileModel()
            {
                Name = string.Empty,
                Message = string.Empty,
                File = null,
                Files = ds.Select().ToList()
            });
        }

        [HttpPost]
        public ActionResult Upload(UploadFileModel model)
        {
            if (model.File.ContentLength > 0)
            {
                InitializeStorage();
                // upload the image to blob storage
                CloudBlobContainer container = blobStorage.GetContainerReference(
                "guestbookpics");
                string uniqueBlobName = string.Format("image_ {0} .jpg ", Guid.NewGuid().ToString
());
                CloudBlockBlob blob = container.GetBlockBlobReference(uniqueBlobName);
                blob.Properties.ContentType = model.File.ContentType;
                blob.UploadFromStream(model.File.InputStream);
                System.Diagnostics.Trace.TraceInformation("Uploaded image'{0}' to blob storage as '{1}'", model.File.FileName, uniqueBlobName);
                // create a new entry in table storage
                GuestBookEntry entry = new GuestBookEntry()
                {
                    GuestName = model.Name,
                    Message = model.Message,
                    PhotoUrl = blob.Uri.ToString(),
                    ThumbnailUrl =
                blob.Uri.ToString()
                };
                GuestBookEntryDataSource ds = new GuestBookEntryDataSource();
                ds.AddGuestBookEntry(entry);

                System.Diagnostics.Trace.TraceInformation("Added entry {0} - {1} in table storage for guest {2}", entry.PartitionKey, entry.RowKey, entry.GuestName);
            }
            return RedirectToAction("Index");
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            //DataList1.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            /*if (!Page.IsPostBack)
            {
                Timer1.Enabled = true;
            }*/
        }

        private void InitializeStorage()
        {
            if (storageInitialized)
            {
                return;
            }
            lock (gate)
            {
                if (storageInitialized)
                {
                    return;
                }
                try
                {
                    // read account configuration settings
                    var storageAccount = CloudStorageAccount.FromConfigurationSetting(
                    "DataConnectionString");
                    // create blob container for images
                    blobStorage = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobStorage.GetContainerReference(
                    "guestbookpics");
                    container.CreateIfNotExist();
                    // configure container for public access
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);

                    // create queue to communicate with worker role
                    queueStorage = storageAccount.CreateCloudQueueClient();
                    CloudQueue queue = queueStorage.GetQueueReference("guestthumbs");
                    queue.CreateIfNotExist();
                }
                catch (WebException)
                {
                    throw new WebException(@"Storage services initialization failure.
                    Check your storage account configuration settings. If running locally,
                    Ensure that the Development Storage service is running.");
                }
                storageInitialized = true;
            }
        }
    }
}