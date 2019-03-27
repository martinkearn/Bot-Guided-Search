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

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetLuisResultAsync,
                EstablishMandatoryCategoriesAsync,
                FinalStepAsync,
            }));
            
            // Logical Steps
            //1 Extract model from step optipns
            //3 Whole entity extraction and completion bit which can probably be done using prompts or that non-waterfall sample in the new samples
            
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetLuisResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _luisModel = (LuisModel)stepContext.Options;

            await stepContext.Context.SendActivityAsync($"Luis Model Top Intent: {_luisModel.TopIntent()}");

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EstablishMandatoryCategoriesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

            // Store the entities that Luis has provided
            state.Entities = new Dictionary<string, string>();
            state.Entities.Add("test", "fibble");

            if (_luisModel.Entities.CPU != null)
            {
                var value = _luisModel.Entities.CPU[0];
                state.Entities.Add(StateKeyCpuEntity, value);
            }

            //if (_luisModel.Entities.Colour != null)
            //{
            //    var value = _luisModel.Entities.Colour[0];
            //    state.Entities.Add(StateKeyColourEntity, value);
            //}

            //if (_luisModel.Entities.Connectivity != null)
            //{
            //    var value = _luisModel.Entities.Connectivity[0];
            //    state.Entities.Add(StateKeyConnectivityEntity, value);
            //}

            //if (_luisModel.Entities.Memory != null)
            //{
            //    var value = _luisModel.Entities.Memory[0].Gb[0];
            //    state.Entities.Add(StateKeyMemoryEntity, value);
            //}

            //if (_luisModel.Entities.Product != null)
            //{
            //    var value = _luisModel.Entities.Product[0];
            //    state.Entities.Add(StateKeyProductEntity, value);
            //}

            //if (_luisModel.Entities.ProductFamily != null)
            //{
            //    var value = _luisModel.Entities.ProductFamily[0];
            //    state.Entities.Add(StateKeyProductFamilyEntity, value);
            //}

            //if (_luisModel.Entities.Storage != null)
            //{
            //    var value = _luisModel.Entities.Storage[0].Gb[0];
            //    state.Entities.Add(StateKeyStorageEntity, value);
            //}

            //// Get mandatory categories
            //state.MandatoryCategories = new List<string>();
            //foreach (var entity in state.Entities)
            //{
            //    var mandCats = await _tableStore.GetMandatoryCategories(entity.Value);
            //    foreach (var mandCat in mandCats)
            //    {
            //        state.MandatoryCategories.Add(mandCat);
            //    }
            //}

            //// Save state
            //await UserProfileAccessor.SetAsync(stepContext.Context, state);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _luisRootDialogStateAccessor.GetAsync(stepContext.Context, () => new LuisRootDialogState());

            await stepContext.Context.SendActivityAsync($"Test state: {state.Entities["test"]}");

            return await stepContext.EndDialogAsync();
        }

    }
}
