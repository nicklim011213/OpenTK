using Dear_ImGui_Sample.Backends;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKEngine.Obscured_Features.GUI
{
    public static class GUI
    {

        internal static void GUIOnLoad(OpenTKEngine.Exposed_Features.Runtime.App app)
        {
            ImGui.CreateContext();
            ImGuiIOPtr ImGuiIO = ImGui.GetIO();
            ImGuiIO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            ImGuiIO.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
            ImGuiIO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGuiIO.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

            ImGui.StyleColorsDark();

            ImGuiStylePtr style = ImGui.GetStyle();
            if ((ImGuiIO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                style.WindowRounding = 0.0f;
                style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            }

            ImguiImplOpenTK4.Init(app);
            ImguiImplOpenGL3.Init();
        }

        internal static void GuiRender(OpenTKEngine.Exposed_Features.Runtime.App app)
        {
            ImguiImplOpenGL3.NewFrame();
            ImguiImplOpenTK4.NewFrame();
            ImGui.NewFrame();

            ImGui.ShowDemoWindow();
            ImGui.Render();
            GL.Viewport(0, 0, app.FramebufferSize.X, app.FramebufferSize.Y);
            ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }
    }
}
