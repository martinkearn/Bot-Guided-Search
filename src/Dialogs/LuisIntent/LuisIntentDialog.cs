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

            var filters = new Dictionary<string, string>();

            if (_luisModel.Entities.CPU != null)
            {
                var value = _luisModel.Entities.CPU[0];
                filters.Add(nameof(_luisModel.Entities.CPU), value);
            }

            if (_luisModel.Entities.Colour != null)
            {
                var value = _luisModel.Entities.Colour[0];
                filters.Add(nameof(_luisModel.Entities.Colour), value);
            }

            if (_luisModel.Entities.Connectivity != null)
            {
                var value = _luisModel.Entities.Connectivity[0];
                filters.Add(nameof(_luisModel.Entities.Connectivity), value);
            }

            if (_luisModel.Entities.Memory != null)
            {
                var value = _luisModel.Entities.Memory[0].Gb[0];
                filters.Add(nameof(_luisModel.Entities.Memory), value);
            }

            if (_luisModel.Entities.Product != null)
            {
                var value = _luisModel.Entities.Product[0];
                filters.Add(nameof(_luisModel.Entities.Product), value);
            }

            if (_luisModel.Entities.ProductFamily != null)
            {
                var value = _luisModel.Entities.ProductFamily[0];
                filters.Add(nameof(_luisModel.Entities.ProductFamily), value);
            }

            if (_luisModel.Entities.Storage != null)
            {
                var value = _luisModel.Entities.Storage[0].Gb[0];
                filters.Add(nameof(_luisModel.Entities.Storage), value);
            }

            var mandatoryCategories = new List<string>();
            foreach (var filter in filters)
            {
                await stepContext.Context.SendActivityAsync($"Filter: {filter.Key}:{filter.Value}");

                var mandCats = await _tableStore.GetMandatoryCategories(filter.Value);
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
