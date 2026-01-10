using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Reponses
{
    public class HoanVeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        public int? VeID { get; set; }
        public int? DonHangID { get; set; }

        // Số tiền hoàn (mình trả về dạng dương cho dễ hiểu)
        public decimal RefundAmount { get; set; }

        public int? ThanhToanID { get; set; }
        public string? MaGiaoDich { get; set; }
    }
}
