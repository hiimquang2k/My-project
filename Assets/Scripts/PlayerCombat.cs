using UnityEngine;

// Example player script using the damage system
public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private DamageProfile meleeProfile;
    [SerializeField] private DamageProfile rangedProfile;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float projectileSpeed = 10f;
    
    [Header("Melee Attack")]
    [SerializeField] private Transform meleeHitbox;
    [SerializeField] private float meleeRadius = 1.5f;
    [SerializeField] private LayerMask enemyLayers;
    
    private float nextFireTime;
    private DamageDealer meleeDealer;
    
    private void Awake()
    {
        // Set up melee damage dealer
        meleeDealer = gameObject.AddComponent<DamageDealer>();
        meleeDealer.SetDamageProfile(meleeProfile);
    }
    
    private void Update()
    {
        // Handle ranged attack input
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            ShootProjectile();
            nextFireTime = Time.time + fireRate;
        }
        
        // Handle melee attack input
        if (Input.GetButtonDown("Fire2"))
        {
            PerformMeleeAttack();
        }
    }
    
    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Set up projectile damage
        DamageDealer projectileDamageDealer = projectile.GetComponent<DamageDealer>();
        if (projectileDamageDealer != null)
        {
            projectileDamageDealer.SetDamageProfile(rangedProfile);
        }
        
        // Set projectile velocity
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * projectileSpeed;
        }
        
        // Destroy after a time
        Destroy(projectile, 5f);
    }
    
    private void PerformMeleeAttack()
    {
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleeHitbox.position, meleeRadius, enemyLayers);
        
        // Apply damage to each enemy
        foreach (Collider2D enemy in hitEnemies)
        {
            meleeDealer.DealDamage(enemy.gameObject);
        }
        
        // Visual feedback
        // PlayMeleeAnimation();
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (meleeHitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeHitbox.position, meleeRadius);
        }
    }
}

// Example enemy with custom damage profile
public class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private DamageProfile contactDamageProfile;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool chasePlayer = true;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    
    private Transform player;
    private Rigidbody2D rb;
    private DamageDealer damageDealer;
    private DamageReceiver damageReceiver;
    private bool canAttack = true;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageDealer = GetComponent<DamageDealer>();
        damageReceiver = GetComponent<DamageReceiver>();
        
        // Set up the damage profile
        if (damageDealer != null && contactDamageProfile != null)
        {
            damageDealer.SetDamageProfile(contactDamageProfile);
        }
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Subscribe to damage events
        if (damageReceiver != null)
        {
            damageReceiver.OnDamageReceived += HandleDamageReceived;
        }
    }
    
    private void Update()
    {
        if (player == null || damageReceiver.IsStunned())
            return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Chase player logic
        if (chasePlayer && distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer <= attackRange && canAttack)
        {
            AttackPlayer();
        }
    }
    
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }
    
    private void AttackPlayer()
    {
        // This could be a special attack instead of just contact damage
        // For example, launching a special ability
        
        // For contact damage, we rely on the DamageDealer's OnTriggerEnter2D/OnCollisionEnter2D
        
        // For a special attack:
        // StartCoroutine(PerformSpecialAttack());
    }
    
    private void HandleDamageReceived(DamageData damageData)
    {
        // React to taking damage - could change behavior, play effects, etc.
        if (damageData.amount > 20)
        {
            // Heavy hit reaction
            // PlayHeavyHitAnimation();
        }
        
        // Could make enemy aggressive towards damage source
        if (damageData.source != null && damageData.source.CompareTag("Player"))
        {
            chasePlayer = true;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (damageReceiver != null)
        {
            damageReceiver.OnDamageReceived -= HandleDamageReceived;
        }
    }
}

// Example for a special ability that deals damage
public class FireballAbility : MonoBehaviour
{
    [SerializeField] private DamageProfile fireballProfile;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float cooldown = 3f;
    
    private float nextCastTime;
    
    public void CastFireball()
    {
        if (Time.time < nextCastTime)
            return;
            
        // Spawn fireball
        GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Set up fireball damage
        DamageDealer fireballDamageDealer = fireball.GetComponent<DamageDealer>();
        if (fireballDamageDealer != null)
        {
            fireballDamageDealer.SetDamageProfile(fireballProfile);
        }
        
        // Set fireball velocity
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = spawnPoint.right * fireballSpeed;
        }
        
        // Set cooldown
        nextCastTime = Time.time + cooldown;
        
        // Add additional effects like particle systems
        
        // Destroy after a time
        Destroy(fireball, 5f);
    }
}

// Example boss with multiple damage types
public class BossEnemy : MonoBehaviour
{
    [Header("Boss Profiles")]
    [SerializeField] private DamageProfile phaseDamageProfile;
    [SerializeField] private DamageProfile meleeDamageProfile;
    [SerializeField] private DamageProfile specialAttackProfile;
    
    [Header("Boss State")]
    [SerializeField] private float phaseHealthThreshold = 0.5f; // 50% health triggers phase change
    [SerializeField] private float specialAttackCooldown = 10f;
    
    private HealthSystem healthSystem;
    private DamageDealer damageDealer;
    private bool isPhaseTwo = false;
    private float nextSpecialAttackTime;
    
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        damageDealer = GetComponent<DamageDealer>();
        
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += CheckPhaseTransition;
        }
    }
    
    private void Update()
    {
        // Boss AI logic here
        
        // Example: Special attack on cooldown
        if (Time.time >= nextSpecialAttackTime)
        {
            PerformSpecialAttack();
            nextSpecialAttackTime = Time.time + specialAttackCooldown;
        }
    }
    
    private void CheckPhaseTransition(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        
        if (!isPhaseTwo && healthPercentage <= phaseHealthThreshold)
        {
            TransitionToPhaseTwo();
        }
    }
    
    private void TransitionToPhaseTwo()
    {
        isPhaseTwo = true;
        
        // Change damage profile
        damageDealer.SetDamageProfile(phaseDamageProfile);
        
        // Boost damage
        damageDealer.SetDamageMultiplier(2.0f);
        
        // Visual/audio feedback for phase change
        // PlayPhaseTransitionEffect();
        
        Debug.Log("Boss transitioned to Phase Two!");
    }
    
    private void PerformMeleeAttack(GameObject target)
    {
        // Create a temporary damage dealer for the melee attack
        DamageDealer meleeDealer = gameObject.AddComponent<DamageDealer>();
        meleeDealer.SetDamageProfile(meleeDamageProfile);
        meleeDealer.DealDamage(target);
        
        // Remove the temporary component after use
        Destroy(meleeDealer, 0.1f);
    }
    
    private void PerformSpecialAttack()
    {
        // Example: Area of effect attack
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, 5f);
        
        foreach (Collider2D target in targets)
        {
            if (target.gameObject != gameObject) // Don't damage self
            {
                // Create a specialized damage dealer for this attack
                DamageDealer specialDealer = gameObject.AddComponent<DamageDealer>();
                specialDealer.SetDamageProfile(specialAttackProfile);
                
                // Apply multiplier based on phase
                if (isPhaseTwo)
                {
                    specialDealer.SetDamageMultiplier(1.5f);
                }
                
                specialDealer.DealDamage(target.gameObject);
                
                // Clean up
                Destroy(specialDealer, 0.1f);
            }
        }
        
        // Visual effects for special attack
        // PlaySpecialAttackEffect();
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= CheckPhaseTransition;
        }
    }
}
