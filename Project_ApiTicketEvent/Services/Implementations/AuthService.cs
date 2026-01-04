using Microsoft.Extensions.Options;
using Models;
using Models.DTOs.Reponses;
using Models.DTOs.Requests;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IVaiTroRepository _vaiTroRepo;
        private readonly JwtSettings _jwt;
        public AuthService(
            INguoiDungRepository nguoiDungRepo,
            IRefreshTokenRepository refreshRepo,
            IVaiTroRepository vaiTroRepo,
            IOptions<JwtSettings> jwtOptions)
        {
            _nguoiDungRepo = nguoiDungRepo;
            _refreshRepo = refreshRepo;
            _vaiTroRepo = vaiTroRepo;
            _jwt = jwtOptions.Value;
        }

        public LoginReponse Login(LoginRequest request)
        {
         
        }


        public LoginReponse Refresh(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public int Register(RegisterRequest request)
        {
            var existing = _nguoiDungRepo.GetByEmail(request.Email);
            if (existing != null)
                throw new InvalidOperationException("Email đã tồn tại.");

            var hashedPassword = PasswordHasher.Hash(request.MatKhau);

            var user = new NguoiDung
            {
                HoTen = request.HoTen,
                TenDangNhap = request.TenDangNhap,
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                MatKhauHash = hashedPassword,
                VaiTroId = request.VaiTroId,
                NgayTao = DateTime.Now,
                TrangThai = true
            };

            return _nguoiDungRepo.Create(user);
        }

        public void RevokeAllRefreshTokens(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public void RevokeRefreshToken(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
