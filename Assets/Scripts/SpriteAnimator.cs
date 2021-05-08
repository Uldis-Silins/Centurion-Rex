using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private SpriteAnimatorData m_animationData;
    [SerializeField]private SpriteAnimatorData.AnimationType m_currentAnimationType = SpriteAnimatorData.AnimationType.IdleLeft;

    private SpriteAnimatorData.SpriteAnimationFrames m_currentFrames;
    private int m_curFrameIndex;
    private float m_timer;

    private bool m_isPlaying;
    private bool m_loop;

    public bool IsPlaying { get { return m_isPlaying; } }
    public int CurrentFrame { get { return m_curFrameIndex; } }
    public SpriteAnimatorData.AnimationType CurrentAnimationType { get { return m_currentAnimationType; } }
    public float CurrentAnimationTotalTime { get { return m_currentFrames == null ? 0f : m_currentFrames.frameCount * m_currentFrames.frameTime; } }

    private void Start()
    {
        m_curFrameIndex = 0;
        m_isPlaying = false;
        m_loop = false;
    }

    private void OnValidate()
    {
        m_curFrameIndex = 0;

        if(m_animationData != null && m_animationData.animations != null)
        {
            m_spriteRenderer.sprite = m_animationData.GetAnimationFrames(m_currentAnimationType).frameSprites[m_curFrameIndex];
        }
    }

    private void Update()
    {
        if(m_isPlaying && m_currentFrames != null)
        {
            m_timer += Time.deltaTime;

            if(m_timer >= m_currentFrames.frameTime)
            {
                m_timer = 0f;
                m_curFrameIndex = (m_curFrameIndex + 1) % m_currentFrames.frameCount;

                if (!m_loop && m_curFrameIndex == m_currentFrames.frameCount - 1)
                {
                    m_isPlaying = false;
                }
                else
                {
                    m_spriteRenderer.sprite = m_currentFrames.frameSprites[m_curFrameIndex];
                }
            }
        }
    }

    public void PlayAnimation(SpriteAnimatorData.AnimationType type, bool loop = true, bool reset = false)
    {
        m_currentAnimationType = type;
        m_currentFrames = m_animationData.GetAnimationFrames(type);
        m_loop = loop;
        m_timer = reset ? 0.0f : m_timer;
        m_isPlaying = true;

        m_spriteRenderer.flipX = m_currentFrames.flipX;
        m_spriteRenderer.flipY = m_currentFrames.flipY;
    }

    public void StopAnimation(bool resetToFirstFrame = true)
    {
        m_isPlaying = false;

        if (resetToFirstFrame && m_currentFrames != null)
        {
            m_spriteRenderer.sprite = m_currentFrames.frameSprites[0];
        }
    }

    public void PauseAnimation(bool isPaused)
    {
        if (m_currentFrames != null)
        {
            m_isPlaying = !isPaused;
        }
    }

#if UNITY_EDITOR
    [ExecuteInEditMode]
    public void PlayNextFrame()
    {
        var curAnim = m_animationData.GetAnimationFrames(m_currentAnimationType);

        if(++m_curFrameIndex >= curAnim.frameCount)
        {
            m_curFrameIndex = 0;
        }

        m_spriteRenderer.sprite = curAnim.frameSprites[m_curFrameIndex];
    }

    [ExecuteInEditMode]
    public void PlayPrevFrame()
    {
        var curAnim = m_animationData.GetAnimationFrames(m_currentAnimationType);

        if (--m_curFrameIndex < 0)
        {
            m_curFrameIndex = curAnim.frameCount - 1;
        }

        m_spriteRenderer.sprite = curAnim.frameSprites[m_curFrameIndex];
    }
#endif
}
