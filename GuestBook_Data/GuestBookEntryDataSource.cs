using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestBook_Data
{
    public class GuestBookEntryDataSource
    {
        private static CloudStorageAccount storageAccount;
        private GuestBookDataContext context;

        public GuestBookEntryDataSource()
        {
            this.context = new GuestBookDataContext(storageAccount.TableEndpoint.AbsoluteUri,
            storageAccount.Credentials);
            this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }

        static GuestBookEntryDataSource()
        {
            storageAccount = CloudStorageAccount.FromConfigurationSetting(
            "DataConnectionString");
            CloudTableClient.CreateTablesFromModel(
            typeof(GuestBookDataContext),
            storageAccount.TableEndpoint.AbsoluteUri,
            storageAccount.Credentials);
        }

        public IEnumerable<GuestBookEntry> Select()
        {
            var results = from g in this.context.GuestBookEntry
                          where g.PartitionKey == DateTime.UtcNow.ToString("MMddyyyy")
                          select g;
            return results;
        }
        public void UpdateImageThumbnail(string partitionKey, string rowKey, string thumbUrl)
        {
            var results = from g in this.context.GuestBookEntry
                          where g.PartitionKey == partitionKey && g.RowKey == rowKey
                          select g;
            var entry = results.FirstOrDefault<GuestBookEntry>();
            entry.ThumbnailUrl = thumbUrl;
            this.context.UpdateObject(entry);
            this.context.SaveChanges();
        }
        public void AddGuestBookEntry(GuestBookEntry newItem)
        {
            this.context.AddObject("GuestBookEntry", newItem);
            this.context.SaveChanges();
        }
    }
}
