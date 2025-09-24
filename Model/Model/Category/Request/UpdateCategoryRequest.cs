using System;

namespace Domain.Model
{
  public class UpdateCategoryRequest
  {
        public Guid Category_id { get; set; }
        public string Description { get; set; }
        public Guid? Category_parent_id { get; set; }
        public bool Active { get; set; }
        public Guid? Updated_by { get; set; }
    }
}

