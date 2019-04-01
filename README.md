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

Link mappings are used to map a set of specific values to certain categories to a link. 

The link could be any URL addressable resource, such as website, search query or SharePoint Url.

For example, if the user defines these categories:

* `Product` = "Surface Pro 6"
* `Memory` = "16Gb"
* `Storage` = "512Gb"
* Then the link mapping system would provide the following link: https://www.microsoft.com/en-gb/store/d/surface-pro-6/8zcnc665slq5/06rv

Link mappings are maintained using a QNAMaker knowledge base where the metadata tags are used to define category values for a specific link. The link is the answer as stored in the knowledge base.

## User Flows

This bot supports 4 main user flows.

### QNA Flow

This flow is designed to give the users natural language querying capabilities over pre-existing FAQs. 

For this flow, we've used the following websites as a source of content for a QNAMaker knowledge base:

- <https://surfacetip.com/surface-go-faq> 
- <https://surfacetip.com/surface-book-2-faq> 

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

- *"What is the weight of a Surface Go?"*
- *"Does Surface Go include a pen?"*
- *"Can I charge just the base of my Surface Book 2?"*

### Search flow

This flow is for when the user asks something that is not answerable by QNAMaker but the user has provided all mandatory categories in the initial question.

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

* *"I want a Surface Pro 6 with 8gb Memory"*

### Search flow with mandatory categories

This flow is where the user asks something that is not answerable by the QNAMaker but has also provided information about a category that has other mandatory categories where the details are not provided in the initial utterance.

The bot will prompt the user to provide details for the mandatory categories.

An example of this using the Microsoft store is that if the bot asks for a "*Surface with 16Gb memory*", the bot identifies the following from the utterance:

- `ProductFamily` = "Surface"
- `Memory` = "16Gb"

The mandatory categories for `ProductFamily` = "Surface" also require that the user provides values for `Storage` and `Product` categories before executing the search.

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

- *"I want a Surface with 16Gb memory"*
- The bot will ask "*Which specific product would you like?*", answer "*Surface Pro 6*"
- The bot will ask "*How much storage would you like?*", answer "*512gb*"

### Search flow with no links

This flow is where the user asks something that is not answerable by the QNAMaker but there are no link mappings for the categories that the user has either defined in the initial utterance or through mandatory category drill-down.

In this case, the bot prompts the user to try a search using the categories that have been identified.

To try it out ask these questions on the [published version of the bot](https://webchat.botframework.com/embed/GuidedSearchBot?s=LIgAt-fF7DE.RMxLyIXpOx52dMFJJB0MjJGrUXM4y68od_Qh7vIxtpA):

* "*I want a copy of Windows 10*"

## To run locally

In order to run locally, follow these high level steps:

1. Create an Azure Bot Service using the `EchoBot` template with C#
2. Create a Luis model by importing `GuidedSearchBot-a4a3.json`
3. Create a QNAMaker model for FAQ's by importing `MicrosoftStoreFAQ-KB.tsv`
4. Create a QNAMaker model by for link mapping importing `LinkMappings-KB.tsv.tsv`
5. Create a Dispatch model which includes the main Luis model and the FAQ QNAMaker model. Refer to [Dispatch Command Line tool](https://github.com/Microsoft/botbuilder-tools/tree/master/packages/Dispatch).
6. Create a Table container called `MandatoryCategories` beneath the Azure Storage Account that was created as part of step 1
7. Add all the relevant values from steps 1-6 to `AppSettings.json` or `Secrets.json` if you are using open source and want to protect your secrets.
8. Run and debug the bot as usual