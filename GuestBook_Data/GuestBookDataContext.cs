using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestBook_Data
{
    public class GuestBookDataContext : TableServiceContext
    {
        public IQueryable<GuestBookEntry> GuestBookEntry
        {
            get
            {
                return this.CreateQuery<GuestBookEntry>("GuestBookEntry");
            }
        }
        public GuestBookDataContext(string baseAddress, StorageCredentials credentials) : base(baseAddress, credentials)
        {
        }
    }
}
