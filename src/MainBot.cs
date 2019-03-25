// <copyright file="MainBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GuidedSearchBot
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Main bot.
    /// </summary>
    public class MainBot : IBot
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainBot"/> class.
        /// </summary>
        /// <param name="loggerFactory">loggerFactory.</param>
        public MainBot(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger<MainBot>();
            this.logger.LogTrace("Turn start.");
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Echo back to the user whatever they typed.
                await turnContext.SendActivityAsync($"You sent '{turnContext.Activity.Text}'\n");
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
