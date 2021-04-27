using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsoSprite : MonoBehaviour
{
    [SerializeField] private int m_defaultSortingOrder = 5000;
    [SerializeField] private int offset = 0;
    [SerializeField] private float m_tickRate = 0.2f;
    [SerializeField] private bool updateSorting = true;
    private Renderer m_renderer;

    private float m_timer;

    private void Awake()
    {
        m_renderer = GetComponent<Renderer>();
        //transform.localScale = new Vector3(transform.localScale.z * ScaleX(), transform.localScale.y, transform.localScale.z);
    }

    private void LateUpdate()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0f)
        {
            m_timer = m_tickRate;
            m_renderer.sortingOrder = (int)(m_defaultSortingOrder - transform.position.y - offset);

            if (!updateSorting)
            {
                this.enabled = false;
            }
        }
    }

    private float ScaleX()
    {
        return Mathf.Atan(Mathf.Sin(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad));
    }
}
