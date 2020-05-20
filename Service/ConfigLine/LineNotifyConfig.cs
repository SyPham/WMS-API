using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.ConfigLine
{
    public class LineNotifyConfig
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string redirect_uri { get; set; }
    }
}
