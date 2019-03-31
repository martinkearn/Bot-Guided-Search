# Guided Search Bot
This is a Microsoft Bot Framework v4.3 bot written in C#.

This bot is designed to demonstrate several scenarios around guiding users to specific links via either QNA, Link Mapping or Search. 

The scenario this bot is built around is the Microsoft store, but could be applied to any scenario that requires a bot that guides users to an online location.

You can see a published version of the bot (published from the Deployment branch) [here](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA).

The bot makes use of several online services:

* Microsoft Azure Bot Service
* Microsoft Azure QNA Maker (Cognitive Services)
* Microsoft Azure Language Understanding Intelligence Service - LUIS (Cognitive Services)
* Microsoft Azure Table Storage
* Microsoft Dispatch Bot CLI tool

In terms of bot code, this sample is a good example of the following (all based on v4.3 patterns):

* General v4.3 patterns and practices
* Using Dispatch
* Using QNAMaker
* Using Luis
* Getting data from Table Storage
* Bot services (without .BOT file)
* Dialogs, including
  * Root dialogs
  * Component dialogs
  * Waterfall steps
  * Passing data between dialogs
  * Dialog State
  * Prompts

## QNA Flow

This flow is designed to give the users natural language querying capabilities over pre-existing FAQs. 

For this flow, we've used the following websites as a source of content for a QNAMaker knowledge base:

* <https://surfacetip.com/surface-go-faq> 
* <https://surfacetip.com/surface-book-2-faq> 
* <https://surfacetip.com/surface-book-2-faq> 

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

* *"What is the weight of a Surface Go?"*
* *"Does Surface Go include a pen?"*
* *"Can I charge just the base of my Surface Book 2?"*

