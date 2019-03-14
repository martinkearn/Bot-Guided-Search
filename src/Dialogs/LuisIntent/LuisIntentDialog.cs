using System.Threading;
using System.Threading.Tasks;
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
            _luisResult = (RecognizerResult)stepContext.Options;

            await stepContext.Context.SendActivityAsync($"You've landed in LuisIntentDialog with {_luisResult.Intents.Count} intents and {_luisResult.Entities.Count} entities detected.");

            var entities = LuisHelpers.GetEntities(_luisResult);

            foreach (var entity in entities)
            {
                await stepContext.Context.SendActivityAsync($"{entity.Type}:{entity.Text}");
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
