// -----------------------------------------------------------------------
// <file>AssimpScene.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa enkapsulira programski kod za ucitavanje modela pomocu na AssimpNet biblioteke i prikazivanje modela uz uslonac na SharpGL biblioteku.</summary>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
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

        /////<summary>
        ///// Generator slucajnih brojeva, koji sluzi za generisanje boje poligona
        /////</summary>
        //private Random m_random;

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

            // Ucitavanje podataka o sceni iz odgovarajuceg fajla
                m_scene = importer.ImportFile(Path.Combine(m_scenePath, m_sceneFileName));

            // Oslobadjanje resursa koriscenih za ucitavanje podataka o sceni.
            importer.Dispose();
        }

        /// <summary>
        ///  Inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize()
        { 
            lista = new DisplayList();
            lista.Generate(gl);
            lista.New(gl, DisplayList.DisplayListMode.Compile);
            RenderNode(m_scene.RootNode);
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

                    bool hasColors = mesh.HasVertexColors(0);
                    uint brojPoli = mesh.Faces[0].IndexCount;

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
                            uint vertexIndex = face.Indices[i];

                            // Definisanje boje temena.
                            if (hasColors)
                                gl.Color(mesh.GetVertexColors(0)[vertexIndex].R, mesh.GetVertexColors(0)[vertexIndex].G, mesh.GetVertexColors(0)[vertexIndex].B, mesh.GetVertexColors(0)[vertexIndex].A);
                            else
                            {
                                // Permutacija boje poligona u zavisnosti od parnosti indeksa
                                if (vertexIndex % 2 == 0)
                                    gl.Color(0.3f, 0.3f, 0.3f);
                                else
                                    gl.Color(0.4f, 0.4f, 0.4f);
                            }
                            // Definisanje temena primitive.
                            gl.Vertex(mesh.Vertices[vertexIndex].X, mesh.Vertices[vertexIndex].Y, mesh.Vertices[vertexIndex].Z);
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
        ///  Metoda za oslobadjanje resursa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lista.Delete(gl);
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
