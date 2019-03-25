using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace GuidedSearchBot.Dialogs.EntityCompletionDialog
{
    internal class EntityCompletionDialog : ComponentDialog
    {
        private const string TextPromptName = "textprompt";
        private string _prompt;
        private string _result;

        public EntityCompletionDialog(string dialogId)
             : base(dialogId)
        {
            InitialDialogId = dialogId;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                GetEntityName,
                PromptForInput,
                HandleInput,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
            AddDialog(new TextPrompt(TextPromptName));
        }

        private async Task<DialogTurnResult> GetEntityName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _prompt = (string)stepContext.Options;
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = _prompt,
                },
            };
            return await stepContext.PromptAsync(TextPromptName, opts);
        }

        private async Task<DialogTurnResult> HandleInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _result = stepContext.Result as string;
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(_result).ConfigureAwait(false);
        }
    }
}
