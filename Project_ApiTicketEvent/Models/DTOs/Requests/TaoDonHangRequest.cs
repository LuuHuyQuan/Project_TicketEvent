using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class TaoDonHangRequest
    {
        public int NguoiMuaID { get; set; }
        public int SuKienID { get; set; }
        public List<TaoDonHangItemRequest> Items { get; set; } = new();
    }
    public class TaoDonHangItemRequest
    {
        public int LoaiVeID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; } // bạn có thể tính từ bảng LoaiVe nếu muốn
    }

}
