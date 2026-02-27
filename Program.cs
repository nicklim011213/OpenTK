using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

class EntryPoint()
{
    static void Main()
    {
        App Application = new(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = (1920, 1080),
            Title = "ExampleWindow",
            APIVersion = new Version(4, 5),
            Profile = ContextProfile.Core,
            NumberOfSamples = 4,
            Vsync = OpenTK.Windowing.Common.VSyncMode.On
        });

        Stage MyStage = new();
        MyStage.Instances.Add(new Instance {
            Position = new Vector3(0,0,-5),
            Scale = new Vector3(1,1,1),
            Rotation = new Quaternion(0,0,0),
            Model = new Model("backpack\\backpack.obj")
        });

        MyStage.Instances.Add(new Instance
        {
            Position = new Vector3(7, -2, -5),
            Scale = new Vector3(2, 2, 2),
            Rotation = new Quaternion(0, 0, 0),
            Model = new Model("Tree\\Tree.obj")
        });

        MyStage.Instances.Add(new Instance
        {
            Position = new Vector3(0, -2, -5),
            Scale = new Vector3(1, 1, 1),
            Rotation = new Quaternion(0, 0, 0),
            Model = new Model("Floor\\Floor.obj")
        });


        Light light = new Light(
            new Vector3(5.0f, 0.0f, 0.0f),
            new Vector3(0.1f, 0.1f, 0.1f),
            new Vector3(0.8f, 0.8f, 0.8f),
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        App.RenderStage = MyStage;  

        Application.Run();
    }
}

partial class App(GameWindowSettings? gameWindowSettings, NativeWindowSettings? nativeWindowSettings): 
              GameWindow(gameWindowSettings, nativeWindowSettings)
{

}
