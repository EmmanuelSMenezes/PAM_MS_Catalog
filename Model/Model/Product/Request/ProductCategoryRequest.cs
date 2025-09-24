using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Request
{
    public class ProductCategoryRequest
    {
        public Guid Category_id { get; set; }
        public Guid? Category_parent_id { get; set; }
    }
}
