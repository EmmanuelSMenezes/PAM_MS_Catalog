using System;

namespace Domain.Model
{
  public class Category
    {
        public Guid Category_id { get; set; } 
        public int Identifier { get; set; } 
	    public string Description { get; set; }
        public string Category_parent_name{ get; set; }
        public Guid? Category_parent_id { get; set; }
        public Guid Created_by { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public bool Active { get; set; }
    }
}
