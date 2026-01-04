using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class NguoiDung
    {
        public int NguoiDungId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MatKhauHash { get; set; } = string.Empty;

        public int? VaiTroId { get; set; }
        public DateTime? NgayTao { get; set; }
        public bool? TrangThai { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
    }
}
