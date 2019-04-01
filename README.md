# Guided Search Bot
This is a Microsoft Bot Framework v4.3 bot written in C#.

This bot is designed to demonstrate several scenarios around guiding users to specific links via either QNA, Link Mapping or Search. 

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

## Concepts

This bot uses several invented concepts which help control the flow and data required by the bot. 

### Microsoft Store Scenario

The scenario this bot is built around is the [Microsoft store](https://www.microsoft.com/en-gb/store/b/home), but could be applied to any scenario that requires a bot that guides users to an online location either on the internet or internal locations such as SharePoint.

This bot will help users locate and learn about items on the Microsoft store by providing values to categories via natural language. 

### Categories

A category is analogous to a piece of metadata for a search result and are used to determine which link mappings (if any) are shown to the user.

In the Microsoft store scenario, there are several categories:

* `ProductFamily`: i.e. Surface, Xbox, Office, Windows
* `Product`: i.e.. Surface Pro 6, Surface Go, Windows 10 professional, Xbox One X
* `Memory`: i.e. 16gb
* `Storage`: i.e. 512gb
* `Connectivity`: i.e. Wifi, LTE
* `CPU`: i.e. I3, I5, I7
* `Colour`: i.e. Black, Silver

### Mandatory Categories

Mandatory categories exist so that if the provides a particular value for a particular category, the bot will require values for other categories that are mandatory for the one provided by the user.

For example, if the user asks for a Surface (which is a `ProductFamily`), the bot will also require the user to provide the `Memory`, `Storage`, and `Product` that is required before checking for link mappings.

Mandatory categories are maintained in an Azure Storage Table Container called `mandatorycategories`. They are inserted, updated and queried in code via the `TableStore` repository.

### Link Mappings

To Do

## User Flows

This bot supports 4 main user flows.

### QNA Flow

This flow is designed to give the users natural language querying capabilities over pre-existing FAQs. 

For this flow, we've used the following websites as a source of content for a QNAMaker knowledge base:

- <https://surfacetip.com/surface-go-faq> 
- <https://surfacetip.com/surface-book-2-faq> 
- <https://surfacetip.com/surface-book-2-faq> 

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

- *"What is the weight of a Surface Go?"*
- *"Does Surface Go include a pen?"*
- *"Can I charge just the base of my Surface Book 2?"*

### Search flow with all mandatory categories in initial utterance

This flow is for when the user asks something that is not answerable by QNAMaker but have provided all mandatory categories in the initial question.

### Search flow drilling into mandatory categories

To Do

### Search flow with no links

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