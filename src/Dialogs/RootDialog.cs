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

        public RootDialog(IConfiguration configuration)
            : base(nameof(RootDialog))
        {
            _configuration = configuration;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What can I help you with today?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as string;
            await stepContext.Context.SendActivityAsync($"you said {result}");

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
        //private const string TextPromptName = "textprompt";
        //private readonly ITableStore _tableStore;
        //private IBotServices _botServices;

        //public RootDialog(IBotServices botServices, ITableStore tableStore)
        //    : base(nameof(RootDialog))
        //{
        //    // ID of the child dialog that should be started anytime the component is started.
        //    InitialDialogId = nameof(RootDialog);
        //    _botServices = botServices;
        //    _tableStore = tableStore;

        //    // Define the steps of the waterfall dialog and add it to the set.
        //    var waterfallSteps = new WaterfallStep[]
        //    {
        //        PromptForInputAsync,
        //        HandleInputResultAsync,
        //        PromptForWhatNextAsync,
        //        HandleWhatNextAsync,
        //    };

        //    AddDialog(new WaterfallDialog("waterfall", waterfallSteps));
        //    AddDialog(new TextPrompt(TextPromptName));

        //    // Child dialogs
        //}

        //private async Task<DialogTurnResult> PromptForInputAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var opts = new PromptOptions
        //    {
        //        Prompt = new Activity
        //        {
        //            Type = ActivityTypes.Message,
        //            Text = Constants.Constants.WhatAreYouLookingFor,
        //        },
        //    };
        //    return await stepContext.PromptAsync(TextPromptName, opts);
        //}

        //private async Task<DialogTurnResult> HandleInputResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var result = stepContext.Result as string;

        //    // Do Dispatcher triage here and then spawn out to appropriate child dialogs
        //    var dispatchRecognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
        //    var dispatchTopScoringIntent = dispatchRecognizerResult?.GetTopScoringIntent();
        //    var dispatchTopIntent = dispatchTopScoringIntent.Value.intent;

        //    switch (dispatchTopIntent)
        //    {
        //        case "l_GuidedSearchBot-a4a3":
        //            await ProcessMainLuisAsync(stepContext, dispatchRecognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
        //            break;

        //        case "q_MicrosoftStoreFAQ":
        //            await ProcessMainQnAAsync(stepContext, cancellationToken);
        //            break;

        //        case "None":
        //            await stepContext.Context.SendActivityAsync($"{Constants.Constants.NoIntent}{Constants.Constants.DontUnderstand}");
        //            break;

        //        default:
        //            await stepContext.Context.SendActivityAsync(Constants.Constants.DontUnderstand);
        //            break;
        //    }

        //    return await stepContext.NextAsync(cancellationToken: cancellationToken);
        //}

        //private async Task<DialogTurnResult> PromptForWhatNextAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var opts = new PromptOptions
        //    {
        //        Prompt = new Activity
        //        {
        //            Type = ActivityTypes.Message,
        //            Text = Constants.Constants.WhatNext,
        //        },
        //    };
        //    return await stepContext.PromptAsync(TextPromptName, opts);
        //}

        //private async Task<DialogTurnResult> HandleWhatNextAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var result = stepContext.Result as string;

        //    switch (result)
        //    {
        //        case "start again":
        //            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);

        //        default:
        //            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);
        //    }
        //}

        //private async Task ProcessMainLuisAsync(WaterfallStepContext stepContext, LuisResult luisResult, CancellationToken cancellationToken)
        //{
        //    await stepContext.Context.SendActivityAsync("Landed in ProcessMainLuisAsync");
        //}

        //private async Task ProcessMainQnAAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var results = await _botServices.MainQnA.GetAnswersAsync(stepContext.Context);
        //    if (results.Any())
        //    {
        //        await stepContext.Context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
        //        await stepContext.Context.SendActivityAsync(Constants.Constants.QNADone);
        //    }
        //    else
        //    {
        //        await stepContext.Context.SendActivityAsync(Constants.Constants.NoAnswerInQNAKB);
        //    }
        //}
    }
}
