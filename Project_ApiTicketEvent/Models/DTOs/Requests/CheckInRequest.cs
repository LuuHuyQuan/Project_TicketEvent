using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class CheckInRequest
    {
        public string? QrToken { get; set; }
        public string? MaVe { get; set; } // optional (nếu muốn check-in bằng mã vé)
        public int NhanVienID { get; set; }
        public string? GhiChu { get; set; }
    }
}
