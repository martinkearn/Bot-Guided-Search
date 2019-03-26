// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuidedSearchBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

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
    public class MainBot : ActivityHandler
    {
        // Messages sent to the user.
        private const string WelcomeMessage = @"Welcome to the guided search bot.";

        private BotState _userState;

        private IStatePropertyAccessor<WelcomeUserState> _welcomeUserStateAccessor;

        // Initializes a new instance of the "WelcomeUserBot" class. 
        public MainBot(UserState userState)
        {
            _userState = userState;
            _welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        }

        // Greet when users are added to the conversation.
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeUserState = await _welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState());
                    if (welcomeUserState.DidBotWelcomeUser == false)
                    {
                        welcomeUserState.DidBotWelcomeUser = true;
                        await turnContext.SendActivityAsync($"Hi {member.Name}! {WelcomeMessage} - from MemberAdded", cancellationToken: cancellationToken);
                    }
                }
            }

            // Save any state changes.
            await _userState.SaveChangesAsync(turnContext);
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
                await turnContext.SendActivityAsync($"Hi {name}! {WelcomeMessage} - from MessageReceived", cancellationToken: cancellationToken);
            }
            else
            {
                // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.           
                var text = turnContext.Activity.Text.ToLowerInvariant();
                await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
            }

            // Save any state changes.
            await _userState.SaveChangesAsync(turnContext);
        }



    }
}
