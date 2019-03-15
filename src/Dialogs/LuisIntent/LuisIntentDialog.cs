using System.Threading;
using System.Threading.Tasks;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs.LuisIntent
{
    internal class LuisIntentDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;

        public LuisIntentDialog(string dialogId, BotServices botServices, ITableStore tableStore)
             : base(dialogId)
        {
            _botServices = botServices;
            _tableStore = tableStore;
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
            _luisModel = (LuisModel)stepContext.Options;

            await stepContext.Context.SendActivityAsync($"You've landed in LuisIntentDialog with {_luisModel.Intents.Count} intents.");

            if (_luisModel.Entities.Product != null)
            {
                await stepContext.Context.SendActivityAsync($"Product: {_luisModel.Entities.Product[0]}");
                var manCatsForProduct = await _tableStore.GetMandatoryCategories(nameof(_luisModel.Entities.Product));
                await stepContext.Context.SendActivityAsync($"Mandatory categories for Product: {string.Join(",", manCatsForProduct)}");
            }

            if (_luisModel.Entities.Memory != null)
            {
                await stepContext.Context.SendActivityAsync($"Memory: {_luisModel.Entities.Memory[0].Gb[0]}");
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
