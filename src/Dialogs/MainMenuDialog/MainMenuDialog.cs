using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Dialogs.LuisDialog;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace BasicBot.Dialogs.MainMenuDialog
{
    public class MainMenuDialog : ComponentDialog
    {
        private const string TextPromptName = "inputPrompt";
        private const string NoneIntent = "None";
        private const string DispatchLuisIntent = "l_GuidedSearchBot-a4a3";
        private const string DispatchQNAIntent = "q_MicrosoftStoreFAQ";
        private const string QNAServiceName = "MicrosoftStoreFAQ";
        private const string LuisDispatchServiceName = "GuidedSearchBotDispatch";
        private const string LuisModelServiceName = "BasicBotLuisApplication";

        // Messages
        private const string WhatAreYouLookingFor = "Please tell me what you are looking for?";
        private const string DontUnderstand = "Sorry, I dont understand, please rephrase or ask for Help";
        private const string NoIntent = "You don't seem to have an intent ....";
        private const string NoAnswerInQNAKB = "Couldn't find an answer in the QNA Knowledge Base";
        private const string QNADone = "Thats all I have, I'll pass you back to the top menu now";

        private readonly ITableStore _tableStore;
        private BotServices _botServices;

        public MainMenuDialog(IStatePropertyAccessor<LuisDialogState> userProfileStateAccessor, string dialogId, BotServices botServices, ITableStore tableStore)
            : base(dialogId)
        {
            // ID of the child dialog that should be started anytime the component is started.
            InitialDialogId = dialogId;
            _botServices = botServices;
            _tableStore = tableStore;

            // Define the steps of the waterfall dialog and add it to the set.
            var waterfallSteps = new WaterfallStep[]
            {
                PromptForInputAsync,
                HandleInputResultAsync,
                ResetDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
            AddDialog(new TextPrompt(TextPromptName));

            // Child dialogs
            AddDialog(new LuisDialog.LuisDialog(userProfileStateAccessor, nameof(LuisDialog), botServices, _tableStore));
        }

        private async Task<DialogTurnResult> PromptForInputAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = WhatAreYouLookingFor,
                },
            };
            return await stepContext.PromptAsync(TextPromptName, opts);
        }

        private async Task<DialogTurnResult> HandleInputResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as string;

            // Do Dispatcher triage here and then spawn out to appropriate child dialogs
            var dispatchResults = await _botServices.LuisServices[LuisDispatchServiceName].RecognizeAsync(stepContext.Context, cancellationToken);
            var dispatchTopScoringIntent = dispatchResults?.GetTopScoringIntent();
            var dispatchTopIntent = dispatchTopScoringIntent.Value.intent;

            switch (dispatchTopIntent)
            {
                case DispatchLuisIntent:
                    var luisResultModel = await _botServices.LuisServices[LuisModelServiceName].RecognizeAsync<LuisModel>(stepContext.Context, CancellationToken.None);
                    return await stepContext.BeginDialogAsync(nameof(LuisDialog), luisResultModel);

                case DispatchQNAIntent:
                    await DispatchToQnAMakerAsync(stepContext.Context, QNAServiceName);
                    break;

                case NoneIntent:
                    await stepContext.Context.SendActivityAsync($"{NoIntent}{DontUnderstand}");
                    break;

                default:
                    await stepContext.Context.SendActivityAsync(DontUnderstand);
                    break;
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ResetDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(context.Activity.Text))
            {
                var results = await _botServices.QnAServices[appName].GetAnswersAsync(context);
                if (results.Any())
                {
                    await context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
                    await context.SendActivityAsync(QNADone);
                }
                else
                {
                    await context.SendActivityAsync(NoAnswerInQNAKB);
                }
            }
        }
    }
}
