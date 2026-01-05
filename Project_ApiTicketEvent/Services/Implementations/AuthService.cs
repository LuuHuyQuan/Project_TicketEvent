using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.DTOs.Reponses;
using Models.DTOs.Requests;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

            var user = _nguoiDungRepo.GetByEmail(request.Email);
            if (user == null)
                throw new InvalidOperationException("Sai email hoặc mật khẩu.");

            if (!Services.Security.PasswordHasher.Verify(request.MatKhau, user.MatKhauHash))
                throw new InvalidOperationException("Sai email hoặc mật khẩu.");

            if (user.TrangThai == false)
                throw new InvalidOperationException("Tài khoản đã bị khóa.");

            var roleCode = "USER";
            if (user.VaiTroId.HasValue)
            {
                var role = _vaiTroRepo.GetById(user.VaiTroId.Value);
                roleCode = role?.MaVaiTro ?? "USER";
            }

            var tokenResult = GenerateAccessToken(user, roleCode);

            var refreshTokenString = TokenGenerator.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.NguoiDungId,          
                Token = refreshTokenString,
                JwtId = tokenResult.JwtId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
                IsRevoked = false,
                IsUsed = false,
                RevokedAt = null
            };

            _refreshRepo.Create(refreshTokenEntity);

            return new LoginReponse
            {
                AccessToken = tokenResult.AccessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = tokenResult.ExpiresAt,

                HoTen = user.HoTen ?? "",
                TenDangNhap = user.TenDangNhap ?? "",
                Email = user.Email ?? "",
                SoDienThoai = user.SoDienThoai,
                VaiTro = roleCode
            };
        }

        private (string AccessToken, DateTime ExpiresAt, string JwtId)
    GenerateAccessToken(NguoiDung user, string role)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwt.Key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString("N");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.NguoiDungId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.TenDangNhap),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   jti),
                new Claim("fullName",                    user.HoTen),
                new Claim(ClaimTypes.Role,               role)
            };

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwt.ExpiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return (accessToken, expires, jti);
        }
        public LoginReponse Refresh(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new InvalidOperationException("Thiếu refresh token.");

            var storedToken = _refreshRepo.GetByToken(request.RefreshToken);
            if (storedToken == null)
                throw new InvalidOperationException("Refresh token không hợp lệ.");

            if (storedToken.IsRevoked)
                throw new InvalidOperationException("Refresh token đã bị thu hồi.");

            if (storedToken.IsUsed)
                throw new InvalidOperationException("Refresh token đã được sử dụng.");

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token đã hết hạn.");

            var user = _nguoiDungRepo.GetById(storedToken.UserId);
            if (user == null)
                throw new InvalidOperationException("Người dùng không tồn tại.");

            if (user.TrangThai == false)
                throw new InvalidOperationException("Tài khoản đã bị khóa.");

            var roleCode = "USER";
            if (user.VaiTroId.HasValue)
            {
                var role = _vaiTroRepo.GetById(user.VaiTroId.Value);
                roleCode = role?.MaVaiTro ?? "USER";
            }

            _refreshRepo.MarkUsed(storedToken.RefreshTokenId);

            var tokenResult = GenerateAccessToken(user, roleCode);

            var newRefreshTokenString = TokenGenerator.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.NguoiDungId,
                Token = newRefreshTokenString,
                JwtId = tokenResult.JwtId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
                IsRevoked = false,
                IsUsed = false,
                RevokedAt = null
            };

            _refreshRepo.Create(newRefreshTokenEntity);

            return new LoginReponse
            {
                AccessToken = tokenResult.AccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresAt = tokenResult.ExpiresAt,

                HoTen = user.HoTen ?? "",
                TenDangNhap = user.TenDangNhap ?? "",
                Email = user.Email ?? "",
                SoDienThoai = user.SoDienThoai,
                VaiTro = roleCode
            };
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
            var storedToken = _refreshRepo.GetByToken(request.RefreshToken);
            if (storedToken == null)
                throw new InvalidOperationException("Refresh token không tồn tại.");

            _refreshRepo.RevokeAllByUser(storedToken.UserId);
        }

        public void RevokeRefreshToken(RefreshTokenRequest request)
        {
            var storedToken = _refreshRepo.GetByToken(request.RefreshToken);
            if (storedToken == null)
                throw new InvalidOperationException("Refresh token không tồn tại.");

            if (storedToken.IsRevoked)
                return;
            _refreshRepo.Revoke(storedToken.RefreshTokenId);
        }
    }
}
