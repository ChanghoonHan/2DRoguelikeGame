using UnityEngine;

public class HeroAnimationEventManager : MonoBehaviour
{
    public Attacker attackerScript;

    public void Awake()
    {
        
    }

    public void StartAttack(string option)
    {
        string[] tempStr = option.Split(',');
        if (tempStr[0] == "S")
        {
            attackerScript.StrongAttack(int.Parse(tempStr[1]));
        }
        else
        {
            attackerScript.WeakAttack(int.Parse(tempStr[1]));
        }
    }

    public void EndAttack()
    {
        HeroController.S.SetAttackEnd();
    }

    public void EndRoll()
    {
        HeroController.S.SetRollEnd();
    }

    public void EndDie()
    {
        HeroController.S.DieEnd();
    }
}
