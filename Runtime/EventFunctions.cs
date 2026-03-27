using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public partial class App : GameWindow
{
    private bool CursorUnlocked = false;
    public Renderer StageRenderer;

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        Camera.UpdateMovement(args, KeyboardState);
        Camera.UpdateMouseMovement(args, MouseState, CursorUnlocked);
        Camera.UpdateVectors();
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
        if (KeyboardState.IsKeyPressed(Keys.Tab))
        {
            if (CursorUnlocked)
            {
                CursorState = CursorState.Normal;
            }
            else
            {
                CursorState = CursorState.Grabbed;
            }
            CursorUnlocked = !CursorUnlocked;
        }
    }

    protected override void OnLoad()
    {
        // GL Context Setup
        base.OnLoad();
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.FramebufferSrgb);
        CursorState = CursorState.Grabbed;

        // Gui Setup
        GUI.GUIOnLoad(this);
        
        // Renderer Setup
        GlobalRenderVars.Init();
        Texture.CreateSafeDefault();

        // World Setup
        StageRenderer = WorldInit.CreateStage();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        StageRenderer.Render();
        GUI.GuiRender(this);
        Context.SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
