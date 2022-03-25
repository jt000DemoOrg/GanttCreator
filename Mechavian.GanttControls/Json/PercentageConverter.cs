using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Mechavian.GanttControls.Json
{
    public class PercentageConverter : JsonConverter<double?>
    {
        public override void WriteJson(JsonWriter writer, double? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override double? ReadJson(JsonReader reader, Type objectType, double? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.Value == null) return null;

                if (reader.Value is double pctValue) return pctValue;

                if (reader.Value is string strValue)
                {
                    var pctRegex = new Regex(@"^(?<val>\d+(.\d+)?)%$", RegexOptions.Singleline);
                    var match = pctRegex.Match(strValue);
                    if (match.Success)
                    {
                        return double.Parse(match.Groups["val"].Value) / 100d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw JsonSerializerException.Create(reader, $"Unable to parse '{reader.TokenType}' as percentage", ex);
            }

            throw JsonSerializerException.Create(reader, $"Unable to parse '{reader.TokenType}' as percentage");
        }

        public override bool CanWrite => false;
    }
}