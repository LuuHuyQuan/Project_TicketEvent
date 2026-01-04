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
        public string TenLoaiVe { get; set; } = null!;
        public string? MoTa { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongToiDa { get; set; }
        public int SoLuongDaBan { get; set; }
        public int? GioiHanMoiKhach { get; set; }
        public DateTime? ThoiGianMoBan { get; set; }
        public DateTime? ThoiGianDongBan { get; set; }
        public bool TrangThai { get; set; }
    }
}
