using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Reponses
{
    public class VeResponse
    {
        public int VeID { get; set; }
        public string MaVe { get; set; } = string.Empty;
        public string QrToken { get; set; } = string.Empty;

        public byte TrangThai { get; set; }

        public int DonHangID { get; set; }
        public int LoaiVeID { get; set; }

        // Thông tin hiển thị thêm (join)
        public string? TenLoaiVe { get; set; }
        public decimal? DonGia { get; set; }

        public int? SuKienID { get; set; }
        public string? TenSuKien { get; set; }
    }
}
