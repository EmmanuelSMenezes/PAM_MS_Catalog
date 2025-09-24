using System;
using System.Collections.Generic;

namespace Domain.Model
{
    public class ProductPartnerResponse
    {
        public Guid Branch_id { get; set; }
        public string Branch_name { get; set; }
        public ProductPartner Product { get; set; }
       
    }
}
