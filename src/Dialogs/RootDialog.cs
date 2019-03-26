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
    public class RootDialog : ComponentDialog
    {
        protected readonly IConfiguration _configuration;
        private string _utterance;
        private readonly ITableStore _tableStore;
        private LuisModel _luisModel;

        public RootDialog(IConfiguration configuration, ITableStore tableStore)
            : base(nameof(RootDialog))
        {
            _configuration = configuration;
            _tableStore = tableStore;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetLuisResultAsync,
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

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }

    }
}
