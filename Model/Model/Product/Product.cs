using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Model.Product
{
  public class Product
    {
        public Guid Product_id { get; set; }
        public int Identifier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public decimal Minimum_price { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public Guid Admin_id { get; set; }
        public string Url { get; set; }
        public Guid Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_at { get; set; }
    }
}
