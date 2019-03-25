using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuidedSearchBot.Dialogs.LuisDialog
{
    public class LuisDialogState
    {
        public Dictionary<string,string> Entities { get; set; }

        public List<string> MandatoryCategories { get; set; }
    }
}
