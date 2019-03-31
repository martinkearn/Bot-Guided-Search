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

## **Search flow with all mandatory categories in initial utterance**

To Do

## **Search flow drilling into mandatory categories**

To Do

## Search flow with no links

To Do

## To run locally

In order to run locally, follow these high level steps:

1. Create an Azure Bot Service using the `EchoBot` template with C#
2. Create a Luis model by importing `GuidedSearchBot-a4a3.json`
3. Create a QNAMaker model by importing `MicrosoftStoreFAQ-KB.tsv`
4. Create a Dispatch model. Refer to the `Readme.md` in the `Dispatch` folder
5. Create a Table container called `MandatoryCategories` beneath the Azure Storage Account that was created as part of step 1
6. Add all the relevant values from steps 1-5 to `AppSettings.json` or `Secrets.json` if you are using open source and want to protect your secrets.
7. Run and debug the bot as usual