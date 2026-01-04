using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
namespace Services.Interfaces
{
    public interface IDanhMucSuKienService
    {
        Task<List<DanhMucSuKien>> GetAllAsync();
        Task<DanhMucSuKien?> GetByNameAsync(string tenDanhMuc);

    }
}
