using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuidedSearchBot.State
{
    public class LuisRootDialogState
    {
        public Dictionary<string, string> Entities { get; set; }

        public List<string> MandatoryCategories { get; set; }
    }
}