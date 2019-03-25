using System.Collections.Generic;
using System.Threading.Tasks;
using GuidedSearchBot.Models;

namespace GuidedSearchBot.Interfaces
{
    public interface ITableStore
    {
        Task<IEnumerable<string>> GetMandatoryCategories(string mappingFor);

        Task<MandatoryCategoryMapping> UpsertMandatoryCategoryMapping(MandatoryCategoryMapping item);
    }
}
