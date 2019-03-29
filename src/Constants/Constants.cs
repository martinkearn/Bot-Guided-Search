using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuidedSearchBot.Constants
{
    public static class Constants
    {
        public static string BotFileNotFound = "The .bot configuration file was not found. botFilePath:";
        public static string BotFileNotLoaded = "The .bot configuration file could not be loaded. botFilePath:";
        public static string BotFileNoEndpoint = "The .bot file does not contain an endpoint with name";
        public static string SomethingWentWrong = "Sorry, it looks like something went wrong.";
        public static string Welcome = "Welcome, I can help you find Microsoft devices such as the Surface or Xbox. You can say things like *\"I want a Surface pro 6 with 8Gb memory\"* or *\"How heavy is a Surface Go?\"*";
        public static string ErrorContinuingDialog = "Error continuing dialog:";
        public static string WhatAreYouLookingFor = "Please tell me what you are looking for?";
        public static string DontUnderstand = "Sorry, I dont understand, please rephrase or ask for Help";
        public static string NoIntent = "You don't seem to have an intent ....";
        public static string NoAnswerInQNAKB = "Couldn't find an answer in the QNA Knowledge Base";
        public static string QNADone = "Thats all I have, I'll pass you back to the top menu now";
        public static string WeAreDoneWhatNext = "We are done now. Please ask another question.";
        public static string WhichCpu = "Which CPU would you like? You can say I3, I5 or I7";
        public static string WhichColour = "Which Colour would you like? You can say Silver or Black";
        public static string WhichConnectivity = "How will you like to get online? You can say WiFi or LTE";
        public static string WhichMemory = "How much memory would you like?";
        public static string WhichProduct = "Which specific product would you like?";
        public static string WhichProductFamily = "Which type of product would you like? You can say Xbox, Surface, Office etc";
        public static string WhichStorage = "How much storage would you like?";
        public static string TryASearch = "I cannot find any links, lets try a search";
        public static string IllSeeWhatICanFind = "I'll see what I can find for";
        public static string IFoundALink = "I found some links which may be useful";
    }
}
