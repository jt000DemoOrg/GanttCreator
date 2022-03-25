using System;
using Newtonsoft.Json;

namespace Mechavian.GanttControls.Json
{
    internal static class JsonSerializerException
    {
        internal static JsonSerializationException Create(JsonReader reader, string message, Exception ex = null)
        {
            return Create(reader as IJsonLineInfo, reader.Path, message, ex);
        }

        internal static JsonSerializationException Create(IJsonLineInfo lineInfo, string path, string message, Exception ex)
        {
            message = FormatMessage(lineInfo, path, message);

            int lineNumber;
            int linePosition;
            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }

            return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
        }

        internal static string FormatMessage(IJsonLineInfo? lineInfo, string path, string message)
        {
            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                message = message.Trim();

                if (!message.EndsWith('.'))
                {
                    message += ".";
                }

                message += " ";
            }

            message += $"Path '{path}'";

            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                message += $", line {lineInfo.LineNumber}, position {lineInfo.LinePosition}";
            }

            message += ".";

            return message;
        }
    }
}