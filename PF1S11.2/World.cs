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
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Collections.Generic;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

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

        /// <summary>
        /// Tip fonta za iscrtavanje teksta u donjem desnom uglu.
        /// </summary>
        private string fontType = "Arial Bold";

        /// <summary>
        /// Velicina fonta za iscrtavanje teksta u donjem desnom uglu.
        /// </summary>
        private float fontSize = 12f;

        /// <summary>
        ///	 Identifikatori tekstura za jednostavniji pristup teksturama
        /// </summary>
        private enum TextureObjects { Metal = 0, Ceramic };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "..//..//images//metal.jpg", "..//..//images//ceramic.jpg" };

        /// <summary>
        /// Tekst koji ce biti ispisan u donjem desnom uglu.
        /// </summary>
        List<string> rightCornerText = new List<string>() { "Predmet: Racunarska grafika", "Sk.god: 2017/18.", "Ime: Nikola", "Prezime: Stojanovic", "Sifra zad: 11.2" };

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

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
            m_textures = new uint[m_textureCount];
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

            // Teksture se primenjuju sa parametrom decal
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);		// Nearest neighbor Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);     // Nearest neighbor Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }

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

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 20000f);

            //podesavanje tekstura
            //setTextures(gl

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Translate(0.0f, -750f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.Scale(20, 20, 20);
            m_scene.Draw();

            //iscrtavanje podloge
            DrawFloor(gl);

            DrawEscalator(gl);

            gl.PopMatrix();

            DrawBlue3DText(gl);

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        /// <summary>
        /// Iscrtava podlogu
        /// </summary>
        /// <param name="gl"></param>
        public void DrawFloor(OpenGL gl)
        {
            gl.PushMatrix();
            //gl.Color(0.09f, 0.43f, 0.34f);
            //gl.Begin(OpenGL.GL_QUADS);
            /*gl.Vertex(300f, 0f, 300f);
            gl.Vertex(300f, 0f, -300f);
            gl.Vertex(-300f, 0f, -300f);
            gl.Vertex(-300f, 0f, 300f);
            gl.End();
            gl.PopMatrix();*/


            // Pod tunela
            gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
            //gl.Scale(10f, 10f, 10f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Ceramic]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(300f, 0f, 300f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(300f, 0f, -300f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(-300f, 0f, -300f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-300f, 0f, 300f);
            gl.End();
            gl.PopMatrix();
        }

        /// <summary>
        /// Podesava teksture
        /// </summary>
        /// <param name="gl"></param>
        public void SetTextures(OpenGL gl)
        {
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
        }

        /// <summary>
        /// Iscrtava pokretne stepenice.
        /// </summary>
        public void DrawEscalator(OpenGL gl)
        {
            //iscrtavanje samih stepenica
            //===========================================
            float scale = 15f;

            #region Iscrtavanje stepenica
            gl.PushMatrix();
            gl.Color(0.5f, 0.5f, 0.5f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Metal]);
            for (int i = 0; i <= 8; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(5 + i, 0, -0.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 6; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(7 + i, 2, -0.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 4; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(9 + i, 4, -0.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 2; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(11 + i, 6, -0.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 8; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(5 + i, 0, 1.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 6; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(7 + i, 2, 1.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 4; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(9 + i, 4, 1.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            for (int i = 0; i <= 2; i += 2)
            {
                gl.PushMatrix();
                gl.Scale(scale, scale, scale);
                gl.Translate(11 + i, 6, 1.5);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }
            gl.PopMatrix();
            #endregion Iscrtavanje stepenica

            //=====================================
            //cilindar - drska

            //===============================
        }

        /// <summary>
        /// Iscrtava 3D tekst u donjem desnom uglu prozora
        /// </summary>
        /// <param name="gl"></param>
        public void DrawBlue3DText(OpenGL gl)
        {
            //===================================
            //definisanje potrebnih promenljivih
            float x = 0.0f;
            float y = -20.5f;
            float z = 0.0f;
            float scaleFactor = 5.0f;
            float offset = 0f;
            //===================================
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-115f, 65.0f, -125.0f, 10.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PushMatrix();
            gl.Color(0f, 0f, 1f);
            gl.Scale(scaleFactor, scaleFactor, scaleFactor);

            //foreach petlja za iscrtavanje teksta u donjem desnom uglu.
            foreach (string s in rightCornerText)
            {
                gl.PushMatrix();
                gl.Translate(x, y - offset, z);
                gl.DrawText3D(fontType, fontSize, 0f, 0f, s);
                gl.PopMatrix();
                offset++;
            }
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