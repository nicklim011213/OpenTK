using System.Text.Json;

public class StageSerlization
{
    public string GetSceneJSON(Stage Stage)
    {
        var Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        Options.Converters.Add(new JSONVec3());
        Options.Converters.Add(new JSONVec4());

        string JSONScene = "";
        JSONScene += JsonSerializer.Serialize(Stage, options: Options);

        return JSONScene;
    }
}
