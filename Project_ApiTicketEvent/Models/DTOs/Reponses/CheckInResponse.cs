using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Reponses
{
    public class CheckInResponse
    {
        public int VeID { get; set; }
        public string MaVe { get; set; } = string.Empty;
        public string QrToken { get; set; } = string.Empty;

        public int SuKienID { get; set; }
        public string TenSuKien { get; set; } = string.Empty;

        public int LoaiVeID { get; set; }
        public string TenLoaiVe { get; set; } = string.Empty;

        public byte TrangThaiTruoc { get; set; }
        public byte TrangThaiSau { get; set; }
        public DateTime ThoiGianCheckIn { get; set; }
        public int BanToChucID { get; set; }
    }
}
