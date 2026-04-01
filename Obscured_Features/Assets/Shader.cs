using OpenTK.Graphics.OpenGL4;

namespace OpenTKEngine.Obscured_Features.Assets
{
    public class Shader
    {
        public int Handle = 0;
        public int MatShininessLoc = -1;

        public Shader(string VertexShaderFile, string FragmentShaderFile)
        {
            string ShaderFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Shaders");
            string VertexShaderSource = Path.Combine(ShaderFolderPath, VertexShaderFile);
            string FragmentShaderSource = Path.Combine(ShaderFolderPath, FragmentShaderFile);

            Console.WriteLine(FragmentShaderSource);
            Console.WriteLine(VertexShaderSource);

            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, File.ReadAllText(VertexShaderSource));

            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, File.ReadAllText(FragmentShaderSource));

            GL.CompileShader(VertexShader);
            CheckShaderCompileStatus(VertexShader);
            GL.CompileShader(FragmentShader);
            CheckShaderCompileStatus(FragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);
            GL.LinkProgram(Handle);
            GetLinkStatus(Handle, VertexShaderFile, FragmentShaderFile);

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            InitLocations();
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        private static void CheckShaderCompileStatus(int Shader)
        {
            GL.GetShader(Shader, ShaderParameter.CompileStatus, out int success);

            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(Shader);
                Console.WriteLine($"Shader compile error:\n{log}");
            }
        }

        private static void GetLinkStatus(int ProgramHandle, string VSStr, string FSStr)
        {
            GL.GetProgram(ProgramHandle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string log = GL.GetProgramInfoLog(ProgramHandle);
                Console.WriteLine("Program link error:\n" + log);
                Console.WriteLine("Shader Combo: " + VSStr + " & " + FSStr);
            }
        }

        private void InitLocations()
        {
            MatShininessLoc = GL.GetUniformLocation(this.Handle, "Shininess");
        }
    }
}
