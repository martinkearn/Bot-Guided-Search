using BasicBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBot.Interfaces
{
    public interface ITableStore
    {
        Task<MandatoryCategoryMapping> UpsertMandatoryCategoryMapping(MandatoryCategoryMapping item);
    }
}
