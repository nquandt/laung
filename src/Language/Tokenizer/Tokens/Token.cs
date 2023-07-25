using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Language
{
    public abstract class Token
    {
        public abstract Tokens Kind { get; }


        [JsonIgnore]
        public byte[] RawValue { get; set; } = Array.Empty<byte>();
                
        public string ByteRepresentation => string.Join(", ", RawValue.Select(x => "0x" + x.ToString("X2")));

        public string StringValue => System.Text.Encoding.UTF8.GetString(RawValue);
    }
}
