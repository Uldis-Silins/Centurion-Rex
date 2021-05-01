using System.Collections;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public SpriteAnimator anim;
    public SpriteAnimatorData.AnimationType animType;

    // HACK: skip first frame for proper init order
    private IEnumerator Start()
    {
        yield return null;
        anim.PlayAnimation(animType, false, true);
    }
}