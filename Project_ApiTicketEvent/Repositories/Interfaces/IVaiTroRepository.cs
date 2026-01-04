using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IVaiTroRepository
    {
        List<VaiTro> GetAll();
        VaiTro? GetById(int id);
        VaiTro? GetByMa(string maVaiTro);
    }
}
