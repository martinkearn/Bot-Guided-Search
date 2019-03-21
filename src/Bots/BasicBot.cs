// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using GuidedSearchBot.Constants;
using GuidedSearchBot.Dialogs.LuisDialog;
using GuidedSearchBot.Dialogs.MainMenuDialog;
using GuidedSearchBot.Interfaces;
using GuidedSearchBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GuidedSearchBot.Bots
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class BasicBot : IBot
    {
        private readonly IStatePropertyAccessor<LuisDialogState> _luisDialogStateAccessor;
        private readonly ILogger _logger;
        private readonly ITableStore _tableStore;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        public BasicBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory, ITableStore tableStore)
        {
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = loggerFactory.CreateLogger<BasicBot>();
            _tableStore = tableStore;
            _luisDialogStateAccessor = _userState.CreateProperty<LuisDialogState>(nameof(LuisDialogState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));

            Dialogs = new DialogSet(_dialogStateAccessor);
            Dialogs.Add(new MainMenuDialog(_luisDialogStateAccessor, nameof(MainMenuDialog), services, _tableStore));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger.LogTrace("Turn start.");
        }

        private DialogSet Dialogs { get; set; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext, cancellationToken);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    if (dc.ActiveDialog != null)
                    {
                        try
                        {
                            await dc.ContinueDialogAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            await turnContext.SendActivityAsync($"{Constants.Constants.ErrorContinuingDialog} {e.Message}");
                            _logger.Log(LogLevel.Error, e.InnerException, null);
                        }
                    }
                    else
                    {
                        await dc.BeginDialogAsync(nameof(MainMenuDialog), null, cancellationToken);
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        // Iterate over all new members added to the conversation.
                        foreach (var member in turnContext.Activity.MembersAdded)
                        {
                            // Greet anyone that was not the target (recipient) of this message.
                            if (member.Id != turnContext.Activity.Recipient.Id)
                            {
                                await dc.Context.SendActivityAsync(Constants.Constants.Welcome);
                                await dc.BeginDialogAsync(nameof(MainMenuDialog), null, cancellationToken);
                            }
                        }
                    }

                    break;
                default:
                    break;
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }
    }
}
