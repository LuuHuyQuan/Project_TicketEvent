using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LoaiVe
    {
        public int LoaiVeID { get; set; }
        public int SuKienID { get; set; }

        public string TenLoaiVe { get; set; } = string.Empty;

        public decimal GiaVe { get; set; }
        public int? SoLuong { get; set; }

        public string? MoTa { get; set; }

        // DB của bạn hay là tinyint nên dùng byte để khỏi lỗi cast
        public byte TrangThai { get; set; }
    }
}
