using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class LoaiVeRequest
    {
        public class GetLoaiVeByNameRequest
        {
            public string Ten { get; set; } = string.Empty;
        }

        public class GetLoaiVeByEventRequest
        {
            public string TenSuKien { get; set; } = string.Empty;
        }
    }
}
