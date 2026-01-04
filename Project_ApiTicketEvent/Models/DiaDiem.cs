using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DiaDiem
    {
        public int DiaDiemID { get; set; }
        public string? TenDiaDiem { get; set; }
        public string DiaChi { get; set; } = string.Empty;
        public int? SucChua { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; }
    }
}
