using System.Collections.Generic;

namespace Domain.Model
{
    public class ListCategoryResponse
    {
        public List<Category> categories { get; set; }
        public Pagination Paginations { get; set; }
    }
}
