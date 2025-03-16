using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] Texture2DHolder textureHolder;
    [SerializeField] TextureType textureType;
    TextureType displayedType;

    HashSet<Vector2Int> edgePixels;

    Dictionary<TextureType, Texture2D> textures;

    bool isInit = false;

    public void Init
    (
        HashSet<Vector2Int> _edgePixels, 
        Texture2D landTexture, 
        Texture2D noiseTexture, 
        Texture2D heightTexture, 
        Texture2D marineTexture, 
        Texture2D heatTexture, 
        Texture2D humidityTexture, 
        Texture2D biomeTexture
    )
    {
        edgePixels = _edgePixels;

        textures = new Dictionary<TextureType, Texture2D>();
        textures.Add(TextureType.LandTexture, landTexture);
        textures.Add(TextureType.NoiseTexture, noiseTexture);
        textures.Add(TextureType.HeightTexture, heightTexture);
        textures.Add(TextureType.MarineTexture, marineTexture);
        textures.Add(TextureType.HeatTexture, heatTexture);
        textures.Add(TextureType.HumidityTexture, humidityTexture);
        textures.Add(TextureType.BiomeTexture, biomeTexture);

        displayedType = TextureType.BiomeTexture;
        textureType = TextureType.BiomeTexture;

        Apply(textures[textureType], edgePixels, textures[TextureType.MarineTexture]);

        isInit = true;
    }

    void Update()
    {
        if (!isInit || textureType == displayedType) return;
        Apply(textures[textureType], edgePixels, textures[TextureType.MarineTexture]);
        displayedType = textureType;
    }

    void Apply(Texture2D texture, HashSet<Vector2Int> edgePixels = null, Texture2D edgeMask = null)
    {
        texture.Apply();
        var textureToRender = new Texture2D(texture.width, texture.height, texture.format, false);
        textureToRender.SetPixelData(texture.GetRawTextureData(), 0);
        if (edgePixels != null)
            foreach (var pixel in edgePixels)
                textureToRender.SetPixel(pixel.x, pixel.y, GetEdgeColor(pixel));
        textureToRender.Apply();
        textureHolder.Push(textureToRender);

        Color GetEdgeColor(Vector2Int edgePixel)
        {
            var edgeColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
            if (edgeMask != null && edgeMask.GetPixel(edgePixel.x, edgePixel.y) != Color.black)
                edgeColor = new Color(150f / 255f, 150f / 255f, 150f / 255f);
            return edgeColor;
        }
    }

    public enum TextureType
    {
        LandTexture,
        NoiseTexture,
        HeightTexture,
        MarineTexture,
        HeatTexture,
        HumidityTexture,
        BiomeTexture
    }
}
