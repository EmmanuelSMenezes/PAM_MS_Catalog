using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class CreateImageProductPartnerRequest
    {
        public Guid Product_id { get; set; }
        public Guid Partner_id { get; set; }
        public string Imagedefaultname { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
