// -----------------------------------------------------------------------
// <file>Lighting.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2012.</copyright>
// <author>Srdjan Mihic</author>
// <summary>Klasa za pomoc prilikom rada sa osvetljenjem.</summary>
// -----------------------------------------------------------------------
using System;

namespace AssimpSample
{
    class LightingUtilities
    {
        #region Staticke metode

        /// <summary>
        ///	 Metoda izracunava normale na nivou poligona.
        /// </summary>
        /// <param name="vertices">niz temena za koje odredjujemo normalu.</param>
        public static float[,] ComputeFaceNormals(float[] vertices)
        {
            float[,] normals = new float[vertices.Length / 3, 3]; // Pretpostavimo da radimo sa trouglovima (GL_TRIANGLES)
            float[] temp;
            int j = 0;

            for (int i = 0; i < vertices.Length - 9; i += 9)
            {
                temp = FindFaceNormal(vertices[i], vertices[i + 1], vertices[i + 2],
                                      vertices[i + 3], vertices[i + 4], vertices[i + 5],
                                      vertices[i + 6], vertices[i + 7], vertices[i + 8]);

                normals[j, 0] = temp[0];
                normals[j, 1] = temp[1];
                normals[j++, 2] = temp[2];
            }

            return normals;
        }


        /// <summary>
        ///	 Metoda izracunava normale na nivou temena.
        /// </summary>
        /// <param name="vertices">niz temena za koje odredjujemo normalu.</param>
        public static float[] ComputeVertexNormals(float[] vertices)
        {
            float[] normals = new float[vertices.Length]; // Pretpostavimo da radimo sa trouglovima (GL_TRIANGLES)
            float[,] faceNormals = ComputeFaceNormals(vertices);
            float[] temp;

            for (int i = 0; i < vertices.Length - 3; i += 3)
            {
                temp = FindVertexNormal(vertices[i], vertices[i + 1], vertices[i + 2],
                                        vertices, faceNormals);

                normals[i] = temp[0];
                normals[i + 1] = temp[1];
                normals[i + 2] = temp[2];
            }

            return normals;
        }

        /// <summary>
        ///	 Metoda izracunava normalu za teme.
        /// </summary>
        /// <param name="x">X koordinata temena.</param>
        /// <param name="y">Y koordinata temena.</param>
        /// <param name="z">Z koordinata temena.</param>
        /// <param name="vertices">niz temena za koje odredjujemo normalu.</param>
        /// <param name="normals">niz normala na nivou poligona.</param>
        private static float[] FindVertexNormal(float x, float y, float z,
                                                float[] vertices, float[,] normals)
        {
            float[] vertexNormal = new float[3];
            int[] adjacentFaces = new int[normals.Length]; // Maksimalno svi poligoni dele ovo teme

            // Odredi sve poligone koji dele ovo teme
            int adjacentFaceCount = 0;
            for (int i = 0; i < vertices.Length - 9; i += 9)
                if ((vertices[i] == x && vertices[i + 1] == y && vertices[i + 2] == z) ||
                    (vertices[i + 3] == x && vertices[i + 4] == y && vertices[i + 5] == z) ||
                    (vertices[i + 6] == x && vertices[i + 7] == y && vertices[i + 8] == z))
                    adjacentFaces[adjacentFaceCount++] = i / 9;

            // Odredi prosek svih normala susednih poligona koji dele ovo teme
            vertexNormal[0] = vertexNormal[1] = vertexNormal[2] = 0.0f;

            for (int i = 0; i < adjacentFaceCount; ++i)
            {
                vertexNormal[0] += normals[adjacentFaces[i], 0];
                vertexNormal[1] += normals[adjacentFaces[i], 1];
                vertexNormal[2] += normals[adjacentFaces[i], 2];
            }

            vertexNormal[0] /= adjacentFaceCount;
            vertexNormal[1] /= adjacentFaceCount;
            vertexNormal[2] /= adjacentFaceCount;

            // Duzina vektora normale
            float len = (float)(Math.Sqrt((vertexNormal[0] * vertexNormal[0]) + (vertexNormal[1] * vertexNormal[1]) + (vertexNormal[2] * vertexNormal[2])));

            // Izbegava se deljenje sa nulom
            if (len == 0.0f)
            {
                len = 1.0f;
            }

            // Normalizacija vektora normale
            vertexNormal[0] /= len;
            vertexNormal[1] /= len;
            vertexNormal[2] /= len;

            return vertexNormal;
        }

        /// <summary>
        ///	 Metoda izracunava normalu za poligon.
        /// </summary>
        /// <param name="x1">X koordinata prvog temena.</param>
        /// <param name="y1">Y koordinata prvog temena.</param>
        /// <param name="z1">Z koordinata prvog temena.</param>
        /// <param name="x2">X koordinata drugog temena.</param>
        /// <param name="y2">Y koordinata drugog temena.</param>
        /// <param name="z2">Z koordinata drugog temena.</param>
        /// <param name="x3">X koordinata treceg temena.</param>
        /// <param name="y3">Y koordinata treceg temena.</param>
        /// <param name="z3">Z koordinata treceg temena.</param>
        public static float[] FindFaceNormal(float x1, float y1, float z1,
                                              float x2, float y2, float z2,
                                              float x3, float y3, float z3)
        {
            float[] normal = new float[3];

            // Racunanje normale na ravan odredjenu sa tri tacke definisane u CCW smeru
            normal[0] = (y1 - y2) * (z2 - z3) - (y2 - y3) * (z1 - z2);
            normal[1] = (x2 - x3) * (z1 - z2) - (x1 - x2) * (z2 - z3);
            normal[2] = (x1 - x2) * (y2 - y3) - (x2 - x3) * (y1 - y2);

            // Duzina vektora normale
            float len = (float)(Math.Sqrt((normal[0] * normal[0]) + (normal[1] * normal[1]) + (normal[2] * normal[2])));

            // Izbegava se deljenje sa nulom
            if (len == 0.0f)
            {
                len = 1.0f;
            }

            // Normalizacija vektora normale
            normal[0] /= len;
            normal[1] /= len;
            normal[2] /= len;

            return normal;
        }

        #endregion Staticke metode
    }
}
