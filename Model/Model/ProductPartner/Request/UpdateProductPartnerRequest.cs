using System;
using System.Collections.Generic;

namespace Domain.Model
{
  public class UpdateProductPartnerRequest
  {
        public Guid Product_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Sale_price { get; set; }
        public bool Active { get; set; }
        public Guid Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Reviewer { get; set; }
    }
}

