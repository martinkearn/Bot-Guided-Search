// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuidedSearchBot.Interfaces;
using GuidedSearchBot.Models;
using GuidedSearchBot.Services;
using GuidedSearchBot.State;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;

namespace GuidedSearchBot.Dialogs
{
    /// <summary>
    /// This is an example root dialog. Replace this with your applications.
    /// </summary>
    public class LuisRootDialog : ComponentDialog
    {
        private const string StateKeyCpuEntity = "cpu";
        private const string StateKeyColourEntity = "colour";
        private const string StateKeyConnectivityEntity = "connectivity";
        private const string StateKeyMemoryEntity = "memory";
        private const string StateKeyProductEntity = "product";
        private const string StateKeyProductFamilyEntity = "productfamily";
        private const string StateKeyStorageEntity = "storage";
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;
        public IStatePropertyAccessor<LuisRootDialogState> _luisRootDialogStateAccessor { get; }

        public LuisRootDialog(UserState userState, ITableStore tableStore, IBotServices botServices)
            : base(nameof(LuisRootDialog))
        {
            _luisRootDialogStateAccessor = userState.CreateProperty<LuisRootDialogState>(nameof(LuisRootDialogState));
            _tableStore = tableStore;

            // Add child dialogs
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetLuisResultAsync,
                GetEntitiesAsync,
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
                ShowLinkAsync,
                FinalStepAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new ShowLinkDialog(nameof(ShowLinkDialog), botServices));
        }

        private async Task<DialogTurnResult> GetLuisResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _luisModel = (LuisModel)stepContext.Options;

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> GetEntitiesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

            // Store the entities that Luis has provided
            state.Entities = new Dictionary<string, string>();
            if (_luisModel.Entities.CPU != null) state.Entities.Add(StateKeyCpuEntity, _luisModel.Entities.CPU[0]);
            if (_luisModel.Entities.Colour != null) state.Entities.Add(StateKeyColourEntity, _luisModel.Entities.Colour[0]);
            if (_luisModel.Entities.Connectivity != null) state.Entities.Add(StateKeyConnectivityEntity, _luisModel.Entities.Connectivity[0]);
            if (_luisModel.Entities.Memory != null) state.Entities.Add(StateKeyMemoryEntity, _luisModel.Entities.Memory[0].Gb[0]);
            if (_luisModel.Entities.Product != null) state.Entities.Add(StateKeyProductEntity, _luisModel.Entities.Product[0]);
            if (_luisModel.Entities.ProductFamily != null) state.Entities.Add(StateKeyProductFamilyEntity, _luisModel.Entities.ProductFamily[0]);
            if (_luisModel.Entities.Storage != null) state.Entities.Add(StateKeyStorageEntity, _luisModel.Entities.Storage[0].Gb[0]);

            // Save state
            await _luisRootDialogStateAccessor.SetAsync(stepContext.Context, state);

            // Next waterfall step
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EstablishMandatoryCategoriesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

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
            await _luisRootDialogStateAccessor.SetAsync(stepContext.Context, state);

            // Next waterfall step
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptCpuCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyCpuEntity, Constants.Constants.WhichCpu);
        private async Task<DialogTurnResult> HandleCpuCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyCpuEntity);
        private async Task<DialogTurnResult> PromptColourCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyColourEntity, Constants.Constants.WhichColour);
        private async Task<DialogTurnResult> HandleColourCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyColourEntity);
        private async Task<DialogTurnResult> PromptConnectivityCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyConnectivityEntity, Constants.Constants.WhichConnectivity);
        private async Task<DialogTurnResult> HandleConnectivityCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyConnectivityEntity);
        private async Task<DialogTurnResult> PromptMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyMemoryEntity, Constants.Constants.WhichMemory);
        private async Task<DialogTurnResult> HandleMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyMemoryEntity);
        private async Task<DialogTurnResult> PromptProductCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyProductEntity, Constants.Constants.WhichProduct);
        private async Task<DialogTurnResult> HandleProductCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyProductEntity);
        private async Task<DialogTurnResult> PromptProductFamilyCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyProductFamilyEntity, Constants.Constants.WhichProductFamily);
        private async Task<DialogTurnResult> HandleProductFamilyCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyProductFamilyEntity);
        private async Task<DialogTurnResult> PromptStorageCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await PromptForEntityIfRequired(stepContext, cancellationToken, StateKeyStorageEntity, Constants.Constants.WhichStorage);
        private async Task<DialogTurnResult> HandleStorageCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await HandlePromptResultIfRequired(stepContext, cancellationToken, StateKeyStorageEntity);

        private async Task<DialogTurnResult> ShowLinkAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

            // Start the dialog to show either a link mapping(s) or search link
            return await stepContext.BeginDialogAsync(nameof(ShowLinkDialog), state.Entities);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }

        #region Private Functions
        private bool PromptForCategory(LuisRootDialogState state, string catKey)
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

        private async Task<DialogTurnResult> PromptForEntityIfRequired(WaterfallStepContext stepContext, CancellationToken cancellationToken, string key, string promptText)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, key))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptText) }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }
        private async Task<DialogTurnResult> HandlePromptResultIfRequired(WaterfallStepContext stepContext, CancellationToken cancellationToken, string key)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, key))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(key, result);
                    await _luisRootDialogStateAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
        #endregion

    }
}
