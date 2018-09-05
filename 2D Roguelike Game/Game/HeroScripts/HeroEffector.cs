using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEffector : MonoBehaviour
{
    public enum EffectType
    {
        None,
        Heal,
    }

    [Header("Set in Inspector")]
    public Animator effectAnimator;

    private void Awake()
    {
        effectAnimator.CrossFade("NoneEffect", 0);
    }

    public void PlayEffect(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.None:
                effectAnimator.CrossFade("NoneEffect", 0);
                break;
            case EffectType.Heal:
                effectAnimator.Play("HealEffect", -1, 0f);
                break;
            default:
                break;
        }
    }

    public void EndEffect()
    {
        effectAnimator.CrossFade("NoneEffect", 0);
    }
}
