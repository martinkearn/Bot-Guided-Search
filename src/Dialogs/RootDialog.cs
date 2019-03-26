using GuidedSearchBot.Interfaces;
using GuidedSearchBot.Models;
using GuidedSearchBot.Services;
using GuidedSearchBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuidedSearchBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private const string TextPromptName = "textprompt";
        private readonly ITableStore _tableStore;
        private IBotServices _botServices;

        public RootDialog(IBotServices botServices, ITableStore tableStore, IConfiguration configuration)
            : base(nameof(LuisRootDialog))
        {

            InitialDialogId = nameof(RootDialog);
            _botServices = botServices;
            _tableStore = tableStore;

            // Define the steps of the waterfall dialog and add it to the set.
            var waterfallSteps = new WaterfallStep[]
            {
                HandleUtteranceAsync,
                PromptForNextQuestionAsync,
                HandleWhatNextAsync,
            };

            AddDialog(new WaterfallDialog(InitialDialogId, waterfallSteps));
            AddDialog(new TextPrompt(TextPromptName));

            // Child dialogs
            AddDialog(new LuisRootDialog(_tableStore));
        }

        private async Task<DialogTurnResult> HandleUtteranceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var utterance = (string)stepContext.Options;

            // Do Dispatcher triage here and then spawn out to appropriate child dialogs
            var dispatchResults = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var dispatchTopScoringIntent = dispatchResults?.GetTopScoringIntent();
            var dispatchTopIntent = dispatchTopScoringIntent.Value.intent;

            switch (dispatchTopIntent)
            {
                case "l_GuidedSearchBot-a4a3":
                    var luisResultModel = await _botServices.MainLuis.RecognizeAsync<LuisModel>(stepContext.Context, CancellationToken.None);
                    return await stepContext.BeginDialogAsync(nameof(LuisRootDialog), luisResultModel);

                case "q_MicrosoftStoreFAQ":
                    var results = await _botServices.MainQnA.GetAnswersAsync(stepContext.Context);
                    if (results.Any())
                    {
                        await stepContext.Context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(Constants.Constants.NoAnswerInQNAKB);
                    }
                    break;

                case "None":
                    await stepContext.Context.SendActivityAsync($"{Constants.Constants.NoIntent}{Constants.Constants.DontUnderstand}");
                    break;

                default:
                    await stepContext.Context.SendActivityAsync(Constants.Constants.DontUnderstand);
                    break;
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForNextQuestionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = Constants.Constants.WhatNext,
                },
            };
            return await stepContext.PromptAsync(TextPromptName, opts);
        }

        private async Task<DialogTurnResult> HandleWhatNextAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);
        }

    }
}