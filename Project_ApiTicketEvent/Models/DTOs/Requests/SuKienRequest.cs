using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class SuKienDto
    {
        public int SuKienID { get; set; }
        public string TenSuKien { get; set; } = string.Empty;

        public int DanhMucID { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;

        public int? DiaDiemID { get; set; }
        public string? TenDiaDiem { get; set; }

        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }

        public string? MoTa { get; set; }
        public string? AnhBia { get; set; }

        public bool TrangThai { get; set; }
    }
}
