using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class UpdateUserRequest
    {
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        public int? VaiTroId { get; set; }
        public bool? TrangThai { get; set; }
    }
}
