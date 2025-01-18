using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorGrid.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class RawJavaScriptConverter : JsonConverter<RawJavaScript>
{
    public override RawJavaScript Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of raw JavaScript is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, RawJavaScript value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Script);
    }
}


