using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Dialogs.LuisIntent;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace BasicBot.Dialogs.MainMenuDialog
{
    public class MainMenuDialog : ComponentDialog
    {
        private const string _inputPrompt = "inputPrompt";
        private const string LuisIntentDialog = "LuisIntentDialog";
        private BotServices _botServices;
        private const string HelpIntent = "Help";
        private const string NoneIntent = "None";
        private const string SearchIntent = "Search";
        private const string DispatchLuisIntent = "l_GuidedSearchBot-a4a3";
        private const string DispatchQNAIntent = "q_MicrosoftStoreFAQ";
        public const string DontUnderstand = "Sorry, I dont understand, please rephrase or ask for Help";

        public MainMenuDialog(string dialogId, BotServices botServices)
            : base(dialogId)
        {
            // ID of the child dialog that should be started anytime the component is started.
            InitialDialogId = dialogId;
            _botServices = botServices;

            // Define the steps of the waterfall dialog and add it to the set.
            var waterfallSteps = new WaterfallStep[]
            {
                PromptForInputAsync,
                HandleInputResultAsync,
                ResetDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
            AddDialog(new TextPrompt(_inputPrompt));

            // Child dialogs
            AddDialog(new LuisIntentDialog(LuisIntentDialog, botServices));

        }

        private async Task<DialogTurnResult> PromptForInputAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "What are you looking for?",
                },
            };
            return await stepContext.PromptAsync(_inputPrompt, opts);
        }

        private async Task<DialogTurnResult> HandleInputResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as string;

            await stepContext.Context.SendActivityAsync($"You said '{result}'");

            // Do Dispatcher triage here and then spawn out to appropriate child dialogs
            var dispatcherResults = await _botServices.LuisServices["GuidedSearchBotDispatch"].RecognizeAsync(stepContext.Context, cancellationToken);
            var topScoringIntent = dispatcherResults?.GetTopScoringIntent();
            var topIntent = topScoringIntent.Value.intent;
            switch (topIntent)
            {
                case DispatchLuisIntent:
                    await stepContext.Context.SendActivityAsync("This is a Luis intent. Starting LuisIntentDialog");
                    return await stepContext.BeginDialogAsync(LuisIntentDialog);

                case DispatchQNAIntent:
                    await stepContext.Context.SendActivityAsync("This is a QNA intent.");
                    break;

                case NoneIntent:
                    await stepContext.Context.SendActivityAsync($"You don't seem to have an intent .... {DontUnderstand}");
                    break;

                default:
                    await stepContext.Context.SendActivityAsync(DontUnderstand);
                    break;
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> ResetDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
