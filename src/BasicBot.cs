// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Dialogs;
using BasicBot.Dialogs.LuisIntent;
using BasicBot.Interfaces;
using BasicBot.Models;
using BasicBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class BasicBot : IBot
    {
        // Supported LUIS Intents
        public const string GreetingIntent = "Greeting";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";
        public const string SearchIntent = "Search";
        public const string DispatchLuisIntent = "l_GuidedSearchBot-a4a3";
        public const string DispatchQNAIntent = "q_MicrosoftStoreFAQ";

        // Messages
        public const string Welcome = "Welcome, I can help you find Microsoft devices such as the Surface or Xbox.";
        public const string DontUnderstand = "Sorry, I dont understand, please rephrase or ask for Help";

        // Luis apps
        public static readonly string GuidedSearchBotDispatchLuisConfiguration = "GuidedSearchBotDispatch";

        private readonly IStatePropertyAccessor<GreetingState> _greetingStateAccessor;

        private readonly ILogger _logger;
        private readonly GuidedSearchDialogSet _dialogSet;
        private readonly BotState _botState;
        private readonly BotServices _services;

        public BasicBot(BotServices services, BotState botState, GuidedSearchDialogSet dialogSet, ILoggerFactory loggerFactory)
        {
            _botState = botState ?? throw new ArgumentNullException(nameof(botState));
            _dialogSet = dialogSet ?? throw new ArgumentNullException(nameof(dialogSet));
            _services = services ?? throw new ArgumentNullException(nameof(services));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<BasicBot>();
            _logger.LogTrace("Turn start.");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    if (dc.ActiveDialog != null)
                    {
                        await dc.ContinueDialogAsync(cancellationToken);
                    }
                    else
                    {
                        await dc.BeginDialogAsync(GuidedSearchDialogSet.StartDialogId, null, cancellationToken);
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    if (activity.MembersAdded != null)
                    {
                        // Iterate over all new members added to the conversation.
                        foreach (var member in activity.MembersAdded)
                        {
                            // Greet anyone that was not the target (recipient) of this message.
                            if (member.Id != activity.Recipient.Id)
                            {
                                await dc.Context.SendActivityAsync(Welcome);
                                await dc.BeginDialogAsync(GuidedSearchDialogSet.StartDialogId, null, cancellationToken);
                            }
                        }
                    }

                    break;
                default:
                    await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
                    break;
            }

            await _botState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        // Determine if an interruption has occurred before we dispatch to any active dialog.
        private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent)
        {
            // See if there are any conversation interrupts we need to handle.
            if (topIntent.Equals(CancelIntent))
            {
                if (dc.ActiveDialog != null)
                {
                    await dc.CancelAllDialogsAsync();
                    await dc.Context.SendActivityAsync("Ok. I've canceled our last activity.");
                }
                else
                {
                    await dc.Context.SendActivityAsync("I don't have anything to cancel.");
                }

                return true;        // Handled the interrupt.
            }

            if (topIntent.Equals(HelpIntent))
            {
                await dc.Context.SendActivityAsync("Let me try to provide some help.");
                await dc.Context.SendActivityAsync("I understand greetings, being asked for help, or being asked to cancel what I am doing.");
                if (dc.ActiveDialog != null)
                {
                    await dc.RepromptDialogAsync();
                }

                return true;        // Handled the interrupt.
            }

            return false;           // Did not handle the interrupt.
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        /// <summary>
        /// Helper function to update greeting state with entities returned by LUIS.
        /// </summary>
        /// <param name="luisResult">LUIS recognizer <see cref="RecognizerResult"/>.</param>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task UpdateGreetingState(RecognizerResult luisResult, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest GreetingState
                var greetingState = await _greetingStateAccessor.GetAsync(turnContext, () => new GreetingState());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] userNameEntities = { "userName", "userName_patternAny" };
                string[] userLocationEntities = { "userLocation", "userLocation_patternAny" };

                // Update any entities
                // Note: Consider a confirm dialog, instead of just updating.
                foreach (var name in userNameEntities)
                {
                    // Check if we found valid slot values in entities returned from LUIS.
                    if (entities[name] != null)
                    {
                        // Capitalize and set new user name.
                        var newName = (string)entities[name][0];
                        greetingState.Name = char.ToUpper(newName[0]) + newName.Substring(1);
                        break;
                    }
                }

                foreach (var city in userLocationEntities)
                {
                    if (entities[city] != null)
                    {
                        // Capitalize and set new city.
                        var newCity = (string)entities[city][0];
                        greetingState.City = char.ToUpper(newCity[0]) + newCity.Substring(1);
                        break;
                    }
                }

                // Set the new values into state.
                await _greetingStateAccessor.SetAsync(turnContext, greetingState);
            }
        }
    }
}
