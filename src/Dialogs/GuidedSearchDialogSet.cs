using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs
{
    public class GuidedSearchDialogSet : DialogSet
    {
        public GuidedSearchDialogSet(BotServices services, IStatePropertyAccessor<DialogState> dialogStatePropertyAccessor)
            : base(dialogStatePropertyAccessor)
        {
            // Add the top-level dialog
            Add(new MainMenuDialog.MainMenuDialog(nameof(MainMenuDialog), services));
        }
    }

}
