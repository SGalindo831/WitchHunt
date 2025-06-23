using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private GameObject crosshairPrefab;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float crosshairSize = 20f;
    
    private Image crosshairImage;
    private Canvas uiCanvas;

    private void Start()
    {
        CreateCrosshairUI();
    }

    private void CreateCrosshairUI()
    {
        // Create or find Canvas
        uiCanvas = FindFirstObjectByType<Canvas>();
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create crosshair GameObject
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(uiCanvas.transform);

        // Add Image component
        crosshairImage = crosshairObj.AddComponent<Image>();
        
        // Create a simple crosshair texture
        crosshairImage.sprite = CreateCrosshairSprite();
        crosshairImage.color = normalColor;

        // Set position to center of screen
        RectTransform rectTransform = crosshairObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
    }

    private Sprite CreateCrosshairSprite()
    {
        // Create a simple crosshair texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Clear texture
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Draw crosshair lines
        int center = size / 2;
        int thickness = 2;
        int lineLength = 8;

        // Horizontal line
        for (int x = center - lineLength; x <= center + lineLength; x++)
        {
            for (int y = center - thickness / 2; y <= center + thickness / 2; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        // Vertical line
        for (int y = center - lineLength; y <= center + lineLength; y++)
        {
            for (int x = center - thickness / 2; x <= center + thickness / 2; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        // Create center dot (optional)
        pixels[center * size + center] = Color.clear; // Remove center pixel for hollow crosshair

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // Call these methods from your interaction system later
    public void SetCrosshairHighlight(bool highlighted)
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = highlighted ? highlightColor : normalColor;
        }
    }

    public void SetCrosshairSize(float newSize)
    {
        if (crosshairImage != null)
        {
            crosshairImage.rectTransform.sizeDelta = new Vector2(newSize, newSize);
        }
    }
}