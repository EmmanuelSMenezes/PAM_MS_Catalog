using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Domain.Model
{
    public class UploadImagePartnerRequest
    {
        public List<IFormFile> File { get; set; }
        public int bucketId = 2;
    }
}