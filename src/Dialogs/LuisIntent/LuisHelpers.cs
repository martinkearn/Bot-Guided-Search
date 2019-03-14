using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasicBot.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BasicBot.Dialogs.LuisIntent
{
    public static class LuisHelpers
    {
        private const string EntityProductFamily = "ProductFamily";
        private const string EntityCPU = "CPU";
        private const string EntityColour = "Colour";
        private const string EntityConnectivity = "Connectivity";
        private const string EntityGb = "Gb";
        private const string EntityMemory = "Memory";
        private const string EntityProduct = "Product";
        private const string EntityStorage = "Storage";
        private static List<LuisEntity> result = new List<LuisEntity>();

        public static List<LuisEntity> GetEntities(RecognizerResult recognizerResult)
        {
            result.Clear();

            foreach (var entity in recognizerResult.Entities)
            {
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityProductFamily);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityProduct);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityMemory);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityCPU);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityColour);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityConnectivity);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityGb);
                AddLuisEntityIfPresent(entity.Value.ToString(), EntityStorage);
            }

            return result;
        }

        private static void AddLuisEntityIfPresent(string entityValue, string entityName)
        {
            try
            {
                var j = JObject.Parse(entityValue)[entityName];
                if (j != null)
                {
                    var e = new LuisEntity()
                    {
                        Text = j[0]["text"].ToString(),
                        Type = j[0]["type"].ToString(),
                        Score = Convert.ToDouble(j[0]["score"]),
                    };
                    result.Add(e);
                }
            }
            catch (Exception e)
            {
                var message = e.Message;
            }
        }

    }
}
