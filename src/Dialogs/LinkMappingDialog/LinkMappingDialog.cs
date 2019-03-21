using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Services;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace BasicBot.Dialogs.LinkMappingDialog
{
    public class LinkMappingDialog : ComponentDialog
    {
        private const string StateKeyMemoryEntity = "memory";
        private const string StateKeyStorageEntity = "storage";

        private readonly BotServices _botServices;
        private Dictionary<string, string> _entities;

        public LinkMappingDialog(string dialogId, BotServices botServices)
             : base(dialogId)
        {
            InitialDialogId = dialogId;

            _botServices = botServices;

            // Define the conversation flow using the waterfall model.
            var waterfallSteps = new WaterfallStep[]
            {
                GetLinkMapping,
                EndDialogAsync,
            };

            AddDialog(new WaterfallDialog(dialogId, waterfallSteps));
        }

        private async Task<DialogTurnResult> GetLinkMapping(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _entities = (Dictionary<string,string>)stepContext.Options;

            // Construct a string to summarise the search
            var textInfo = new CultureInfo("en-GB", false).TextInfo;
            var entityString = string.Empty;
            foreach (var entity in _entities)
            {
                if (entity.Key == StateKeyMemoryEntity)
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else if (entity.Key == StateKeyStorageEntity)
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value}, ");
                }
            }

            entityString = entityString.TrimEnd(' ');
            entityString = entityString.TrimEnd(',');
            await stepContext.Context.SendActivityAsync($"I'll see what i can find for {entityString}");

            // Get Link Mapping
            var options = new QnAMakerOptions();
            var metadata = new List<Metadata>();
            foreach (var entity in _entities)
            {
                metadata.Add(new Metadata() { Name = entity.Key, Value = entity.Value });
            }

            options.StrictFilters = metadata.ToArray();

            // If we query QNA for a strict filter that does not exist, it returns a 500 error whihc is why we've got this in a try catch
            try
            {
                var results = await _botServices.QnAServices["LinkMappings"].GetAnswersAsync(stepContext.Context, options);

                if (results.Length > 0)
                {
                    var reply = stepContext.Context.Activity.CreateReply($"I found a link which may be useful {results[0].Answer}");
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                        {
                            new CardAction() { Title = "Open Now", Type = ActionTypes.OpenUrl, Value = results[0].Answer },
                        },
                    };
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"No links matching that criteria, lets try a search", cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync($"Could not find a link, lets try a search", cancellationToken: cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
