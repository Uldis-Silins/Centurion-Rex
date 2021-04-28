using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class UI_MinimapController : MonoBehaviour
{
    public CameraController mainCamera;
    public Player_Controller playerController;
    public VisibilityManager visibilityManager;
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapBackgroundImage;
    public TilemapRenderer levelTilemapRenderer;

    public Canvas minimapCanvas;

    [Range(0, 16)] public int unitMarkerSize = 2;
    [Range(0, 32)] public int buildingMarkerSize = 6;
    public Color playerMarkerColor = Color.blue;
    public Color enemyMarkerColor = Color.red;

    private RenderTexture m_renderTexture;
    private Texture2D m_targetTexture;

    private void Start()
    {
        m_renderTexture = new RenderTexture(((int)levelTilemapRenderer.bounds.size.x) * 2, ((int)levelTilemapRenderer.bounds.size.y) * 2, 24);
        m_targetTexture = new Texture2D(m_renderTexture.width, m_renderTexture.height);
        m_targetTexture.name = "Minimap Render Texture";
        minimapCamera.targetTexture = m_renderTexture;
        minimapImage.texture = m_targetTexture;

        FitCameraToLevelBounds();
        FitMinimapImage();
    }

    private void Update()
    {
        RenderTexture.active = m_renderTexture;
        m_targetTexture.ReadPixels(new Rect(0, 0, m_renderTexture.width, m_renderTexture.height), 0, 0);

        for (int i = 0; i < playerController.OwnedUnits.Count; i++)
        {
            DrawMarker(playerController.OwnedUnits[i].transform.position, unitMarkerSize, playerMarkerColor);
        }

        for (int i = 0; i < playerController.ownedBuildings.Count; i++)
        {
            DrawMarker(playerController.ownedBuildings[i].selectable.transform.position, buildingMarkerSize, playerMarkerColor);
        }

        foreach (var unit in visibilityManager.VisibleUnits)
        {
            DrawMarker(unit.transform.position, unitMarkerSize, enemyMarkerColor);
        }

        foreach (var building in visibilityManager.VisibleBuildings)
        {
            DrawMarker(building.transform.position, buildingMarkerSize, enemyMarkerColor);
        }

        m_targetTexture.Apply();
        RenderTexture.active = null;

        if (UI_Helpers.IsPointerOverCanvasElement(minimapCanvas))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapImage.rectTransform, Input.mousePosition, null, out localPoint) && minimapImage.rectTransform.rect.Contains(localPoint))
                {
                    localPoint += new Vector2(minimapImage.rectTransform.rect.width / 2.0f, minimapImage.rectTransform.rect.height / 2.0f);
                    Vector2 viewPos = new Vector2(localPoint.x / minimapImage.rectTransform.rect.width, localPoint.y / minimapImage.rectTransform.rect.height);

                    float mainCamZPos = mainCamera.transform.position.z;
                    Vector3 boundsMin = levelTilemapRenderer.bounds.min;
                    boundsMin.z = 0f;

                    mainCamera.SetPosition(boundsMin + new Vector3(levelTilemapRenderer.bounds.size.x * viewPos.x, levelTilemapRenderer.bounds.size.y * viewPos.y, mainCamZPos));
                }
            }
            else if(Input.GetMouseButtonUp(1))
            {
                // TODO: Move selected units
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(levelTilemapRenderer.bounds.center, levelTilemapRenderer.bounds.size);
        Gizmos.color = prevColor;
    }

    private void OnValidate()
    {
        if(levelTilemapRenderer != null && minimapCamera != null)
        {
            FitCameraToLevelBounds();
        }

        if(levelTilemapRenderer != null && minimapImage != null && minimapBackgroundImage != null)
        {
            FitMinimapImage();
        }
    }

    [ExecuteInEditMode]
    private void FitCameraToLevelBounds()
    {
        if(!minimapCamera.orthographic)
        {
            Debug.LogWarning("Minimap camera is not orthographic");
            minimapCamera.orthographic = true;
        }

        minimapCamera.aspect = levelTilemapRenderer.bounds.size.x / levelTilemapRenderer.bounds.size.y;
        float width = levelTilemapRenderer.bounds.extents.y;
        minimapCamera.orthographicSize = width * minimapCamera.aspect;
        minimapCamera.transform.position = levelTilemapRenderer.bounds.center - Vector3.forward * 70.0f;
    }

    [ExecuteInEditMode]
    private void FitMinimapImage()
    {
        float aspect = levelTilemapRenderer.bounds.size.x / levelTilemapRenderer.bounds.size.y;

        minimapImage.rectTransform.sizeDelta = new Vector2(minimapBackgroundImage.rect.height * aspect, minimapBackgroundImage.rect.height);
    }

    private void DrawMarker(Vector3 worldPosition, int markerSize, Color color)
    {
        Vector3 viewPos = minimapCamera.WorldToViewportPoint(worldPosition);

        Vector2Int markerPos = new Vector2Int((int)(m_targetTexture.width * viewPos.x), (int)(m_targetTexture.height * viewPos.y));

        int startPosX = markerPos.x - markerSize / 2;
        int endPosX = markerPos.x + markerSize / 2;

        startPosX = startPosX < 0 ? 0 : startPosX;
        endPosX = endPosX >= m_renderTexture.width ? m_renderTexture.width - 1 : endPosX;

        for (int x = startPosX; x <= endPosX; x++)
        {
            int startPosY = markerPos.y - markerSize / 2;
            int endPosY = markerPos.y + markerSize / 2;

            startPosY = startPosY < 0 ? 0 : startPosY;
            endPosY = endPosY >= m_renderTexture.height ? m_renderTexture.height - 1 : endPosY;

            for (int y = startPosY; y <= endPosY; y++)
            {
                m_targetTexture.SetPixel(x, y, color);
            }
        }
    }
}
