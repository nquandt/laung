using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Language
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Tokens
    {
        EOF,
        WHITESPACE,
        INTEGER,
        STRING,
        KEYWORD,
        NULL,
        BOOLEAN,
        IDENTIFIER,
        PUNCTUATOR
    }
}
