using System;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace BasicBot.Dialogs.LuisIntent
{
    internal class LuisIntentDialog : ComponentDialog
    {
        private readonly BotServices _botServices;

        public LuisIntentDialog(string dialogId, BotServices botServices)
             : base(dialogId)
        {
            _botServices = botServices;

            // ID of the child dialog that should be started anytime the component is started.
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
            //var luisResults = await _botServices.LuisServices["BasicBotLuisApplication"].RecognizeAsync(stepContext.Context, cancellationToken);
            //var topScoringIntent = luisResults?.GetTopScoringIntent();
            //var topIntent = topScoringIntent.Value.intent;

            var topIntent = "made up";
            await stepContext.Context.SendActivityAsync($"Top scoring intent was {topIntent}");

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
