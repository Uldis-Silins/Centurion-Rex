using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class CameraScaler : MonoBehaviour
{
    [SerializeField] private float m_pixelsToUnits = 100.0f;

    private Camera m_mainCam;

    private void Awake()
    {
        m_mainCam = Camera.main;
    }

    private void Update()
    {
        m_mainCam.orthographicSize = Screen.height / m_pixelsToUnits / 2;
    }
}