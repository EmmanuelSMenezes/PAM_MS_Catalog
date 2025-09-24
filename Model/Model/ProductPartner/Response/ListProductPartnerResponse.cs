using System.Collections.Generic;

namespace Domain.Model
{
    public class ListProductPartnerResponse
    {
        public List<ProductPartner> Products { get; set; }
        public Pagination Pagination { get; set; }
    }
}
