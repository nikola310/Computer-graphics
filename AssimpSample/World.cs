// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using SharpGL;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;

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
        private float m_sceneDistance = 70.0f;

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

        /// <summary>
        /// Omogucava interakciju preko tastature
        /// </summary>
        public bool keyEventsEnabled = true;

        /// <summary>
        /// Ambijentalna komponenta tackastog izvora svetlosti
        /// </summary>
        private float[] ambientComponent = { 1.0f, 1.0f, 1.0f, 1.0f };

        /// <summary>
        /// Faktor skaliranja osobe po x osi.
        /// </summary>
        private float obesity = 8.0f;
        Cube cb;
        Cylinder cyl;

        /// <summary>
        /// Parametri za animaciju
        /// </summary>
        private DispatcherTimer timer1;
        private float translatePersonX = 0.0f;
        private float translatePersonY = 0.0f;
        private float translatePersonZ = -10.0f;
        private bool animationRunning = false;
        private float v = 0.2f;
        private float[] translateEscalatorX = { 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f, 17.0f };
        private float[] translateEscalatorY = { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f };
        private bool personOnEscalator = false;

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

        public float[] AmbientComponent
        {
            get { return ambientComponent; }
            set { ambientComponent = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float Obesity
        {
            get { return obesity; }
            set { obesity = value; }
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
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            //Ukljucen color tracking mehanizam
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            // Rad sa teksturama
            Set_Textures(gl);

            //Set_Camera(gl);

            Setup_Lighting(gl);

            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        /// Podesava osvetljenje
        /// </summary>
        /// <param name="gl"></param>
        public void Setup_Lighting(OpenGL gl)
        {
            float[] ambLight = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] difLight = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] spcLight = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightPos = { 5.0f, 0.0f, 10000.0f, 1.0f };

            // Rad sa osvetljenjem
            // Ukljuci normalizaciju
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);

            // Podesi osvetljenje
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, ambLight);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambientComponent);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difLight);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, spcLight);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, lightPos);

            gl.Enable(OpenGL.GL_LIGHT0);

            // Reflektorski svetlosni izvor
            float[] refPozicija = new float[] { 15.0f, 25.0f, 0.0f, 1.0f };
            //float[] refSpekularnaKomponenta = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] refAmbijentalnaKomponenta = { 0.0f, 0.0f, 1.0f, 1.0f };
            float[] refDifuznaKomponenta = { 0.0f, 0.0f, 1.0f, 0.5f };
            float[] smer = { 0.0f, -1.0f, 0.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, refAmbijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, refDifuznaKomponenta);
            // Podesi parametre reflektorskog izvora
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            // Ukljuci svetlosni izvor
            gl.Enable(OpenGL.GL_LIGHT1);
            // Pozicioniraj svetloni izvor
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, refPozicija);

            gl.ShadeModel(OpenGL.GL_SMOOTH);
        }

        /// <summary>
        /// Podesavanje tekstura
        /// </summary>
        /// <param name="gl"></param>
        public void Set_Textures(OpenGL gl)
        {
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
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 20000f);
            gl.Viewport(0, 0, m_width, m_height);

            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.LookAt(-20.0f, 10.0f, 0.0f, 0.0f, 10.0f, 0.0f, 0.0f, 1.0f, 0.0f);

            gl.PushMatrix();

            gl.MatrixMode(OpenGL.GL_MODELVIEW_MATRIX);

            gl.Rotate(90.0f, 0.0f, 1.0f, 0.0f);
            gl.PushMatrix();

            Draw_Person(gl);

            DrawFloor(gl);

            gl.Rotate(90.0f, 0.0f, 1.0f, 0.0f);

            DrawEscalator(gl);

            DrawBlue3DText(gl);

            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        /// <summary>
        /// Iscrtavanje modela osobe
        /// </summary>
        /// <param name="gl"></param>
        public void Draw_Person(OpenGL gl)
        {
            gl.Color(0.65f, 0.65f, 0.65f, 0.0f);
            gl.Rotate(0.0f, 90.0f, 0.0f);
            gl.Translate(translatePersonX, translatePersonY, translatePersonZ);
            gl.Scale(obesity, 7.0f, 7.0f);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            m_scene.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
        }

        /// <summary>
        /// Iscrtava podlogu
        /// </summary>
        /// <param name="gl"></param>
        public void DrawFloor(OpenGL gl)
        {
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Ceramic]);
            gl.MatrixMode(OpenGL.GL_MODELVIEW_MATRIX);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(LightingUtilities.FindFaceNormal(30f, 0f, 30f, 30f, 0f, -30f, -30f, 0f, -30f));
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(30f, 0f, 30f);
            gl.TexCoord(0.0f, 4.0f);
            gl.Vertex(30f, 0f, -30f);
            gl.TexCoord(4.0f, 4.0f);
            gl.Vertex(-30f, 0f, -30f);
            gl.TexCoord(4.0f, 0.0f);
            gl.Vertex(-30f, 0f, 30f);
            gl.End();
            gl.PopMatrix();
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
        /// Iscrtava pokretne stepenice.
        /// </summary>
        public void DrawEscalator(OpenGL gl)
        {
            gl.PushMatrix();
            //gl.Color(0.5f, 0.5f, 0.5f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Metal]);
            //iscrtavanje samih stepenica
            //===========================================
            float scaleX = 1.0f;
            float scaleY = 0.5f;
            float scaleZ = 2.15f;
            for (int i = 0; i < translateEscalatorX.Length; i++)
            {
                gl.PushMatrix();
                gl.Scale(scaleX, scaleY, scaleZ);
                gl.Translate(translateEscalatorX[i], translateEscalatorY[i], 0.25);
                cb.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            }

            gl.PopMatrix();

            //=====================================
            gl.PushMatrix();
            gl.Translate(3.0f, 1.5f, 2.75f);
            gl.Rotate(90f, 0f, 1f, 0f);
            gl.Rotate(-43, 1, 0, 0);
            gl.Color(0.36f, 0.36f, 0.36f);
            cyl.TopRadius = 0.25;
            cyl.Height = 10;
            cyl.BaseRadius = 0.25;
            cyl.CreateInContext(gl);

            cyl.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(3.0f, 1.0f, 2.75f);
            gl.Scale(0.25f, 1.0f, 0.25f);
            gl.Color(0.36f, 0.36f, 0.36f);
            cb.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(10.5f, 4.0f, 2.75f);
            gl.Scale(0.25f, 5.0f, 0.25f);
            gl.Color(0.36f, 0.36f, 0.36f);
            cb.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(3.0f, 1.5f, -1.75f);
            gl.Rotate(90f, 0f, 1f, 0f);
            gl.Rotate(-43, 1, 0, 0);
            gl.Color(0.36f, 0.36f, 0.36f);
            cyl.TopRadius = 0.25;
            cyl.Height = 10;
            cyl.BaseRadius = 0.25;
            cyl.CreateInContext(gl);
            cyl.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(3.0f, 1.0f, -1.75f);
            gl.Scale(0.25f, 1.0f, 0.25f);
            gl.Color(0.36f, 0.36f, 0.36f);
            cb.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(10.5f, 4.0f, -1.75f);
            gl.Scale(0.25f, 5.0f, 0.25f);
            gl.Color(0.36f, 0.36f, 0.36f);
            cb.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //=====================================

            gl.PopMatrix();
        }

        /// <summary>
        ///  Funkcija ograničava vrednost na opseg min - max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        /// <summary>
        /// Funkcija koja odredjuje translaciju osobe u sklopu animacije.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MovePerson(object sender, EventArgs e)
        {
            if (translatePersonZ <= 2.5f && translatePersonZ <= 15.4f && animationRunning == true)
            {
                translatePersonZ += 0.5f;
            }
            else if (translatePersonZ > 2.5f && translatePersonZ <= 15.4f && animationRunning == true)
            {
                personOnEscalator = true;
                translatePersonY += 0.256f * 7 / obesity;
                translatePersonZ += 0.5f * 7 / obesity;
            }
            else
            {
                translatePersonZ = -10.0f;
                translatePersonY = 0.0f;
                keyEventsEnabled = true;
                animationRunning = false;
            }
        }

        public void MoveEscalator(object sender, EventArgs e)
        {
            if (animationRunning == true)
            {
                if (personOnEscalator)
                    v = 0.1f; //v * 7 / obesity;

                for (int i = 0; i < translateEscalatorX.Length; i++)
                {
                    if (translateEscalatorX[i] < 17.0f)
                    {
                        translateEscalatorX[i] += v;
                        translateEscalatorY[i] += v;
                    }
                    else
                    {
                        translateEscalatorX[i] = 5.0f;
                        translateEscalatorY[i] = 0.0f;
                    }
                }
            }
        }

        /// <summary>
        /// Funkcija koja pokrece animaciju.
        /// </summary>
        public void Start_Animation()
        {
            timer1 = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timer1.Tick += new EventHandler(MoveEscalator);
            timer1.Tick += new EventHandler(MovePerson);
            timer1.Start();
            keyEventsEnabled = false;
            animationRunning = true;
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
