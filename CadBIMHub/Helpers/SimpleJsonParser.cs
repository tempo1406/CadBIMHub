using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CadBIMHub.Models;

namespace CadBIMHub.Helpers
{
    public class SimpleJsonParser
    {
        public static RouteDataResponse ParseRouteData(string json)
        {
            var response = new RouteDataResponse
            {
                Items = new List<RouteItemDto>()
            };

            var totalCountMatch = Regex.Match(json, @"""totalCount"":\s*(\d+)");
            if (totalCountMatch.Success)
            {
                response.TotalCount = int.Parse(totalCountMatch.Groups[1].Value);
            }

            var itemsMatch = Regex.Match(json, @"""items"":\s*\[(.*)\]", RegexOptions.Singleline);
            if (!itemsMatch.Success) return response;

            string itemsContent = itemsMatch.Groups[1].Value;
            
            var objectMatches = Regex.Matches(itemsContent, @"\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}", RegexOptions.Singleline);

            foreach (Match objMatch in objectMatches)
            {
                string objContent = objMatch.Value;
                var item = new RouteItemDto
                {
                    CustomFields = new List<object>()
                };

                item.Id = ExtractInt(objContent, "id");
                item.Name = ExtractString(objContent, "name");
                item.Code = ExtractString(objContent, "code");
                item.Description = ExtractString(objContent, "description");
                item.BatchNo = ExtractString(objContent, "batchNo");
                item.Symbol = ExtractString(objContent, "symbol");
                item.Quantity = ExtractDouble(objContent, "quantity");
                item.ItemGroupId = ExtractInt(objContent, "itemGroupId");
                item.ItemGroupName = ExtractString(objContent, "itemGroupName");
                item.ItemId = ExtractInt(objContent, "itemId");
                item.ItemDescription = ExtractString(objContent, "itemDescription");
                item.SizeId = ExtractInt(objContent, "sizeId");
                item.SizeName = ExtractString(objContent, "sizeName");
                item.Status = ExtractInt(objContent, "status");
                item.CreateBy = ExtractString(objContent, "createBy");
                item.IsActive = ExtractInt(objContent, "isActive");

                response.Items.Add(item);
            }

            return response;
        }

        private static string ExtractString(string json, string fieldName)
        {
            var match = Regex.Match(json, $@"""{fieldName}"":\s*""([^""]*)""");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int ExtractInt(string json, string fieldName)
        {
            var match = Regex.Match(json, $@"""{fieldName}"":\s*(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static double ExtractDouble(string json, string fieldName)
        {
            var match = Regex.Match(json, $@"""{fieldName}"":\s*(\d+(?:\.\d+)?)");
            return match.Success ? double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : 0.0;
        }
    }
}
