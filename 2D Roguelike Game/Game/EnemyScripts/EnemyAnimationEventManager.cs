using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEventManager : MonoBehaviour {
    public Attacker attackerScript;
    public MovingEnemy enemyScript;

    public void StartAttack(string option = "")
    {
        attackerScript.OffAttackerColl();
        if (option == "")
        {
            attackerScript.WeakAttack(0);
            return;
        }

        int tempAttackOption = int.Parse(option);
        attackerScript.WeakAttack(tempAttackOption);
    }

    public void EndAttack()
    {
        enemyScript.SetAttackEnd();
    }

    public void DestroyEnemy()
    {
        enemyScript.DestroyEnemy();
    }
}
