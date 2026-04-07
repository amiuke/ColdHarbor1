using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    public enum MonsterState
    {
        IDLE,
        CHASE,
        ATTACK,
        KNOCKBACK,
        DEAD
    }

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float knockbackDistance = 2f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float burnTickInterval = 1f;
    [SerializeField] private float burnDamagePercent = 0.3f;

    [Header("NavMesh Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 8f;

    [Header("Events")]
    public UnityEvent OnAttackHit;
    public UnityEvent<float> OnDamaged;
    public UnityEvent OnDeath;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    private NavMeshAgent navAgent;
    private float currentHealth;
    private MonsterState currentState;
    private MonsterState previousState;
    private bool isBurning = false;
    private Coroutine burnCoroutine;
    private bool canAttack = true;
    private Vector3 knockbackDirection;

    public MonsterState CurrentState => currentState;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.acceleration = acceleration;
            
            // Ensure NavMeshAgent starts disabled until placed on NavMesh
            navAgent.enabled = false;
        }

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        
        // Try to enable NavMeshAgent and place on NavMesh
        if (navAgent != null)
        {
            navAgent.enabled = true;
            
            // Check if we're on a NavMesh, if not try to find nearest valid position
            if (!navAgent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                }
                else
                {
                    Debug.LogWarning($"[MonsterAI] {gameObject.name} could not be placed on NavMesh!");
                }
            }
        }
        
        TransitionToState(MonsterState.IDLE);
    }

    private void Update()
    {
        switch (currentState)
        {
            case MonsterState.IDLE:
                UpdateIdle();
                break;
            case MonsterState.CHASE:
                UpdateChase();
                break;
            case MonsterState.ATTACK:
                UpdateAttack();
                break;
            case MonsterState.KNOCKBACK:
                // Knockback is handled in coroutine
                break;
            case MonsterState.DEAD:
                // Dead state does nothing
                break;
        }
    }

    private void UpdateIdle()
    {
        if (playerTransform == null) return;
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            TransitionToState(MonsterState.CHASE);
        }
    }

    private void UpdateChase()
    {
        if (playerTransform == null) return;
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            TransitionToState(MonsterState.ATTACK);
        }
        else if (distanceToPlayer > detectionRange)
        {
            TransitionToState(MonsterState.IDLE);
        }
        else
        {
            // Continue chasing
            navAgent.SetDestination(playerTransform.position);
        }
        Debug.Log($"[MonsterAI] {gameObject.name} is chasing player. Distance: {distanceToPlayer}");
    }

    private void UpdateAttack()
    {
        if (playerTransform == null) return;
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            TransitionToState(MonsterState.CHASE);
            return;
        }

        if (canAttack)
        {
            StartCoroutine(PerformAttack());
        }
        Debug.Log($"[MonsterAI] {gameObject.name} is attacking player. Distance: {distanceToPlayer}");
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger("Attack");

        // Deal damage
        OnAttackHit?.Invoke();

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private IEnumerator PerformKnockback()
    {
        if (playerTransform == null) yield break;

        // Disable NavMeshAgent
        navAgent.enabled = false;

        // Calculate knockback direction (away from player)
        knockbackDirection = (transform.position - playerTransform.position).normalized;
        knockbackDirection.y = 0; // Keep on ground

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + knockbackDirection * knockbackDistance;

        float elapsedTime = 0f;

        while (elapsedTime < knockbackDuration)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                (knockbackDistance / knockbackDuration) * Time.deltaTime
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Re-enable NavMeshAgent
        navAgent.enabled = true;

        // Return to previous state
        if (currentHealth > 0)
        {
            TransitionToState(previousState);
        }
        Debug.Log($"[MonsterAI] {gameObject.name} performed knockback. Current Health: {currentHealth}");
    }

    private IEnumerator BurnCoroutine()
    {
        while (isBurning && currentHealth > 0)
        {
            yield return new WaitForSeconds(burnTickInterval);

            if (isBurning && currentHealth > 0)
            {
                float burnDamage = maxHealth * burnDamagePercent;
                TakeDamage(burnDamage);
            }
        }
    }

    public void TransitionToState(MonsterState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        switch (newState)
        {
            case MonsterState.IDLE:
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                    navAgent.isStopped = true;
                if (animator != null)
                    animator.SetBool("Run_N", false);
                break;

            case MonsterState.CHASE:
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                    navAgent.isStopped = false;
                if (animator != null)
                    animator.SetBool("Run_N", true);
                break;

            case MonsterState.ATTACK:
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                    navAgent.isStopped = true;
                if (animator != null)
                    animator.SetBool("Run_N", false);
                break;

            case MonsterState.KNOCKBACK:
                StartCoroutine(PerformKnockback());
                break;

            case MonsterState.DEAD:
                if (navAgent != null)
                {
                    if (navAgent.enabled && navAgent.isOnNavMesh)
                        navAgent.isStopped = true;
                    navAgent.enabled = false;
                }

                // Disable all colliders
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                    col.enabled = false;

                OnDeath?.Invoke();

                // Destroy immediately
                Destroy(gameObject);
                break;
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentState == MonsterState.DEAD) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            TransitionToState(MonsterState.DEAD);
        }
        else
        {
            TransitionToState(MonsterState.KNOCKBACK);
        }
        Debug.Log($"[MonsterAI] {gameObject.name} took damage: {amount}. Current Health: {currentHealth}");
    }

    public void StartBurn()
    {
        // �ؼ��޸�������Ѿ���ȼ���У�ֱ�ӷ��أ���Ҫ����Э��
        if (isBurning && burnCoroutine != null)
        {
            return;
        }

        isBurning = true;
        burnCoroutine = StartCoroutine(BurnCoroutine());
    }

    public void StopBurn()
    {
        isBurning = false;
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }
    }

    // Draw gizmos for visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
