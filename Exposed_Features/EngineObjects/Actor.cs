using OpenTK.Mathematics;
using System.Text.Json.Serialization;
using OpenTKEngine.Obscured_Features.Rendering;

namespace OpenTKEngine.Exposed_Features.EngineObjects
{
    public class Actor
    {
        [JsonIgnore]
        public ObjectRenderData Internal_Model;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public string ModelFilePath;
        public Actor(string ModelFilePath, Vector3? Pos = null, Vector3? Scale = null, Quaternion? Rot = null)
        {
            this.Position = Pos ?? Vector3.Zero;
            this.Rotation = Rot ?? Quaternion.Identity;
            this.Scale = Scale ?? Vector3.One;
            this.ModelFilePath = ModelFilePath;
            Internal_Model = new ObjectRenderData(ModelFilePath);
        }
    }
}
