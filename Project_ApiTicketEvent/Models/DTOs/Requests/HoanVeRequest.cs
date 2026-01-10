using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class HoanVeRequest
    {
        public string? LyDo { get; set; }
        public string? PhuongThuc { get; set; } = "REFUND_MOCK";
        public string? RawResponse { get; set; }
    }
}
