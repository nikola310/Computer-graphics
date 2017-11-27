// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        Cube cb;
        Cylinder cyl;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            cb = new Cube();
            cyl = new Cylinder();
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);


            //            gl.PushMatrix();
            //          gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 20000f);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Translate(0.0f, -750f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.Scale(20, 20, 20);
            //gl.Translate(0.5f, -5f, 0f);
            m_scene.Draw();
            gl.PushMatrix();
            //iscrtavanje podloge
            gl.Color(0.09f, 0.43f, 0.34f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(250f, 0f, 250f);
            gl.Vertex(250f, 0f, -250f);
            gl.Vertex(-250f, 0f, -250f);
            gl.Vertex(-250f, 0f, 250f);
            gl.End();
            gl.PopMatrix();

            //gl.PushMatrix();
            //drawBlue3DText(gl);

            drawEscalator(gl);

            gl.PopMatrix();
            //gl.PopMatrix();

            /*gl.PushMatrix();
            gl.Color(1f, 1f, 1f);
            //gl.Translate(0.5f, -5f, 0f);
            gl.DrawText3D("Arial", 50f, 1f, 0.1f, "teapot");
            gl.DrawText(10, 30, 0.0f, 1.0f, 0.0f, "Courier New", 12, "Perspektiva");
            gl.PopMatrix();*/



            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        /// <summary>
        /// Iscrtava pokretne stepenice.
        /// </summary>
        public void drawEscalator(OpenGL gl)
        {
            //iscrtavanje samih stepenica
            //===========================================
            float scale = 15f;
            float translateX = 5;
            float translateY = 0;
            float translateZ = 0;

            drawCubesForEscalator(gl, translateX, translateY, translateZ, scale);
            translateZ += 2;
            drawCubesForEscalator(gl, translateX, translateY, translateZ, scale);
            //=====================================
            //iscrtavanje tela stepenica - cilindri
            //=====================================
            cyl.BaseRadius = 2;
            cyl.Height = 5;
            cyl.TopRadius = 2;

            gl.PushMatrix();
            gl.Color(0.5f, 0.5f, 0.5f);
            gl.Scale(15f, 15f, 15f);
            gl.Translate(8f, 0f, -1.5f);
            cyl.CreateInContext(gl);
            cyl.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Color(0.5f, 0.5f, 0f);
            gl.Scale(15f, 15f, 15f);
            gl.Translate(10.5f, 3.0f, -1.5f);
            cyl.CreateInContext(gl);
            cyl.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //===================================
            //ostatak tela stepenica - kocke
            translateX = 13;
            translateY = 0;
            translateZ = 0;
            drawCubesForEscalatorBody(gl, translateX, translateY, translateZ, scale);
            translateZ += 2;
            drawCubesForEscalatorBody(gl, translateX, translateY, translateZ, scale);
            //===============================
        }
        /// <summary>
        /// Iscrtava kocke za pokretne stepenice
        /// </summary>
        public void drawCubesForEscalator(OpenGL gl, float translateX, float translateY, float translateZ, float scale)
        {
            for (int i = 0; i <= 6; i += 2)
            {
                gl.PushMatrix();
                gl.Color(0.89f, 0.75f, 0f);
                gl.Scale(scale, scale, scale);
                gl.Translate(translateX + i, translateY + i, translateZ);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }
        }

        /// <summary>
        /// Iscrtava kocke za telo pokretnih stepenica
        /// </summary>
        public void drawCubesForEscalatorBody(OpenGL gl, float translateX, float translateY, float translateZ, float scale)
        {
            for (int i = 0; i <= 6; i += 2)
            {
                gl.PushMatrix();
                gl.Color(0.0f, 0.75f, 0.89f);
                gl.Scale(scale, scale, scale);
                gl.Translate(translateX, translateY + i, translateZ);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }
        }

        public void drawBlue3DText(OpenGL gl)
        {
            float txtX = 0.0f;
            float txtY = 0.0f;
            float txtZ = -5.0f;
            //redefinisanje projekcije tako da bude u donjem desnom uglu
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            //gl.Ortho2D(-100.0f, 60.0f, -130.0f, 50.0f);
            //gl.Ortho2D(m_)
            //gl.Ortho2D()
            //gl.Perspective(35.0, m_width / (double)m_height, 0.5, 40.0);
            gl.Color(1f, 1f, 1f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Scale(10.0f, 10.0f, 10.0f);
            gl.PushMatrix();
            gl.Translate(txtX, txtY, txtZ);
            gl.DrawText3D("Arial", 12f, 0f, 0f, "Predmet: Racunarska grafika");
            gl.PopMatrix();
            gl.Viewport((m_width / 4) * 3, (m_height / 4) * 3, m_width / 4, m_height / 4);
            gl.DrawText(10, 30, 0.0f, 1.0f, 0.0f, "Courier New", 12, "Perspektiva");
            gl.PopMatrix();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            //podesavanje Viewport preko celog prozora
            gl.Viewport(0, 0, m_width, m_height);
            //definisanje perspektive
            gl.Perspective(45f, (double)width / height, 0.5f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
