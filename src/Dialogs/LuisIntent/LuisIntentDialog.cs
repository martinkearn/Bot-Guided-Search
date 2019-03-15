using System.Collections.Generic;
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

            var entityValues = new List<string>();

            if (_luisModel.Entities.CPU != null)
            {
                var value = _luisModel.Entities.CPU[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.Colour != null)
            {
                var value = _luisModel.Entities.Colour[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.Connectivity != null)
            {
                var value = _luisModel.Entities.Connectivity[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.Memory != null)
            {
                var value = _luisModel.Entities.Memory[0].Gb[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.Product != null)
            {
                var value = _luisModel.Entities.Product[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.ProductFamily != null)
            {
                var value = _luisModel.Entities.ProductFamily[0];
                entityValues.Add(value);
            }

            if (_luisModel.Entities.Storage != null)
            {
                var value = _luisModel.Entities.Storage[0].Gb[0];
                entityValues.Add(value);
            }

            await stepContext.Context.SendActivityAsync($"All entity values: {string.Join(",", entityValues)}");

            var mandatoryCategories = new List<string>();
            foreach (var entityValue in entityValues)
            {
                var mandCats = await _tableStore.GetMandatoryCategories(entityValue);
                foreach (var mandCat in mandCats)
                {
                    mandatoryCategories.Add(mandCat);
                }
            }

            await stepContext.Context.SendActivityAsync($"All mandatory categories: {string.Join(",", mandatoryCategories)}");

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
