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
        //transform.localScale = new Vector3(transform.localScale.z * ScaleX(), transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        m_spriteRenderer.sortingOrder = (int)transform.position.z * -10;
    }

    private float ScaleX()
    {
        return Mathf.Atan(Mathf.Sin(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad));
    }
}
