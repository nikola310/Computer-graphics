// -----------------------------------------------------------------------
// <file>AssimpScene.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić, Aleksandar Josić</author>
// <summary>Klasa enkapsulira programski kod za ucitavanje modela pomocu na AssimpNet biblioteke i prikazivanje modela uz uslonac na SharpGL biblioteku.</summary>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Assimp;
using System.IO;
using SharpGL.SceneGraph;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL;


namespace AssimpSample
{
    /// <summary>
    /// Klasa enkapsulira programski kod za ucitavanje modela pomocu AssimpNet biblioteke i prikazivanje modela uz uslonac na TaoFramework biblioteku.
    /// </summary>
    public class AssimpScene : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private Assimp.Scene m_scene;

        /// <summary>
        ///	 OpenGL referenca koju dobijamo iz aplikacije
        /// </summary>
        private OpenGL gl;

        /// <summary>
        ///	 Display lista preko koje iscrtavamo model
        /// </summary>
        private DisplayList lista;

        /// <summary>
        ///	 Putanja do foldera u kojem se nalaze podaci o sceni
        /// </summary>
        private String m_scenePath;

        /// <summary>
        ///	 Naziv fajla u kojem se nalaze podaci o sceni
        /// </summary>
        private String m_sceneFileName;

        /// <summary>
        ///	 Identifikator tekstura.
        /// </summary>
        private uint[] m_texIds;

        /// <summary>
        ///	 Mapiranje teksture na njen identifikator.
        /// </summary>
        private Dictionary<TextureSlot, uint> m_texMappings;

        #endregion

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public Assimp.Scene Scene
        {
            get { return m_scene; }
            private set { m_scene = value; }
        }

        #endregion

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase AssimpScene.
        /// </summary>
        /// <param name="scenePath">Putanja do foldera u kojem se nalaze podaci o sceni.</param>
        /// <param name="sceneFileName">Naziv fajla u kojem se nalaze podaci o sceni.</param>
        public AssimpScene(String scenePath, String sceneFileName, OpenGL gl)
        {
            this.m_scenePath = scenePath;
            this.m_sceneFileName = sceneFileName;
            this.m_texMappings = new Dictionary<TextureSlot, uint>();
            this.gl = gl;
        }

        /// <summary>
        ///  Destruktor klase AssimpScene.
        /// </summary>
        ~AssimpScene()
        {
            this.Dispose(false);
        }

        #endregion

        #region Metode

        /// <summary>
        ///  Iscrtavanje scene.
        /// </summary>
        public void Draw()
        {
            lista.Call(gl);
        }

        /// <summary>
        ///  Ucitavanje podataka o sceni iz odgovarajuceg fajla.
        /// </summary>
        public void LoadScene()
        {
            // Instanciranje klase za ucitavanje podataka o sceni.
            AssimpImporter importer = new AssimpImporter();

            // Definisanje callback delegata za belezenje poruka u toku ucitavanja podataka o sceni.
            LogStream logstream = new LogStream(delegate(String msg, String userData)
            {
                Console.WriteLine(msg);
            });
            importer.AttachLogStream(logstream);

            // Ucitavanje podataka o sceni iz odgovarajuceg fajla.
            m_scene = importer.ImportFile(Path.Combine(m_scenePath, m_sceneFileName));

            // Oslobadjanje resursa koriscenih za ucitavanje podataka o sceni.
            importer.Dispose();
        }

