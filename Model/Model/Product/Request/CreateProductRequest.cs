using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Model.Product.Request
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ProductCategoryRequest> Category { get; set; }
        public float Minimum_price { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public Guid Admin_id { get; set; }
        public Guid Created_by { get; set; }
    }
}
