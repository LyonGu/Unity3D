#define OPTIMIZE_TMP
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace TMPro
{
    /// <summary>
    /// Class which contains information about every element contained within the text object.
    /// </summary>
    [Serializable]
    public class TMP_TextInfo
    {
        internal static Vector2 k_InfinityVectorPositive = new Vector2(32767, 32767);
        internal static Vector2 k_InfinityVectorNegative = new Vector2(-32767, -32767);

        public TMP_Text textComponent;

        public int characterCount;
        public int spriteCount;
        public int spaceCount;
        public int wordCount;
        public int linkCount;
        public int lineCount;
        public int pageCount;

        public int materialCount;

        public TMP_CharacterInfo[] characterInfo;
        public TMP_WordInfo[] wordInfo;
        public TMP_LinkInfo[] linkInfo;
        public TMP_LineInfo[] lineInfo;
        public TMP_PageInfo[] pageInfo;
        public TMP_MeshInfo[] meshInfo;

        private TMP_MeshInfo[] m_CachedMeshInfo;



        // Default Constructor

        public TMP_TextInfo()
        {
            #if OPTIMIZE_TMP
                TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);
                characterInfo = TMP_ArrayPool<TMP_CharacterInfo>.Get(8);

                TMP_ArrayPool<TMP_WordInfo>.Release(wordInfo);
                wordInfo = TMP_ArrayPool<TMP_WordInfo>.Get(16);

                TMP_ArrayPool<TMP_LinkInfo>.Release(linkInfo);
                linkInfo = TMP_ArrayPool<TMP_LinkInfo>.Get(0);

                TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                lineInfo = TMP_ArrayPool<TMP_LineInfo>.Get(2);

                TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                pageInfo = TMP_ArrayPool<TMP_PageInfo>.Get(4);

                TMP_ArrayPool<TMP_MeshInfo>.Release(meshInfo);
                meshInfo = TMP_ArrayPool<TMP_MeshInfo>.Get(1);                            
            #else
                characterInfo = new TMP_CharacterInfo[8];
                wordInfo = new TMP_WordInfo[16];
                linkInfo = new TMP_LinkInfo[0];
                lineInfo = new TMP_LineInfo[2];
                pageInfo = new TMP_PageInfo[4];
                meshInfo = new TMP_MeshInfo[1];
            #endif
            
            
        }

        internal TMP_TextInfo(int characterCount)
        {
            #if OPTIMIZE_TMP
                TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);
                characterInfo = TMP_ArrayPool<TMP_CharacterInfo>.Get(characterCount);

                TMP_ArrayPool<TMP_WordInfo>.Release(wordInfo);
                wordInfo = TMP_ArrayPool<TMP_WordInfo>.Get(16);

                TMP_ArrayPool<TMP_LinkInfo>.Release(linkInfo);
                linkInfo = TMP_ArrayPool<TMP_LinkInfo>.Get(0);

                TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                lineInfo = TMP_ArrayPool<TMP_LineInfo>.Get(2);

                TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                pageInfo = TMP_ArrayPool<TMP_PageInfo>.Get(4);

                TMP_ArrayPool<TMP_MeshInfo>.Release(meshInfo);
                meshInfo = TMP_ArrayPool<TMP_MeshInfo>.Get(1);   

            #else
                characterInfo = new TMP_CharacterInfo[characterCount];
                wordInfo = new TMP_WordInfo[16];
                linkInfo = new TMP_LinkInfo[0];
                lineInfo = new TMP_LineInfo[2];
                pageInfo = new TMP_PageInfo[4];
                meshInfo = new TMP_MeshInfo[1];
            #endif
        }

        public TMP_TextInfo(TMP_Text textComponent)
        {
            this.textComponent = textComponent;
            #if OPTIMIZE_TMP
                TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);
                characterInfo = TMP_ArrayPool<TMP_CharacterInfo>.Get(characterCount);

                TMP_ArrayPool<TMP_WordInfo>.Release(wordInfo);
                wordInfo = TMP_ArrayPool<TMP_WordInfo>.Get(16);

                TMP_ArrayPool<TMP_LinkInfo>.Release(linkInfo);
                linkInfo = TMP_ArrayPool<TMP_LinkInfo>.Get(0);

                TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                lineInfo = TMP_ArrayPool<TMP_LineInfo>.Get(2);

                TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                pageInfo = TMP_ArrayPool<TMP_PageInfo>.Get(4);

                TMP_ArrayPool<TMP_MeshInfo>.Release(meshInfo);
                meshInfo = TMP_ArrayPool<TMP_MeshInfo>.Get(1);   

            #else
                characterInfo = new TMP_CharacterInfo[8];
                wordInfo = new TMP_WordInfo[4];
                linkInfo = new TMP_LinkInfo[0];
                lineInfo = new TMP_LineInfo[2];
                pageInfo = new TMP_PageInfo[4];
                meshInfo = new TMP_MeshInfo[1];
            #endif
            
            meshInfo[0].mesh = textComponent.mesh;
            materialCount = 1;
        }


        /// <summary>
        /// Function to clear the counters of the text object.
        /// </summary>
        public void Clear()
        {
            characterCount = 0;
            spaceCount = 0;
            wordCount = 0;
            linkCount = 0;
            lineCount = 0;
            pageCount = 0;
            spriteCount = 0;

            for (int i = 0; i < this.meshInfo.Length; i++)
            {
                this.meshInfo[i].vertexCount = 0;
            }
        }


        /// <summary>
        ///
        /// </summary>
        internal void ClearAllData()
        {
            characterCount = 0;
            spaceCount = 0;
            wordCount = 0;
            linkCount = 0;
            lineCount = 0;
            pageCount = 0;
            spriteCount = 0;

            
            #if OPTIMIZE_TMP
                TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);
                characterInfo = TMP_ArrayPool<TMP_CharacterInfo>.Get(characterCount);

                TMP_ArrayPool<TMP_WordInfo>.Release(wordInfo);
                wordInfo = TMP_ArrayPool<TMP_WordInfo>.Get(16);

                TMP_ArrayPool<TMP_LinkInfo>.Release(linkInfo);
                linkInfo = TMP_ArrayPool<TMP_LinkInfo>.Get(0);

                TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                lineInfo = TMP_ArrayPool<TMP_LineInfo>.Get(2);

                TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                pageInfo = TMP_ArrayPool<TMP_PageInfo>.Get(4);

                TMP_ArrayPool<TMP_MeshInfo>.Release(meshInfo);
                materialCount = 0;
                meshInfo = TMP_ArrayPool<TMP_MeshInfo>.Get(1);
            #else
                this.characterInfo = new TMP_CharacterInfo[4];
                this.wordInfo = new TMP_WordInfo[1];
                this.lineInfo = new TMP_LineInfo[1];
                this.pageInfo = new TMP_PageInfo[1];
                this.linkInfo = new TMP_LinkInfo[0];
                materialCount = 0;
                this.meshInfo = new TMP_MeshInfo[1];
            #endif
            
        }


        /// <summary>
        /// Function to clear the content of the MeshInfo array while preserving the Triangles, Normals and Tangents.
        /// </summary>
        public void ClearMeshInfo(bool updateMesh)
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].Clear(updateMesh);
        }


        /// <summary>
        /// Function to clear the content of all the MeshInfo arrays while preserving their Triangles, Normals and Tangents.
        /// </summary>
        public void ClearAllMeshInfo()
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].Clear(true);
        }


        /// <summary>
        ///
        /// </summary>
        public void ResetVertexLayout(bool isVolumetric)
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].ResizeMeshInfo(0, isVolumetric);
        }


        /// <summary>
        /// Function used to mark unused vertices as degenerate.
        /// </summary>
        /// <param name="materials"></param>
        public void ClearUnusedVertices(MaterialReference[] materials)
        {
            for (int i = 0; i < meshInfo.Length; i++)
            {
                int start = 0; // materials[i].referenceCount * 4;
                meshInfo[i].ClearUnusedVertices(start);
            }
        }


        /// <summary>
        /// Function to clear and initialize the lineInfo array.
        /// </summary>
        public void ClearLineInfo()
        {
            #if OPTIMIZE_TMP
                if (this.lineInfo == null)
                {
                    TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                    this.lineInfo = TMP_ArrayPool<TMP_LineInfo>.Get(2);
                }

            #else
                if (this.lineInfo == null)
                {
                    this.lineInfo = new TMP_LineInfo[2];
                }
            #endif

            int length = this.lineInfo.Length;

            for (int i = 0; i < length; i++)
            {
                this.lineInfo[i].characterCount = 0;
                this.lineInfo[i].spaceCount = 0;
                this.lineInfo[i].wordCount = 0;
                this.lineInfo[i].controlCharacterCount = 0;
                this.lineInfo[i].width = 0;

                this.lineInfo[i].ascender = k_InfinityVectorNegative.x;
                this.lineInfo[i].descender = k_InfinityVectorPositive.x;

                this.lineInfo[i].marginLeft = 0;
                this.lineInfo[i].marginRight = 0;

                this.lineInfo[i].lineExtents.min = k_InfinityVectorPositive;
                this.lineInfo[i].lineExtents.max = k_InfinityVectorNegative;

                this.lineInfo[i].maxAdvance = 0;
                //this.lineInfo[i].maxScale = 0;
            }
        }

        internal void ClearPageInfo()
        {

            #if OPTIMIZE_TMP
                if (this.pageInfo == null)
                {
                    TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                    this.pageInfo = TMP_ArrayPool<TMP_PageInfo>.Get(2);
                }
                        
            #else
                if (this.pageInfo == null)
                   this.pageInfo = new TMP_PageInfo[2];
            #endif


            int length = this.pageInfo.Length;

            for (int i = 0; i < length; i++)
            {
                this.pageInfo[i].firstCharacterIndex = 0;
                this.pageInfo[i].lastCharacterIndex = 0;
                this.pageInfo[i].ascender = -32767;
                this.pageInfo[i].baseLine = 0;
                this.pageInfo[i].descender = 32767;
            }
        }


        /// <summary>
        /// Function to copy the MeshInfo Arrays and their primary vertex data content.
        /// </summary>
        /// <returns>A copy of the MeshInfo[]</returns>
        public TMP_MeshInfo[] CopyMeshInfoVertexData()
        {
            if (m_CachedMeshInfo == null || m_CachedMeshInfo.Length != meshInfo.Length)
            {
                #if OPTIMIZE_TMP
                    TMP_ArrayPool<TMP_MeshInfo>.Release(m_CachedMeshInfo);
                    m_CachedMeshInfo = TMP_ArrayPool<TMP_MeshInfo>.Get(meshInfo.Length);
                #else
                    m_CachedMeshInfo = new TMP_MeshInfo[meshInfo.Length];
                #endif
   

                // Initialize all the vertex data arrays
                for (int i = 0; i < m_CachedMeshInfo.Length; i++)
                {
                    int length = meshInfo[i].vertices.Length;

                    m_CachedMeshInfo[i].vertices = new Vector3[length];
                    m_CachedMeshInfo[i].uvs0 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs2 = new Vector2[length];
                    m_CachedMeshInfo[i].colors32 = new Color32[length];

                    //m_CachedMeshInfo[i].normals = new Vector3[length];
                    //m_CachedMeshInfo[i].tangents = new Vector4[length];
                    //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
                }
            }

            for (int i = 0; i < m_CachedMeshInfo.Length; i++)
            {
                int length = meshInfo[i].vertices.Length;

                if (m_CachedMeshInfo[i].vertices.Length != length)
                {
                    m_CachedMeshInfo[i].vertices = new Vector3[length];
                    m_CachedMeshInfo[i].uvs0 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs2 = new Vector2[length];
                    m_CachedMeshInfo[i].colors32 = new Color32[length];

                    //m_CachedMeshInfo[i].normals = new Vector3[length];
                    //m_CachedMeshInfo[i].tangents = new Vector4[length];
                    //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
                }


                // Only copy the primary vertex data
                Array.Copy(meshInfo[i].vertices, m_CachedMeshInfo[i].vertices, length);
                Array.Copy(meshInfo[i].uvs0, m_CachedMeshInfo[i].uvs0, length);
                Array.Copy(meshInfo[i].uvs2, m_CachedMeshInfo[i].uvs2, length);
                Array.Copy(meshInfo[i].colors32, m_CachedMeshInfo[i].colors32, length);

                //Array.Copy(meshInfo[i].normals, m_CachedMeshInfo[i].normals, length);
                //Array.Copy(meshInfo[i].tangents, m_CachedMeshInfo[i].tangents, length);
                //Array.Copy(meshInfo[i].triangles, m_CachedMeshInfo[i].triangles, meshInfo[i].triangles.Length);
            }

            return m_CachedMeshInfo;
        }



        /// <summary>
        /// Function to resize any of the structure contained in the TMP_TextInfo class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public static void Resize<T> (ref T[] array, int size)
        {
            // Allocated to the next power of two
            int newSize = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size);

            #if OPTIMIZE_TMP
                T[] newArray = TMP_ArrayPool<T>.Get(newSize);
                Array.Copy(array, 0, newArray, 0, array.Length > newSize? newSize : array.Length);
                TMP_ArrayPool<T>.Release(array);
                array = newArray;
            #else
                Array.Resize(ref array, newSize);
            #endif
        }


        /// <summary>
        /// Function to resize any of the structure contained in the TMP_TextInfo class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <param name="isFixedSize"></param>
        public static void Resize<T>(ref T[] array, int size, bool isBlockAllocated)
        {
            if (isBlockAllocated) size = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size);

            if (size == array.Length) return;

            //Debug.Log("Resizing TextInfo from [" + array.Length + "] to [" + size + "]");

        #if OPTIMIZE_TMP
            T[] newArray = TMP_ArrayPool<T>.Get(size);
            Array.Copy(array, 0, newArray, 0, array.Length > size ? size : array.Length);
            TMP_ArrayPool<T>.Release(array);
            array = newArray;
        #else
            Array.Resize(ref array, size);
        #endif
        }

        private void ReleaseLinkInfo()
        {
            int len = linkInfo.Length;
            for (int i = 0; i < len; i++)
            {
                linkInfo[i].Release();
            }
        }
        
        private void ReleaseMeshInfo()
        {
            int len = meshInfo.Length;
            for (int i = 0; i < len; i++)
            {
                meshInfo[i].Release();
            }

            if (m_CachedMeshInfo != null)
            {
                len = m_CachedMeshInfo.Length;
                for (int i = 0; i < len; i++)
                {
                    m_CachedMeshInfo[i].Release();
                }
            }

           
        }

        public void Release()
        {
            #if OPTIMIZE_TMP
                TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);
                TMP_ArrayPool<TMP_WordInfo>.Release(wordInfo);

                ReleaseLinkInfo();
                TMP_ArrayPool<TMP_LinkInfo>.Release(linkInfo);
                TMP_ArrayPool<TMP_LineInfo>.Release(lineInfo);
                TMP_ArrayPool<TMP_PageInfo>.Release(pageInfo);
                ReleaseMeshInfo();
                TMP_ArrayPool<TMP_MeshInfo>.Release(meshInfo);
                TMP_ArrayPool<TMP_MeshInfo>.Release(m_CachedMeshInfo);
            #else

            #endif
        }

    }
}
