using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Request
{
    public class UpdateProductRequest
    {
        public Guid Product_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ProductCategoryRequest> Category { get; set; }
        public float Minimum_price { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; } 
        public Guid Updated_by { get; set; }
    }
}
