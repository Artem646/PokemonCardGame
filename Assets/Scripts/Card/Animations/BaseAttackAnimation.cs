using System;
using System.Collections;
using UnityEngine;

public abstract class BaseAttackAnimation
{
    public abstract IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender,
        AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback
    );
}