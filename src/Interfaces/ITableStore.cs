using System.Collections.Generic;
using System.Threading.Tasks;
using BasicBot.Models;

namespace BasicBot.Interfaces
{
    public interface ITableStore
    {
        Task<IEnumerable<string>> GetMandatoryCategories(string mappingFor);
    }
}
