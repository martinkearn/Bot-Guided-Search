using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;

namespace BasicBot.Dialogs.LuisDialog
{
    internal class LuisDialog : ComponentDialog
    {
        private const string StateKeyCpuEntity = "cpu";
        private const string StateKeyColourEntity = "colour";
        private const string StateKeyConnectivityEntity = "connectivity";
        private const string StateKeyMemoryEntity = "memory";
        private const string StateKeyProductEntity = "product";
        private const string StateKeyProductFamilyEntity = "productfamily";
        private const string StateKeyStorageEntity = "storage";

        private readonly BotServices _botServices;
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;

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
                PromptCpuCategoryAsync,
                HandleCpuCategoryAsync,
                PromptColourCategoryAsync,
                HandleColourCategoryAsync,
                PromptConnectivityCategoryAsync,
                HandleConnectivityCategoryAsync,
                PromptMemoryCategoryAsync,
                HandleMemoryCategoryAsync,
                PromptProductCategoryAsync,
                HandleProductCategoryAsync,
                PromptProductFamilyCategoryAsync,
                HandleProductFamilyCategoryAsync,
                PromptStorageCategoryAsync,
                HandleStorageCategoryAsync,
                GetLinkMappingsAsync,
                HandleLinkMappingsResultAsync,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));

            // Child dialogs
            AddDialog(new EntityCompletionDialog.EntityCompletionDialog(nameof(EntityCompletionDialog.EntityCompletionDialog)));
            AddDialog(new LinkMappingDialog.LinkMappingDialog(nameof(LinkMappingDialog.LinkMappingDialog), botServices));
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
                state.Entities.Add(StateKeyCpuEntity, value);
            }

            if (_luisModel.Entities.Colour != null)
            {
                var value = _luisModel.Entities.Colour[0];
                state.Entities.Add(StateKeyColourEntity, value);
            }

            if (_luisModel.Entities.Connectivity != null)
            {
                var value = _luisModel.Entities.Connectivity[0];
                state.Entities.Add(StateKeyConnectivityEntity, value);
            }

            if (_luisModel.Entities.Memory != null)
            {
                var value = _luisModel.Entities.Memory[0].Gb[0];
                state.Entities.Add(StateKeyMemoryEntity, value);
            }

            if (_luisModel.Entities.Product != null)
            {
                var value = _luisModel.Entities.Product[0];
                state.Entities.Add(StateKeyProductEntity, value);
            }

            if (_luisModel.Entities.ProductFamily != null)
            {
                var value = _luisModel.Entities.ProductFamily[0];
                state.Entities.Add(StateKeyProductFamilyEntity, value);
            }

            if (_luisModel.Entities.Storage != null)
            {
                var value = _luisModel.Entities.Storage[0].Gb[0];
                state.Entities.Add(StateKeyStorageEntity, value);
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

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptCpuCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyCpuEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichCpu);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleCpuCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyCpuEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyCpuEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptColourCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyColourEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichColour);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleColourCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyColourEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyColourEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptConnectivityCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyConnectivityEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichConnectivity);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleConnectivityCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyConnectivityEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyConnectivityEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyMemoryEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichMemory);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyMemoryEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyMemoryEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptProductCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyProductEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichProduct);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleProductCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyProductEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyProductEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptProductFamilyCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyProductFamilyEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichProductFamily);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleProductFamilyCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyProductFamilyEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyProductFamilyEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptStorageCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyStorageEntity))
            {
                return await stepContext.BeginDialogAsync(nameof(EntityCompletionDialog.EntityCompletionDialog), Constants.Constants.WhichStorage);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleStorageCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyStorageEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyStorageEntity, result);
                    await UserProfileAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> GetLinkMappingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            return await stepContext.BeginDialogAsync(nameof(LinkMappingDialog.LinkMappingDialog), state.Entities);
        }

        private async Task<DialogTurnResult> HandleLinkMappingsResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Was a link offered?
            if (stepContext.Result != null)
            {
                var result = (bool)stepContext.Result;
                if (result)
                {
                    // Link was offered, contnue
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    // Link was not offered, do a search
                    var entityString = CreatEntityString(state.Entities);
                    var entityStringEncoded = HttpUtility.UrlEncode(entityString);
                    await stepContext.Context.SendActivityAsync($"{Constants.Constants.TryASearch} https://www.microsoft.com/en-gb/search?q={entityStringEncoded}", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }

        private bool PromptForCategory(LuisDialogState state, string catKey)
        {
            // Check if categry is a mandatory category
            if (state.MandatoryCategories.Contains(catKey))
            {
                // Check if we already have category
                if (state.Entities.ContainsKey(catKey))
                {
                    // Already have category, dont need to prompt for it
                    return false;
                }
                else
                {
                    // Do not have category, need to prompt for it
                    return true;
                }
            }
            else
            {
                // Dont require category, dont need to prompt for it
                return false;
            }
        }

        // TO DO make this a static helper method
        private string CreatEntityString(Dictionary<string, string> entities)
        {
            var textInfo = new CultureInfo("en-GB", false).TextInfo;
            var entityString = string.Empty;
            foreach (var entity in entities)
            {
                if (entity.Key == StateKeyMemoryEntity)
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else if (entity.Key == StateKeyStorageEntity)
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value}, ");
                }
            }

            entityString = entityString.TrimEnd(' ');
            entityString = entityString.TrimEnd(',');
            return entityString;
        }
    }
}
