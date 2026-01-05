using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DonHang
    {
        public int DonHangID { get; set; }
        public int NguoiMuaID { get; set; }
        public int SuKienID { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public byte TrangThai { get; set; } // tinyint: 0..3
    }
    public class DonHangChiTiet
    {
        public int ChiTietID { get; set; }
        public int DonHangID { get; set; }
        public int LoaiVeID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class DonHangDetail : DonHang
    {
        public List<DonHangChiTiet> Items { get; set; } = new();
    }
}
