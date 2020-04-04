using GuestBook_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuestBook_WebRole.Models
{
    public class UploadFileModel
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public HttpPostedFileBase File { get; set; }
        public IEnumerable<GuestBookEntry> Files { get; set; }
}
}