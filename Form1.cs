using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Patient
{
    public partial class Form1 : Form
    {
        bool loaded = false;
        double __glPi = 3.14159265358979323846;
        int VertexShaderObject, FragmentShaderObject, ProgramObject;
        float mouseX, mouseY;
        string[] files;
        string ShaderName = "star";
        string ShaderDir = "./shaders/";
        string ShaderExt = ".glsl";
        string ShaderFilename;

      //  int TextureObject;
        Stopwatch stopwatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();

        }

 

        public List<string> GetFileList(string Root, bool SubFolders)
        {
            List<string> FileArray = new List<string>();
            try {
                string[] Files = System.IO.Directory.GetFiles(Root);
                string[] Folders = System.IO.Directory.GetDirectories(Root);

                for (int i = 0; i < Files.Length; i++)
                {
                    FileArray.Add(Files[i].ToString());
                }

                if (SubFolders == true) 
                {
                    for (int i = 0; i < Folders.Length; i++) 
                    {
                        FileArray.AddRange(GetFileList(Folders[i], SubFolders));
                    }
                }
            }
            catch (Exception Ex) 
            {
                throw (Ex);
            }
            return FileArray;
        }

        private void getShaderList(string ShaderDir, string ShaderName , string ShaderExt )
        {
            
            files = System.IO.Directory.GetFiles(ShaderDir, "*" + ShaderExt);

            for (int i = 0; i < files.Length; i++)
            {
                String s = files[i].Substring(10, files[i].Length - 15);
                comboBox1.Items.Add(s);
            }
        }


        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true;
            stopwatch.Start();
     
          // glControl1.MouseWheel += new MouseEventHandler(glControl1_MouseWheel);
          //  setupViewport();
          //  displayList = GL.GenLists(1);
          // declare some variables for tracking which shader did compile, and which texture to use

        
            String LogInfo;

         
            // Load&Compile Vertex Shader
            using (StreamReader sr = new StreamReader("./shaders/vertex_shader.glsl"))
            {
                VertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShaderObject, sr.ReadToEnd());
                GL.CompileShader(VertexShaderObject);
            }

            GL.GetShaderInfoLog(VertexShaderObject, out LogInfo);
            if (LogInfo.Length > 0 && !LogInfo.Contains("hardware"))
                Trace.WriteLine("Vertex Shader Log:\n" + LogInfo);
            else
                Trace.WriteLine("Vertex Shader compiled without complaint.");

            
            ShaderFilename = ShaderDir + ShaderName + ShaderExt;
            getShaderList(ShaderDir,ShaderName,ShaderExt);

            compileShader();

        }

        private void compileShader()
        {
            // Load&Compile Fragment Shader
            FragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);
            String LogInfo2;
            using (StreamReader sr = new StreamReader(ShaderFilename))
            {
                String shaderSrc1 = "uniform vec3 iResolution;\nuniform float iGlobalTime;\nuniform float iChannelTime[4];\n";
                String shaderSrc2 = "uniform vec4 iMouse;\n";
                //String shaderSrc3 = "uniform Sampler2D iChannel0;\nuniform Sampler2D iChannel1;\nuniform Sampler2D iChannel2;\nuniform Sampler2D iChannel3;\n";
                String shaderSrc3 = "";
                String shaderSrc4 = "";
                String shaderSrc = shaderSrc1 + shaderSrc2 + shaderSrc3 + shaderSrc4;

                shaderSrc += sr.ReadToEnd();
                textBox1.Text = shaderSrc;
                GL.ShaderSource(FragmentShaderObject, shaderSrc);
                Trace.WriteLine(GL.GetError());
                GL.CompileShader(FragmentShaderObject);
                Trace.WriteLine(GL.GetError());
            }
            GL.GetShaderInfoLog(FragmentShaderObject, out LogInfo2);
            Trace.WriteLine(GL.GetError());
            if (LogInfo2.Length > 0 /* && !LogInfo2.Contains("hardware")*/)
            {
                Trace.WriteLine("Compiling " + ShaderFilename + " failed!\nLog:\n" + LogInfo2);
                MessageBox.Show("Compiling " + ShaderFilename + " failed!\nLog:\n" + LogInfo2);
            }
            else
            {
                Trace.WriteLine("Fragment Shader compiled without complaint.");

            }

            
            // Link the Shaders to a usable Program
            ProgramObject = GL.CreateProgram();
            GL.AttachShader(ProgramObject, VertexShaderObject);
            GL.AttachShader(ProgramObject, FragmentShaderObject);
            GL.LinkProgram(ProgramObject);
            Trace.WriteLine(GL.GetError());
            // make current
            GL.UseProgram(ProgramObject);
            GL.GetProgramInfoLog(ProgramObject, out LogInfo2);
            if (LogInfo2.Length > 0 /* && !LogInfo2.Contains("hardware")*/)
            {
                Trace.WriteLine("Using " + ShaderFilename + " failed!\nLog:\n" + LogInfo2);
            }
            Trace.WriteLine(GL.GetError());

            // Flag ShaderObjects for delete when app exits
            GL.DeleteShader(VertexShaderObject);
            GL.DeleteShader(FragmentShaderObject);

        }
        void gluPerspective(double fovy, double aspect, double zNear, double zFar)
        {
            Matrix4d m = Matrix4d.Identity;
            double sine, cotangent, deltaZ;
            double radians = fovy / 2 * __glPi / 180;

            deltaZ = zFar - zNear;
            sine = Math.Sin(radians);
            if ((deltaZ == 0) || (sine == 0) || (aspect == 0))
            {
                return;
            }
            //TODO: check why this cos was written COS?
            cotangent = Math.Cos(radians) / sine;

            m.M11 = cotangent / aspect;
            m.M22 = cotangent;
            m.M33 = -(zFar + zNear) / deltaZ;
            m.M34 = -1;
            m.M43 = -2 * zNear * zFar / deltaZ;
            m.M44 = 0;

            GL.MultMatrix(ref m);
        }


        public void LookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            gluLookAt(eye.X, eye.Y, eye.Z, center.X, center.Y, center.Z, up.X, up.Y, up.Z);
        }

        void gluLookAt(double eyex, double eyey, double eyez, double centerx, double centery, double centerz, double upx, double upy, double upz)
        {
            Vector3 forward, side, up;
            Matrix4 m;

            forward.X = (float)(centerx - eyex);
            forward.Y = (float)(centery - eyey);
            forward.Z = (float)(centerz - eyez);

            up.X = (float)upx;
            up.Y = (float)upy;
            up.Z = (float)upz;

            forward.Normalize();

            /* Side = forward x up */
            side = Vector3.Cross(forward, up);
            side.Normalize();

            /* Recompute up as: up = side x forward */
            up = Vector3.Cross(side, forward);

            m = Matrix4.Identity;

            m.M11 = side.X;
            m.M21 = side.Y;
            m.M31 = side.Z;

            m.M12 = up.X;
            m.M22 = up.Y;
            m.M32 = up.Z;

            m.M13 = -forward.X;
            m.M23 = -forward.Y;
            m.M33 = -forward.Z;

            GL.MultMatrix(ref m);
            GL.Translate(-eyex, -eyey, -eyez);
        }
        private void setupViewport()
        {
            int w = glControl1.Width;
            int h = glControl1.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            gluPerspective(45.0f, (float)w / (float)h, 0.01f, 1000.0f);    // Todo: machen das da nich mehr veraltet steht
            gluLookAt(
                0.0, 0.0, 20.0,
                0.0, 0.0, 0.0,
                0.0, 1.0, 0.0);
            GL.MatrixMode(MatrixMode.Modelview);
         //   GL.Enable(EnableCap.Lighting);

            // GL.MultMatrix(ref m4);

        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;
          //  Trace.WriteLine("paint frame, timer = " + stopwatch.ElapsedMilliseconds);
            GL.ClearColor(1.0f, 0.2f, 0.2f, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.Color3(Color.Black);
            //GL.Uniform2(GL.GetUniformLocation(ProgramObject, "iResolution"), 474.0f, 327.0f);
            int deb = GL.GetUniformLocation(ProgramObject, "iResolution");
            GL.Uniform3(deb, (float)glControl1.Size.Width, (float)glControl1.Size.Height,1.0f);
            
            int muh = GL.GetUniformLocation(ProgramObject, "iGlobalTime");
            GL.Uniform1(muh, (float)(stopwatch.ElapsedMilliseconds / 1000.0f));

            muh = GL.GetUniformLocation(ProgramObject, "iChannelTime");
            if (muh != -1)
                GL.Uniform1(muh, (float)(stopwatch.ElapsedMilliseconds / 1000.0f));

            muh = GL.GetUniformLocation(ProgramObject, "iChannel0");
            if (muh != -1)
                GL.Uniform1(muh, (float)(stopwatch.ElapsedMilliseconds / 1000.0f));
            
            muh = GL.GetUniformLocation(ProgramObject, "time");
            if (muh != -1)
                GL.Uniform1(muh, (float)(stopwatch.ElapsedMilliseconds / 1000.0f));

            muh = GL.GetUniformLocation(ProgramObject, "resolution");
            if (muh != -1)
                GL.Uniform2(muh, (float)glControl1.Size.Width, (float)glControl1.Size.Height);

            muh = GL.GetUniformLocation(ProgramObject, "mouse");
            if (muh != -1)
                GL.Uniform2(muh, mouseX /  glControl1.Width, mouseY / glControl1.Height);           
            
            muh = GL.GetUniformLocation(ProgramObject, "iMouse");
            if (muh != -1)
                GL.Uniform2(muh, mouseX /  glControl1.Width, mouseY / glControl1.Height);           

            GL.Begin(BeginMode.Quads);
            {
                GL.Vertex2(-10.0f, -10.0f);
                GL.Vertex2(10.0f, -10.0f);
                GL.Vertex2(10.0f, 10.0f);
                GL.Vertex2(-10.0f, 10.0f);
            }


            GL.End();

           // Trace.WriteLine("vor swapbuffers");
            glControl1.SwapBuffers();
           // Trace.WriteLine("nach swapbuffers");
            glControl1.Invalidate();

            
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            /*
            glControl1.Update();
            int w = glControl1.Width;
            int h = glControl1.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            //GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
            GL.MatrixMode(MatrixMode.Modelview);
             * */
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Refresh();
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {

        }

        private void btnFullscreen_Click(object sender, EventArgs e)
        {
            //GraphicsMode.Fullscreen
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            new Form2().Show();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = -e.Y;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Trace.WriteLine("Shader: " + files[comboBox1.SelectedIndex]);
            ShaderFilename = files[comboBox1.SelectedIndex];
            compileShader();

            Form1.ActiveForm.Text = ProductName + " GLSL Viewer " + " V" + ProductVersion + " " + files[comboBox1.SelectedIndex];
             
        }

        FormWindowState LastWindowState = FormWindowState.Minimized;
        private void Form1_Resize(object sender, EventArgs e)
        {

            // When window state changes
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;


                if (WindowState == FormWindowState.Maximized)
                {

                    textBox1.Refresh();
                }
                if (WindowState == FormWindowState.Normal)
                {

                    textBox1.Refresh();
                }
            }

        }


    }
}
