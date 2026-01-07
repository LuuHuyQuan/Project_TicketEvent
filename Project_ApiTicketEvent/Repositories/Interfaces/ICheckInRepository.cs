using Models.DTOs.Reponses;
using Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICheckInRepository
    {
        CheckInResponse? Scan(CheckInRequest req, int banToChucId, int? suKienId = null);
    }
}
