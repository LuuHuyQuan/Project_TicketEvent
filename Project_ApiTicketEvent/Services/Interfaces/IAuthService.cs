using Models.DTOs.Reponses;
using Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        int Register(RegisterRequest request);
        LoginReponse Login(LoginRequest request);
        LoginReponse Refresh(RefreshTokenRequest request);
        void RevokeRefreshToken(RefreshTokenRequest request);
        void RevokeAllRefreshTokens(RefreshTokenRequest request);
    }
}
