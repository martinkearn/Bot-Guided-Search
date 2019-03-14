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

        public static List<LuisEntity> GetEntities(RecognizerResult recognizerResult)
        {
            var result = new List<LuisEntity>();

            foreach (var entity in recognizerResult.Entities)
            {
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityProductFamily);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityProduct);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityMemory);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityCPU);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityColour);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityConnectivity);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityGb);
                result = AddLuisEntityIfPresent(result, entity.Value.ToString(), EntityStorage);
            }

            return result;
        }

        private static List<LuisEntity> AddLuisEntityIfPresent(List<LuisEntity> result, string entityValue, string entityName)
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
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return result;
            }
        }

    }
}
