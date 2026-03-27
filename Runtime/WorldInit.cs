using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class WorldInit
{
    public static Renderer CreateStage()
    {
        Stage stage = new Stage();

        var StageRenderer = new Renderer()
        {
            DefaultShader = new Shader("Vertex.vs", "Fragment.fs"),
            FrameBuffer = new FrameBuffer(),
            ShadowMapper = new ShadowMapper(),
            Stage = stage,
        };

        Light light = new Light(
            Pos: new Vector3(10.0f, 20.0f, 0.0f),
            Ambient: new Vector3(0.1f, 0.1f, 0.1f),
            Diffuse: new Vector3(0.8f, 0.8f, 0.8f),
            Specular: new Vector3(0.5f, 0.5f, 0.5f)
        );

        Actor backpack = new Actor(
            ModelFilePath: "backpack\\backpack.obj",
            Pos: new Vector3(0f, -1.25f, 0.55f),
            Scale: new Vector3(0.25f, 0.25f, 0.25f),
            Rot: new Quaternion(new Vector3(MathHelper.DegreesToRadians(-20), 0.0f, 0.0f))
        );

        Actor tree = new Actor(
            ModelFilePath: "Tree\\Tree.obj",
            Pos: new Vector3(0f, -1.75f, 0f),
            Scale: new Vector3(2, 2, 2)
        );

        Actor floor = new Actor(
            ModelFilePath: "Floor\\Floor.obj",
            Pos: new Vector3(0, -2, 0),
            Scale: new Vector3(5.0f, 0.2f, 5.0f)
        );

        stage.Actors.Add(backpack);
        stage.Actors.Add(tree);
        stage.Actors.Add(floor);
        stage.Lights.Add(light);

        stage.UploadLight();
        stage.UploadActors();

        return StageRenderer;
    }
}
