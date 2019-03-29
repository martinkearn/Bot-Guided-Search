using GuidedSearchBot.Interfaces;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GuidedSearchBot.Dialogs
{
    public class ShowLinkDialog : ComponentDialog
    {
        private readonly IBotServices _botServices;
        private Dictionary<string, string> _entities;

        public ShowLinkDialog(string dialogId, IBotServices botServices)
             : base(dialogId)
        {
            InitialDialogId = dialogId;

            _botServices = botServices;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                ShowLinkAsync,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
        }

        private async Task<DialogTurnResult> ShowLinkAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _entities = (Dictionary<string, string>)stepContext.Options;

            var entityString = Helpers.Helpers.GetEntityString(_entities);
            await stepContext.Context.SendActivityAsync($"{Constants.Constants.IllSeeWhatICanFind} {entityString}");

            // Get Link Mapping
            var options = new QnAMakerOptions();
            var metadata = new List<Metadata>();
            foreach (var entity in _entities)
            {
                metadata.Add(new Metadata() { Name = entity.Key, Value = entity.Value });
            }

            options.StrictFilters = metadata.ToArray();

            // If we query QNA for a strict filter that does not exist, it returns a 500 error which is why we've got this in a try catch
            var showSearch = false;
            try
            {
                var results = await _botServices.LinksQnA.GetAnswersAsync(stepContext.Context, options);

                if (results.Length > 0)
                {
                    // Show link mapping
                    var reply = stepContext.Context.Activity.CreateReply();
                    var buttons = new List<CardAction>();
                    var i = 0;
                    foreach (var result in results)
                    {
                        var title = string.Empty;
                        foreach (var metadataResult in result.Metadata)
                        {
                            title += $"{metadataResult.Value} ";
                        }
                        buttons.Add(new CardAction(ActionTypes.OpenUrl, title: $"{title}", value: results[i].Answer));
                        i += 1;
                    }
                    var card = new HeroCard
                    {
                        Text = Constants.Constants.IFoundALink,
                        Buttons = buttons,
                    };
                    reply.Attachments = new List<Attachment>() { card.ToAttachment() };

                    await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);
                }
                else
                {
                    showSearch = true;
                }
            }
            catch
            {
                showSearch = true;
            }
            
            // Link mapping not avaliable, show a search link
            if (showSearch)
            {
                var entityStringEncoded = HttpUtility.UrlEncode(entityString);
                var searchUrl = $"https://www.microsoft.com/en-gb/search?q={entityStringEncoded}";
                var reply = stepContext.Context.Activity.CreateReply();
                var buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, title: $"Search for {entityString}", value: searchUrl)
                };
                var card = new HeroCard
                {
                    Text = Constants.Constants.TryASearch,
                    Buttons = buttons,
                };
                reply.Attachments = new List<Attachment>() { card.ToAttachment() };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}
