using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Ve
    {
        public int VeID { get; set; }
        public int DonHangID { get; set; }
        public int LoaiVeID { get; set; }
        public int NguoiSoHuuID { get; set; }

        public string MaVe { get; set; } = string.Empty;
        public string QrToken { get; set; } = string.Empty;
        public byte TrangThai { get; set; } // TINYINT (vd: 0=ChuaSuDung, 1=DaCheckIn, 2=Huy...)

    }
}