        /// <summary>
        ///  Inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize()
        {
            LoadTextures();
            lista = new DisplayList();
            lista.Generate(gl);
            lista.New(gl, DisplayList.DisplayListMode.Compile);
                gl.PushAttrib(OpenGL.GL_ENABLE_BIT);
                gl.PushAttrib(OpenGL.GL_TEXTURE_BIT);
                gl.PushAttrib(OpenGL.GL_POLYGON_BIT);
                gl.PushAttrib(OpenGL.GL_CURRENT_BIT);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_CULL_FACE);
                gl.FrontFace(OpenGL.GL_CCW);
                RenderNode(m_scene.RootNode);
                gl.PopAttrib();
                gl.PopAttrib();
                gl.PopAttrib();
                gl.PopAttrib();
            lista.End(gl);
        }

        /// <summary>
        ///  Rekurzivna metoda zaduzena za iscrtavanje objekata u sceni koji su reprezentovani cvorovima. 
        ///  U zavisnosti od karakteristika objekata podesavaju se odgovarajuce promenjive stanja (GL_LIGHTING, GL_COLOR_MATERIAL, GL_TEXTURE_2D).
        /// </summary>
        /// <param name="node">Cvor koji ce biti iscrtan.</param>
        private void RenderNode(Node node)
        {
            gl.PushMatrix();

            // Primena tranformacija, definisanih za dati cvor.
            float[] matrix = new float[16] { node.Transform.A1, node.Transform.B1, node.Transform.C1, node.Transform.D1, node.Transform.A2, node.Transform.B2, node.Transform.C2, node.Transform.D2, node.Transform.A3, node.Transform.B3, node.Transform.C3, node.Transform.D3, node.Transform.A4, node.Transform.B4, node.Transform.C4, node.Transform.D4 };
            gl.MultMatrix(matrix);

            // Iscrtavanje objekata u sceni koji su reprezentovani datim cvorom.
            if (node.HasMeshes)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    Mesh mesh = m_scene.Meshes[meshIndex];
                    Material material = m_scene.Materials[mesh.MaterialIndex];

                    // Primena komponenti materijala datog objekta.
                    ApplyMaterial(material);

                    // Primena teksture u slucaju da je ista definisana za dati materijal.
                    if (material.GetAllTextures().Length > 0)
                        gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_texMappings[material.GetAllTextures()[0]]);

                    // Podesavanje proracuna osvetljenja za dati objekat.
                    bool hasNormals = mesh.HasNormals;
                    if (hasNormals)
                        gl.Enable(OpenGL.GL_LIGHTING);
                    else
                        gl.Disable(OpenGL.GL_LIGHTING);

                    // Podesavanje color tracking mehanizma za dati objekat.
                    bool hasColors = mesh.HasVertexColors(0);
                    if (hasColors)
                        gl.Enable(OpenGL.GL_COLOR_MATERIAL);
                    else
                        gl.Disable(OpenGL.GL_COLOR_MATERIAL);

                    // Podesavanje rezima mapiranja na teksture.
                    bool hasTexCoords = material.GetAllTextures().Length > 0 && mesh.HasTextureCoords(0);
                    if (hasTexCoords)
                        gl.Enable(OpenGL.GL_TEXTURE_2D);
                    else
                        gl.Disable(OpenGL.GL_TEXTURE_2D);

                    // Iscrtavanje primitiva koji cine dati objekat.
                    // U zavisnosti od broja temena, moguce je iscrtavanje tacaka, linija, trouglova ili poligona.
                    foreach (Assimp.Face face in mesh.Faces)
                    {
                        switch (face.IndexCount)
                        {
                            case 1:
                                gl.Begin(OpenGL.GL_POINTS);
                                break;
                            case 2:
                                gl.Begin(OpenGL.GL_LINES);
                                break;
                            case 3:
                                gl.Begin(OpenGL.GL_TRIANGLES);
                                break;
                            default:
                                gl.Begin(OpenGL.GL_POLYGON);
                                break;
                        }

                        for (int i = 0; i < face.IndexCount; i++)
                        {
                            uint indice = face.Indices[i];

                            // Definisanje boje temena.
                            if (hasColors)
                                gl.Color(mesh.GetVertexColors(0)[indice].R, mesh.GetVertexColors(0)[indice].G, mesh.GetVertexColors(0)[indice].B, mesh.GetVertexColors(0)[indice].A);

                            // Definisanje normale temena.
                            if (hasNormals)
                                gl.Normal(mesh.Normals[indice].X, mesh.Normals[indice].Y, mesh.Normals[indice].Z);

                            // Definisanje koordinata teksture temena.
                            if (hasTexCoords)
                                gl.TexCoord(mesh.GetTextureCoords(0)[indice].X, 1 - mesh.GetTextureCoords(0)[indice].Y);

                            // Definisanje temena primitive.
                            gl.Vertex(mesh.Vertices[indice].X, mesh.Vertices[indice].Y, mesh.Vertices[indice].Z);
                        }
                        gl.End();
                    }
                }
            }

            // Rekurzivno scrtavanje podcvorova datog cvora.
            for (int i = 0; i < node.ChildCount; i++)
            {
                RenderNode(node.Children[i]);
            }
            gl.PopMatrix();
        }

        /// <summary>
        ///  Ucitavanje tekstura.
        /// </summary>
        /// <param name="textureSlot">Informacije o teksturi koja ce biti ucitana.</param>
        private void LoadTextures()
        {
            int texCount = 0;
            foreach (Material material in m_scene.Materials)
            {
                foreach (TextureSlot texSlot in material.GetAllTextures())
                {
                    texCount++;
                }
            }

            m_texIds = new uint[texCount];
            gl.GenTextures(texCount, m_texIds);

            int index = 0;
            foreach (Material material in m_scene.Materials)
            {
                foreach (TextureSlot texSlot in material.GetAllTextures())
                {
                    m_texMappings[texSlot] = m_texIds[index];

                    // Pridruzi teksturu odgovarajucem identifikatoru.
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_texIds[index]);

                    // Formiranje putanje do fajla koji predstavlja teksturu.
                    String fileName = Path.Combine(m_scenePath, texSlot.FilePath.StartsWith("/") ? texSlot.FilePath.Substring(1) : texSlot.FilePath);
                    if (!File.Exists(fileName))
                        throw new ArgumentException();

                    // Ucitavanje teksture iz datog fajla.
                    Bitmap textureBitmap = new Bitmap(fileName);
                    BitmapData textureData = textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA8, textureData.Width, textureData.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, textureData.Scan0);

                    // Podesavanje filtriranja teksture.
                    gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                    gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                    // Podesavanje ponavljanja teksture za dati materijal. 
                    if (texSlot.WrapModeU == TextureWrapMode.Clamp)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
                    if (texSlot.WrapModeV == TextureWrapMode.Clamp)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
                    if (texSlot.WrapModeU == TextureWrapMode.Wrap)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                    if (texSlot.WrapModeV == TextureWrapMode.Wrap)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                    // Oslobadjanje resursa teksture.
                    textureBitmap.UnlockBits(textureData);
                    textureBitmap.Dispose();

                    index++;
                }
            }
        }

        /// <summary>
        ///  Primena razlicitih komponenti datog materijala (ambijentalna, difuzna, spekularna, emisiona, sjaj).
        /// </summary>
        /// <param name="material">Materijal cije ce karakteristike biti primenjene.</param>
        private void ApplyMaterial(Material material)
        {
            // Primena ambijentalne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] ambientColor = material.HasColorAmbient ? new float[] { material.ColorAmbient.R, material.ColorAmbient.G, material.ColorAmbient.B, material.ColorAmbient.A } : new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, ambientColor);

            // Primena difuzne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] diffuseColor = material.HasColorDiffuse ? new float[] { material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B, material.ColorDiffuse.A } : new float[] { 0.8f, 0.8f, 0.8f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, diffuseColor);

            // Primena spekularne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] specularColor = material.HasColorSpecular ? new float[] { material.ColorSpecular.R, material.ColorSpecular.G, material.ColorSpecular.B, material.ColorSpecular.A } : new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, specularColor);


            // Primena emisione komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] emissiveColor = material.HasColorEmissive ? new float[] { material.ColorEmissive.R, material.ColorEmissive.G, material.ColorEmissive.B, material.ColorEmissive.A } : new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_EMISSION, emissiveColor);

            // Primena sjaja materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float shininess = material.HasShininess ? material.Shininess : 1.0f;
            float strength = material.HasShininessStrength ? material.ShininessStrength : 1.0f;
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shininess * strength);
        }

        /// <summary>
        ///  Metoda za oslobadjanje resursa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lista.Delete(gl);
                gl.DeleteTextures(m_texIds.Length, m_texIds);
            }
        }

        #endregion Private metode

        #region IDisposable metode

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
