using System;

namespace Domain.Model
{
  public class CreateCategoryRequest
  {
        public string Description { get; set; }
        public Guid? Category_parent_id { get; set; }
        public Guid Created_by { get; set; }
    }
}

