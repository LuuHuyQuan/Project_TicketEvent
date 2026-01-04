using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        int Create(RefreshToken token);
        RefreshToken? GetByToken(string token);
        bool MarkUsed(int refreshTokenId);
        bool Revoke(int refreshTokenId);
        bool RevokeAllByUser(int userId);
    }
}
