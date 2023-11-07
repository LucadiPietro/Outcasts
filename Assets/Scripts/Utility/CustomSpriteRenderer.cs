using UnityEngine;

public class CustomSpriteRenderer : MonoBehaviour
{
    public Transform modelTransform; // the transform of the 3D model
    public Camera renderCamera; // the camera to use for rendering
    public int spriteWidth = 256; // the width of the sprite
    public int spriteHeight = 256; // the height of the sprite

    private RenderTexture renderTexture;
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer chSprite;

    void Start()
    {
        // create a render texture to render the model to
        renderTexture = new RenderTexture(spriteWidth, spriteHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        // set the camera to use the render texture
        renderCamera.targetTexture = renderTexture;

        // create a sprite renderer component to render the sprite
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }

    void Update()
    {
        // render the model to the render texture
        renderCamera.Render();

        // create a new texture for the sprite and copy the render texture to it
        Texture2D texture = new Texture2D(spriteWidth, spriteHeight, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, spriteWidth, spriteHeight), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // set the texture to the sprite renderer
        chSprite.sprite = Sprite.Create(texture, new Rect(0, 0, spriteWidth, spriteHeight), Vector2.one * 0.5f);
    }
}