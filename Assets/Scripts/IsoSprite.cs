using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer)), ExecuteInEditMode]
public class IsoSprite : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        //transform.localScale += Scale();
    }

    private void Update()
    {
        m_spriteRenderer.sortingOrder = (int)transform.position.z * -10;
    }

    private Vector3 Scale()
    {
        return new Vector3(Mathf.Atan(Mathf.Sin(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad)),
            Mathf.Atan(Mathf.Sin(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad)),
            transform.localScale.z);
    }
}
