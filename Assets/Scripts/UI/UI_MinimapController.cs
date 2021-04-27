using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class UI_MinimapController : MonoBehaviour
{
    public CameraController mainCamera;
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapBackgroundImage;
    public TilemapRenderer levelTilemapRenderer;

    private void Start()
    {
        FitCameraToLevelBounds();
        FitMinimapImage();
    }

    private void Update()
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
    }

    private void OnDrawGizmos()
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
}
