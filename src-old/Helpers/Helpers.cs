using System.Collections.Generic;
using System.Globalization;

namespace GuidedSearchBot.Helpers
{
    public static class Helpers
    {
        public static string GetEntityString(Dictionary<string,string> entities)
        {
            var textInfo = new CultureInfo("en-GB", false).TextInfo;
            var entityString = string.Empty;
            foreach (var entity in entities)
            {
                if (entity.Key == "memory")
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else if (entity.Key == "storage")
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value} {entity.Key}, ");
                }
                else
                {
                    entityString += textInfo.ToTitleCase($"{entity.Value}, ");
                }
            }

            entityString = entityString.TrimEnd(' ');
            entityString = entityString.TrimEnd(',');

            return entityString;
        }
    }
}
