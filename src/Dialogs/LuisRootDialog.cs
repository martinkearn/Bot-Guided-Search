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

        public LuisRootDialog(UserState userState, ITableStore tableStore)
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
                PromptMemoryCategoryAsync,
                HandleMemoryCategoryAsync,
                FinalStepAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
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

        private async Task<DialogTurnResult> PromptMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyMemoryEntity))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Constants.Constants.WhichMemory) }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> HandleMemoryCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context);

            if (PromptForCategory(state, StateKeyMemoryEntity))
            {
                // We dont already have this category. This result should be the value for it
                if (stepContext.Result != null)
                {
                    var result = (string)stepContext.Result;
                    state.Entities.Add(StateKeyMemoryEntity, result);
                    await _luisRootDialogStateAccessor.SetAsync(stepContext.Context, state);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

            foreach (var entity in state.Entities)
            {
                await stepContext.Context.SendActivityAsync($"Entity: {entity.Key}-{entity.Value}");
            }

            foreach (var mandCat in state.MandatoryCategories)
            {
                await stepContext.Context.SendActivityAsync($"Mand Cat: {mandCat}");
            }

            return await stepContext.EndDialogAsync();
        }

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

    }
}
