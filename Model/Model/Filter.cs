using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Model
{
    public class Filter
    {
        public string? filter { get; set; }
        public int? page { get; set; }
        public int? itensPerPage { get; set; }
    }
}
