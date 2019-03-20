using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs.LuisDialog
{
    internal class LuisDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;
        //private List<string> _mandatoryCategories;
        //private Dictionary<string, string> _entities;

        public LuisDialog(IStatePropertyAccessor<LuisDialogState> userProfileStateAccessor, string dialogId, BotServices botServices, ITableStore tableStore)
             : base(dialogId)
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            _botServices = botServices;
            _tableStore = tableStore;
            InitialDialogId = dialogId;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
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

        public IStatePropertyAccessor<LuisDialogState> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (state == null)
            {
                if (stepContext.Options is LuisDialogState luisDialogStateOpt)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, luisDialogStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new LuisDialogState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> GetLuisResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _luisModel = (LuisModel)stepContext.Options;

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EstablishMandatoryCategoriesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Store the entities that Luis has provided
            state.Entities = new Dictionary<string, string>();
            if (_luisModel.Entities.CPU != null)
            {
                var value = _luisModel.Entities.CPU[0];
                state.Entities.Add(nameof(_luisModel.Entities.CPU), value);
            }

            if (_luisModel.Entities.Colour != null)
            {
                var value = _luisModel.Entities.Colour[0];
                state.Entities.Add(nameof(_luisModel.Entities.Colour), value);
            }

            if (_luisModel.Entities.Connectivity != null)
            {
                var value = _luisModel.Entities.Connectivity[0];
                state.Entities.Add(nameof(_luisModel.Entities.Connectivity), value);
            }

            if (_luisModel.Entities.Memory != null)
            {
                var value = _luisModel.Entities.Memory[0].Gb[0];
                state.Entities.Add(nameof(_luisModel.Entities.Memory), value);
            }

            if (_luisModel.Entities.Product != null)
            {
                var value = _luisModel.Entities.Product[0];
                state.Entities.Add(nameof(_luisModel.Entities.Product), value);
            }

            if (_luisModel.Entities.ProductFamily != null)
            {
                var value = _luisModel.Entities.ProductFamily[0];
                state.Entities.Add(nameof(_luisModel.Entities.ProductFamily), value);
            }

            if (_luisModel.Entities.Storage != null)
            {
                var value = _luisModel.Entities.Storage[0].Gb[0];
                state.Entities.Add(nameof(_luisModel.Entities.Storage), value);
            }

            // Get mandatory categories
            state.MandatoryCategories = new List<string>();
            foreach (var entity in state.Entities)
            {
                var mandCats = await _tableStore.GetMandatoryCategories(entity.Value);
                foreach (var mandCat in mandCats)
                {
                    state.MandatoryCategories.Add(mandCat);
                }
            }

            // Save state
            await UserProfileAccessor.SetAsync(stepContext.Context, state);

            await stepContext.Context.SendActivityAsync($"We need some more information to help you find the right product, specifically {string.Join(", ", state.MandatoryCategories)}");
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Check if Memory is a mandaory category
            if (state.MandatoryCategories.Contains(nameof(_luisModel.Entities.Memory).ToLower()))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), $"How much memory would you like?");
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

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
