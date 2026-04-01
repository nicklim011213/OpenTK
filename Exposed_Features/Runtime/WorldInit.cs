using OpenTK.Mathematics;
using OpenTKEngine.Exposed_Features.EngineObjects;
using OpenTKEngine.Obscured_Features.Rendering;
using OpenTKEngine.Obscured_Features.Assets;

namespace OpenTKEngine.Exposed_Features.Runtime
{
    public static class WorldInit
    {
        public static Renderer CreateStage()
        {
            Stage stage = new();

            Light light = new(
                Pos: new Vector3(10.0f, 20.0f, 0.0f),
                Ambient: new Vector3(0.1f, 0.1f, 0.1f),
                Diffuse: new Vector3(0.8f, 0.8f, 0.8f),
                Specular: new Vector3(0.5f, 0.5f, 0.5f)
            );

            Actor backpack = new(
                ModelFilePath: "backpack\\backpack.obj",
                Pos: new Vector3(0f, -1.25f, 0.55f),
                Scale: new Vector3(0.25f, 0.25f, 0.25f),
                Rot: new Quaternion(new Vector3(MathHelper.DegreesToRadians(-20), 0.0f, 0.0f))
            );

            Actor tree = new(
                ModelFilePath: "Tree\\Tree.obj",
                Pos: new Vector3(0f, -1.75f, 0f),
                Scale: new Vector3(2, 2, 2)
            );

            Actor floor = new(
                ModelFilePath: "Floor\\Floor.obj",
                Pos: new Vector3(0, -2, 0),
                Scale: new Vector3(5.0f, 0.2f, 5.0f)
            );

            stage.Actors.Add(backpack);
            stage.Actors.Add(tree);
            stage.Actors.Add(floor);
            stage.Lights.Add(light);

            var StageRenderer = new Renderer
            (
                Stage: stage,
                Shader: new Shader("Vertex.vs", "Fragment.fs")
            );
            StageRenderer.ShadowMapper = new ShadowMapper();
            StageRenderer.FrameBuffer = new FrameBuffer();

            StageRenderer.UploadStageData();

            return StageRenderer;
        }
    }
}
