using UnityEngine;

namespace Terrain
{
    public class MapDisplay : MonoBehaviour
    {
        [SerializeField] private Renderer textureRender;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public void DrawMapTexture(Texture2D texture)
        {
            textureRender.sharedMaterial.mainTexture = texture;
            textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

        public void DrawMesh(MeshData meshData, Texture2D texture)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }
}
