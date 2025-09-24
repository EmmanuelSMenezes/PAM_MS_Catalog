using Microsoft.AspNetCore.Http;

namespace Domain.Model.Product.Image.Request
{
    public class UploadImageRequest
    {
        public IFormFile File { get; set; }
        public int bucketId = 2;
    }
}