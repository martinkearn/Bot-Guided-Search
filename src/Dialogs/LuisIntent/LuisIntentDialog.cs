using System.Threading;
using System.Threading.Tasks;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs.LuisIntent
{
    internal class LuisIntentDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private RecognizerResult _luisResult;

        public LuisIntentDialog(string dialogId, BotServices botServices)
             : base(dialogId)
        {
            _botServices = botServices;
            InitialDialogId = dialogId;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                GetLuisResultAsync,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
        }

        private async Task<DialogTurnResult> GetLuisResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisModel = (LuisModel)stepContext.Options;

            await stepContext.Context.SendActivityAsync($"You've landed in LuisIntentDialog with {luisModel.Intents.Count} intents.");

            if (luisModel.Entities.Product != null)
            {
                await stepContext.Context.SendActivityAsync($"Product: {luisModel.Entities.Product[0]}");
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
