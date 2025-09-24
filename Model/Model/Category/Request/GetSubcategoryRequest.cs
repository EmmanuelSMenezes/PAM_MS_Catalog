using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class GetSubcategoryRequest
    {
        public List<Guid> Category_ids {  get; set; }
    }
}
