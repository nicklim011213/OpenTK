using OpenTK.Mathematics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTKEngine.Obscured_Features.JSON
{
    public class JSONVec3 : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float x = 0, y = 0, z = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string PropertyName = reader.GetString();
                    reader.Read();

                    switch (PropertyName)
                    {
                        case "x": x = reader.GetSingle(); break;
                        case "y": y = reader.GetSingle(); break;
                        case "z": z = reader.GetSingle(); break;
                    }
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Vector3(x, y, z);
                }
            }
            return new Vector3(0, 0, 0);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteEndObject();
        }
    }

    public class JSONVec4 : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float x = 0, y = 0, z = 0, w = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string prop = reader.GetString();
                    reader.Read();

                    switch (prop)
                    {
                        case "x": x = reader.GetSingle(); break;
                        case "y": y = reader.GetSingle(); break;
                        case "z": z = reader.GetSingle(); break;
                        case "w": w = reader.GetSingle(); break;
                    }
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Vector4(x, y, z, w);
                }
            }
            return new Vector4(0, 0, 0, 0);
        }

        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteNumber("w", value.W);
            writer.WriteEndObject();
        }
    }
}
