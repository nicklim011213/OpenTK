using System.Text.Json;
using OpenTKEngine.Exposed_Features.EngineObjects;

namespace OpenTKEngine.Obscured_Features.JSON
{
    public class StageSerlization
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        public static string GetSceneJSON(Stage Stage)
        {
            Options.Converters.Add(new JSONVec3());
            Options.Converters.Add(new JSONVec4());

            string JSONScene = "";
            JSONScene += JsonSerializer.Serialize(Stage, options: Options);

            return JSONScene;
        }
    }
}
