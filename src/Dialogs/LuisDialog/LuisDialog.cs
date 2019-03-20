using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs.LuisDialog
{
    internal class LuisDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;
        private List<string> _mandatoryCategories;
        private Dictionary<string, string> _entities;

        public LuisDialog(string dialogId, BotServices botServices, ITableStore tableStore)
             : base(dialogId)
        {
            _botServices = botServices;
            _tableStore = tableStore;
            InitialDialogId = dialogId;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                GetLuisResultAsync,
                EstablishMandatoryCategoriesAsync,
                PromptMemoryCategoryAsync,
                HandleMemoryCategoryAsync,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));

            // Child dialogs
            AddDialog(new EntityCompletionDialog.EntityCompletionDialog(nameof(EntityCompletionDialog.EntityCompletionDialog)));
        }

        private async Task<DialogTurnResult> GetLuisResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _luisModel = (LuisModel)stepContext.Options;

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EstablishMandatoryCategoriesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _entities = new Dictionary<string, string>();

            if (_luisModel.Entities.CPU != null)
            {
                var value = _luisModel.Entities.CPU[0];
                _entities.Add(nameof(_luisModel.Entities.CPU), value);
            }

            if (_luisModel.Entities.Colour != null)
            {
                var value = _luisModel.Entities.Colour[0];
                _entities.Add(nameof(_luisModel.Entities.Colour), value);
            }

            if (_luisModel.Entities.Connectivity != null)
            {
                var value = _luisModel.Entities.Connectivity[0];
                _entities.Add(nameof(_luisModel.Entities.Connectivity), value);
            }

            if (_luisModel.Entities.Memory != null)
            {
                var value = _luisModel.Entities.Memory[0].Gb[0];
                _entities.Add(nameof(_luisModel.Entities.Memory), value);
            }

            if (_luisModel.Entities.Product != null)
            {
                var value = _luisModel.Entities.Product[0];
                _entities.Add(nameof(_luisModel.Entities.Product), value);
            }

            if (_luisModel.Entities.ProductFamily != null)
            {
                var value = _luisModel.Entities.ProductFamily[0];
                _entities.Add(nameof(_luisModel.Entities.ProductFamily), value);
            }

            if (_luisModel.Entities.Storage != null)
            {
                var value = _luisModel.Entities.Storage[0].Gb[0];
                _entities.Add(nameof(_luisModel.Entities.Storage), value);
            }

            _mandatoryCategories = new List<string>();
            foreach (var entity in _entities)
            {
                var mandCats = await _tableStore.GetMandatoryCategories(entity.Value);
                foreach (var mandCat in mandCats)
                {
                    _mandatoryCategories.Add(mandCat);
                }
            }

            await stepContext.Context.SendActivityAsync($"We need some more information to help you find the right product, specifically {string.Join(", ", _mandatoryCategories)}");
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (_mandatoryCategories.Contains(nameof(_luisModel.Entities.Memory).ToLower()))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), $"How much memory would you like?");
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                var result = (string)stepContext.Result;
                //_entities.Add(nameof(_luisModel.Entities.Memory), result);
                // BUG _entities gets lost between urns - need to use state and acessors to store it
                return await stepContext.NextAsync(result, cancellationToken: cancellationToken);
            }
            else
            {
                // HACK This is jus a hack to check that we are receiving eth result before state is implemented
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // HACK This is jus a hack to check that we are receiving eth result before state is implemented
            if (stepContext.Result != null)
            {
                var result = (string)stepContext.Result;
                await stepContext.Context.SendActivityAsync($"You would like {result} memory");
            }

            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
