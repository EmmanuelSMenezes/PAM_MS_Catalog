
using System;
using System.Collections.Generic;

namespace Domain.Model
{
    public class ProductPartner
    {
        public Guid Product_id { get; set; }
        public Guid Partner_id { get; set; }
        public Guid? Image_default { get; set; }
        public int Identifier { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Sale_price { get; set; }
        public decimal Minimum_price { get; set; }
        public List<ImageProduct> Images { get; set; } = new List<ImageProduct>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public bool Active { get; set; }
        public Guid Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public bool Reviewer { get; set; }
    }
}
