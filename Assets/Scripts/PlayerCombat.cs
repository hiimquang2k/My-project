using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public DamageSystem damageSystem;
    public float attackDamage = 10f;

    // Method to perform a melee attack
    public void MeleeAttack(HealthSystem target)
    {
        if (target != null)
        {
            // Apply damage to the target
            damageSystem.ApplyDamage((int)attackDamage, DamageType.Physical);
            Debug.Log(gameObject.name + " attacked " + target.gameObject.name + " for " + attackDamage + " damage.");
        }
        else
        {
            Debug.LogWarning("Target is null!");
        }
    }
}
