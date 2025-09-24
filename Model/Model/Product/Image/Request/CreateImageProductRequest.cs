using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Image.Request
{
    public class CreateImageProductRequest
    {
        public IFormFile Image { get; set; } = null;
        public Guid product_id { get; set; }
    }
}
