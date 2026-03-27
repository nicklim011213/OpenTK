using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;

public static class EntryPoint
{
    static void Main()
    {
        App Application = new(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = (1920, 1080),
            Title = "ExampleWindow",
            APIVersion = new Version(4, 6),
            Profile = ContextProfile.Core,
            NumberOfSamples = 4,
            Vsync = OpenTK.Windowing.Common.VSyncMode.On,
            Flags = ContextFlags.Debug,
        });

        Console.WriteLine("=========INFO=========");
        Console.WriteLine("Renderer:   " + GL.GetString(StringName.Renderer));
        Console.WriteLine("Vendor:     " + GL.GetString(StringName.Vendor));
        Console.WriteLine("Version:    " + GL.GetString(StringName.Version));
        Console.WriteLine("Extensions: ");
        int Ext = GL.GetInteger(GetPName.NumExtensions);
        for (int i = 0; i != Ext; ++i)
        {
            Console.WriteLine("            " + GL.GetString(StringNameIndexed.Extensions, i));
        }
        Console.WriteLine("======================");

        Application.Run();
    }
}

partial class App(GameWindowSettings? gameWindowSettings, NativeWindowSettings? nativeWindowSettings): 
              GameWindow(gameWindowSettings, nativeWindowSettings)
{

}
