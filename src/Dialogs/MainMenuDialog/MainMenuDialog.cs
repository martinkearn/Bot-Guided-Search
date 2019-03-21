using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Constants;
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
        private const string TextPromptName = "textprompt";
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
                PromptForWhatNextAsync,
                HandleWhatNextAsync,
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
                    Text = Constants.Constants.WhatAreYouLookingFor,
                },
            };
            return await stepContext.PromptAsync(TextPromptName, opts);
        }

        private async Task<DialogTurnResult> HandleInputResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as string;

            // Do Dispatcher triage here and then spawn out to appropriate child dialogs
            var dispatchResults = await _botServices.LuisServices["GuidedSearchBotDispatch"].RecognizeAsync(stepContext.Context, cancellationToken);
            var dispatchTopScoringIntent = dispatchResults?.GetTopScoringIntent();
            var dispatchTopIntent = dispatchTopScoringIntent.Value.intent;

            switch (dispatchTopIntent)
            {
                case "l_GuidedSearchBot-a4a3":
                    var luisResultModel = await _botServices.LuisServices["BasicBotLuisApplication"].RecognizeAsync<LuisModel>(stepContext.Context, CancellationToken.None);
                    return await stepContext.BeginDialogAsync(nameof(LuisDialog), luisResultModel);

                case "q_MicrosoftStoreFAQ":
                    await DispatchToQnAMakerAsync(stepContext.Context, "MicrosoftStoreFAQ");
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

        private async Task<DialogTurnResult> PromptForWhatNextAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
            var result = stepContext.Result as string;

            switch (result)
            {
                case "start again":
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);

                default:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(context.Activity.Text))
            {
                var results = await _botServices.QnAServices[appName].GetAnswersAsync(context);
                if (results.Any())
                {
                    await context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
                    await context.SendActivityAsync(Constants.Constants.QNADone);
                }
                else
                {
                    await context.SendActivityAsync(Constants.Constants.NoAnswerInQNAKB);
                }
            }
        }
    }
}
