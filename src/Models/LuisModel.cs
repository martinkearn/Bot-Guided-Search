﻿// <auto-generated>
// Code generated by LUISGen GuidedSearchBot-a4a3.json -cs Luis.LuisModel -o 
// Tool github: https://github.com/microsoft/botbuilder-tools
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;

namespace BasicBot.Models
{
    public class LuisModel : IRecognizerConvert
    {
        public string Text;
        public string AlteredText;
        public enum Intent
        {
            Cancel,
            Help,
            None,
            Search
        };
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {
            // Simple entities
            public string[] Colour;
            public string[] Connectivity;
            public string[] CPU;
            public string[] Gb;
            public string[] Product;
            public string[] ProductFamily;

            // Lists
            public string[][] MemoryIndicator;
            public string[][] StorageIndicator;

            // Composites
            public class _InstanceMemory
            {
                public InstanceData[] Gb;
                public InstanceData[] MemoryIndicator;
            }
            public class MemoryClass
            {
                public string[] Gb;
                public string[][] MemoryIndicator;
                [JsonProperty("$instance")]
                public _InstanceMemory _instance;
            }
            public MemoryClass[] Memory;

            public class _InstanceStorage
            {
                public InstanceData[] Gb;
                public InstanceData[] StorageIndicator;
            }
            public class StorageClass
            {
                public string[] Gb;
                public string[][] StorageIndicator;
                [JsonProperty("$instance")]
                public _InstanceStorage _instance;
            }
            public StorageClass[] Storage;

            // Instance
            public class _Instance
            {
                public InstanceData[] Colour;
                public InstanceData[] Connectivity;
                public InstanceData[] CPU;
                public InstanceData[] Gb;
                public InstanceData[] Product;
                public InstanceData[] ProductFamily;
                public InstanceData[] MemoryIndicator;
                public InstanceData[] StorageIndicator;
                public InstanceData[] Memory;
                public InstanceData[] Storage;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<LuisModel>(JsonConvert.SerializeObject(result));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
