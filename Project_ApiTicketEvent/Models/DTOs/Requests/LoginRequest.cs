using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Requests
{
    public class LoginRequest
   {
       public string TenDangNhap { get; set; } = null!;
       public string Email { get; set; } = null!;
       public string MatKhau { get; set; } = null!;
   }
}
