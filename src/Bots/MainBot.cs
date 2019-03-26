// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuidedSearchBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GuidedSearchBot.Bots
{
    // Represents a bot that processes incoming activities.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.
    // For example, the "MemoryStorage" object and associated
    // IStatePropertyAccessor{T} object are created with a singleton lifetime.
    public class MainBot<T> : ActivityHandler where T: Dialog
    {
        // Messages sent to the user.
        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;
        private BotState _userState;
        private IStatePropertyAccessor<WelcomeUserState> _welcomeUserStateAccessor;

        // Initializes a new instance of the "WelcomeUserBot" class. 
        public MainBot(ConversationState conversationState, UserState userState, T dialog)
        {
            _dialog = dialog;
            _conversationState = conversationState;
            _userState = userState;
            _welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // Greet when users are added to the conversation.
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserState = await _welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState());

            foreach (var member in membersAdded)
            {
                // The bot itself is a conversation member too ... this check makes sure this is not the bot joining
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Look for web chat channel because it sends this event when a user messages so we want to only do this if not webchat. Webchat welcome is handled on receipt of first message
                    if (turnContext.Activity.ChannelId.ToLower() != "webchat")
                    {
                        welcomeUserState.DidBotWelcomeUser = true;
                        await turnContext.SendActivityAsync($"Hi {member.Name}! {Constants.Constants.Welcome} - from OnMembersAddedAsync", cancellationToken: cancellationToken);
                        // Save any state changes.
                        await _userState.SaveChangesAsync(turnContext);
                    }
                }
            }
        }

        // Process incoming message
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserState = await _welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState());

            if (welcomeUserState.DidBotWelcomeUser == false)
            {
                welcomeUserState.DidBotWelcomeUser = true;

                // the channel should sends the user name in the 'From' object
                var name = turnContext.Activity.From.Name ?? string.Empty;
                await turnContext.SendActivityAsync($"Hi {name}! {Constants.Constants.Welcome} - from OnMessageActivityAsync", cancellationToken: cancellationToken);
                // Save any state changes.
                await _userState.SaveChangesAsync(turnContext);
            }
            else
            {
                await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
        }



    }
}
