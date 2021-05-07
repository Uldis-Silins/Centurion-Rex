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
    public SelectableManager selectableManager;
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapBackgroundImage;
    public Renderer levelBoundsRenderer;

    public Canvas minimapCanvas;

    [Range(0, 16)] public int unitMarkerSize = 2;
    [Range(0, 32)] public int buildingMarkerSize = 6;
    public Color playerMarkerColor = Color.blue;
    public Color enemyMarkerColor = Color.red;

    private RenderTexture m_renderTexture;
    private Texture2D m_targetTexture;

    private void Start()
    {
        m_renderTexture = new RenderTexture(((int)levelBoundsRenderer.bounds.size.x) * 2, ((int)levelBoundsRenderer.bounds.size.y) * 2, 24);
        m_targetTexture = new Texture2D(m_renderTexture.width, m_renderTexture.height);
        m_targetTexture.name = "Minimap Render Texture";
        minimapCamera.targetTexture = m_renderTexture;
        minimapImage.texture = m_targetTexture;

        FitCameraToLevelBounds();
        FitMinimapImage();
    }

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;

        RenderTexture.active = m_renderTexture;
        m_targetTexture.ReadPixels(new Rect(0, 0, m_renderTexture.width, m_renderTexture.height), 0, 0);

        for (int i = 0; i < playerController.OwnedUnits.Count; i++)
        {
            DrawMarker(playerController.OwnedUnits[i].transform.position, unitMarkerSize, playerMarkerColor);
        }

        for (int i = 0; i < playerController.ownedBuildings.Count; i++)
        {
            DrawMarker(playerController.ownedBuildings[i].gameObject.transform.position, buildingMarkerSize, playerMarkerColor);
        }

        foreach (var unit in visibilityManager.VisibleUnits)
        {
            DrawMarker(unit.transform.position, unitMarkerSize, enemyMarkerColor);
        }

        foreach (var building in visibilityManager.VisibleBuildings)
        {
            if (building.gameObject != null)
            {
                DrawMarker(building.gameObject.transform.position, buildingMarkerSize, enemyMarkerColor);
            }
        }

        m_targetTexture.Apply();
        RenderTexture.active = null;

        if (UI_Helpers.IsPointerOverCanvasElement(minimapCanvas))
        {
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapImage.rectTransform, Input.mousePosition, null, out localPoint) && minimapImage.rectTransform.rect.Contains(localPoint))
            {
                localPoint += new Vector2(minimapImage.rectTransform.rect.width / 2.0f, minimapImage.rectTransform.rect.height / 2.0f);
                Vector2 viewPos = new Vector2(localPoint.x / minimapImage.rectTransform.rect.width, localPoint.y / minimapImage.rectTransform.rect.height);

                float mainCamZPos = mainCamera.transform.position.z;
                Vector3 boundsMin = levelBoundsRenderer.bounds.min;
                boundsMin.z = 0f;

                if (Input.GetMouseButtonDown(0))
                {
                    mainCamera.SetPosition(boundsMin + new Vector3(levelBoundsRenderer.bounds.size.x * viewPos.x, levelBoundsRenderer.bounds.size.y * viewPos.y, mainCamZPos));
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    List<ISelecteble> curSelectedUnits = new List<ISelecteble>(selectableManager.GetCurrentSelected());

                    if (curSelectedUnits.Count > 0)
                    {
                        Vector3 hitPos = boundsMin + new Vector3(levelBoundsRenderer.bounds.size.x * viewPos.x, levelBoundsRenderer.bounds.size.y * viewPos.y, mainCamZPos);

                        List<Vector2> formationPositions = Formations.GetPositionListCircle(hitPos, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });

                        for (int i = 0; i < curSelectedUnits.Count; i++)
                        {
                            Unit_Base unit = curSelectedUnits[i] as Unit_Base;

                            if (unit != null)
                            {
                                unit.SetAttackTarget(null);

                                unit.SetMoveTarget(Formations.GetAdjustedPosition(formationPositions[i], unit, unit.circleCollider.radius * 1.5f));
                                unit.SetState(Unit_Base.UnitStateType.Move);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(levelBoundsRenderer.bounds.center, levelBoundsRenderer.bounds.size);
        Gizmos.color = prevColor;
    }

    private void OnValidate()
    {
        if(levelBoundsRenderer != null && minimapCamera != null)
        {
            FitCameraToLevelBounds();
        }

        if(levelBoundsRenderer != null && minimapImage != null && minimapBackgroundImage != null)
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

        minimapCamera.aspect = levelBoundsRenderer.bounds.size.x / levelBoundsRenderer.bounds.size.y;
        float width = levelBoundsRenderer.bounds.extents.y;
        minimapCamera.orthographicSize = width * minimapCamera.aspect;
        minimapCamera.transform.position = levelBoundsRenderer.bounds.center - Vector3.forward * 70.0f;
    }

    [ExecuteInEditMode]
    private void FitMinimapImage()
    {
        float aspect = levelBoundsRenderer.bounds.size.x / levelBoundsRenderer.bounds.size.y;

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
