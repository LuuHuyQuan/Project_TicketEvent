using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class ThanhToanRequest
    {
        public class TaoThanhToanRequest
        {
            public int NguoiMuaID { get; set; }
            public int DonHangID { get; set; }
            public string PhuongThuc { get; set; } = "VNPAY";
            public string? MaGiaoDich { get; set; }
            public string? RawResponse { get; set; }
        }

        public class XacNhanThanhToanRequest
        {
            public int NguoiMuaID { get; set; }
            public int DonHangID { get; set; }
            public string PhuongThuc { get; set; } = "VNPAY";
            public string? MaGiaoDich { get; set; }
            public bool ThanhCong { get; set; } = true;   // true: success, false: failed
            public string? RawResponse { get; set; }
        }
    }
}
