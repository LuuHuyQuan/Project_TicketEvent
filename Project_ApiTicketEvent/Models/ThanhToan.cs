using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ThanhToan
    {
        public int ThanhToanID { get; set; }
        public int DonHangID { get; set; }
        public string? MaGiaoDich { get; set; }
        public string PhuongThuc { get; set; } = string.Empty; // VNPAY, MOMO, CASH
        public decimal SoTien { get; set; }
        public byte TrangThai { get; set; } // TINYINT: 0,1,2,3
        public DateTime? ThoiGianThanhToan { get; set; }
        public string? RawResponse { get; set; }
    }
}
