using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class ThanhToanRequest
    {
        public string? PhuongThuc { get; set; } = "MOCK";
        public string? RawResponse { get; set; } // muốn lưu log thì gửi
    }
}
