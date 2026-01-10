using Models.DTOs.Reponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IVeRepository
    {
        List<VeResponse> GetMyTickets(int nguoiSoHuuId);
        VeResponse? GetMyTicketByMaVe(int nguoiSoHuuId, string maVe);
        Task<bool> HuyVeAsync(int nguoiSoHuuId, string maVe, string? lyDo);
        Task<HoanVeResponse> HoanVeAsync(int nguoiSoHuuId, string maVe, string? lyDo, string? phuongThuc, string? rawResponse);
    }
}
