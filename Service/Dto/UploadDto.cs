using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class UploadDto
    {
        public int UserID { get; set; }
        public string Imagebase64 { get; set; }
    }
}
