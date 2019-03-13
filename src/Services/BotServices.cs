// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;

namespace BasicBot.Services
{
    /// <summary>
    /// Represents references to external services.
    ///
    /// For example, LUIS services are kept here as a singleton.  This external service is configured
    /// using the <see cref="BotConfiguration"/> class.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    /// <seealso cref="https://www.luis.ai/home"/>
    public class BotServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        /// <param name="luisServices">A dictionary of named <see cref="LuisRecognizer"/> instances for usage within the bot.</param>
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Luis:
                        {
                            var luisService = (LuisService)service;
                            if (luisService == null)
                            {
                                throw new InvalidOperationException("The LUIS service is not configured correctly in your '.bot' file.");
                            }

                            var luisEndpoint = (luisService.Region?.StartsWith("https://") ?? false) ? luisService.Region : luisService.GetEndpoint();
                            var luisApp = new LuisApplication(luisService.AppId, luisService.AuthoringKey, luisEndpoint);
                            var luisRecognizer = new LuisRecognizer(luisApp);
                            LuisServices.Add(luisService.Name, luisRecognizer);
                            break;
                        }

                    case ServiceTypes.QnA:
                        {
                            var qnaService = (QnAMakerService)service;
                            if (qnaService == null)
                            {
                                throw new InvalidOperationException("The QNA Maker service is not configured correctly in your '.bot' file.");
                            }

                            var qnaMaker = new QnAMaker(qnaService);
                            QnAServices.Add(qnaService.Name, qnaMaker);
                            break;
                        }

                    case ServiceTypes.BlobStorage:
                        {
                            var blobStorage = service as BlobStorageService;
                            this.StorageConnectionString = blobStorage.ConnectionString;
                            break;
                        }
                }
            }
        }

        public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();

        public Dictionary<string, QnAMaker> QnAServices { get; } = new Dictionary<string, QnAMaker>();

        public string StorageConnectionString { get; set; }
    }
}
