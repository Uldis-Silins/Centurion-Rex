using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteAnimatorData", menuName = "Units/Animation")]
public class SpriteAnimatorData : ScriptableObject
{
    public enum AnimationType 
    { 
        None, 
        IdleLeft,
        IdleRight,
        WalkUp, 
        WalkDown, 
        WalkLeft, 
        WalkRight, 
        WalkLeftUp, 
        WalkLeftDown, 
        WalkRightUp, 
        WalkRightDown, 
        AttackLeft, 
        AttackRight, 
        DiePrimary, 
        DieSecondary 
    }

    [System.Serializable]
    public class SpriteAnimationFrames
    {
        public Sprite[] frameSprites;
        public float frameTime;

        public bool flipX;
        public bool flipY;

        [HideInInspector]
        public int frameCount;
    }

    [System.Serializable]
    public class SpriteAnimation
    {
        public AnimationType type;
        public SpriteAnimationFrames[] frames;
        public float playbackSpeedModifier = 1.0f;
    }

    public SpriteAnimation[] animations;

    private void OnValidate()
    {
        for (int i = 0; i < animations.Length; i++)
        {
            if(animations[i] != null && animations[i].frames != null)
            {
                for (int j = 0; j < animations[i].frames.Length; j++)
                {
                    if(animations[i].frames[j].frameSprites != null)
                    {
                        animations[i].frames[j].frameCount = animations[i].frames[j].frameSprites.Length;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get frame sequence of animation type.
    /// </summary>
    /// <param name="animationIndex">Sequence index; -1 for random</param>
    public SpriteAnimationFrames GetAnimationFrames(AnimationType type, int animationIndex = -1)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            if(animations[i].type == type)
            {
                if(animationIndex == -1)
                {
                    return animations[i].frames[Random.Range(0, animations[i].frames.Length)];
                }
                else if(animationIndex >= 0 && animationIndex < animations[i].frames.Length)
                {
                    return animations[i].frames[animationIndex];
                }

                throw new System.IndexOutOfRangeException("animationIndex " + animationIndex + " for " + type + " out of range.");
            }
        }

        throw new System.Exception(string.Format("Animation {0} animation index {1} not found.", type, animationIndex));
    }
}