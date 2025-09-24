using System;
using System.Collections.Generic;

namespace Domain.Model
{
  public class CreateProductPartnerRequest
  {
        public Guid Base_product_id { get; set; }
        public Guid Branch_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Guid Created_by { get; set; }
    }
}

