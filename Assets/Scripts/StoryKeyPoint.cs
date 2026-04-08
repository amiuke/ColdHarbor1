using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class StoryKeyPoint : PuzzleCondition
{
    [Header("Story Key Point Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnEnd = true;

    [Header("Events")]
    public UnityEvent OnActivated;

    private Renderer[] renderers;
    private Collider[] colliders;
    private ThirdPersonController playerController;
    private bool isActivated = false;
    private bool isEventEnded = false;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated || isEventEnded)
            return;

        if (!other.CompareTag(playerTag))
            return;

        playerController = other.GetComponent<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[StoryKeyPoint] {gameObject.name}: Player entered but ThirdPersonController not found!");
            return;
        }

        ActivateKeyPoint();
    }

    private void ActivateKeyPoint()
    {
        isActivated = true;

        DisableVisuals();

        LockPlayerInput();

        FreezeAllMonsters();

        OnActivated?.Invoke();

        SetSatisfied(true);

        Debug.Log($"[StoryKeyPoint] {gameObject.name} activated!");
    }

    private void DisableVisuals()
    {
        foreach (Renderer rend in renderers)
        {
            if (rend != null)
                rend.enabled = false;
        }

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }
    }

    private void LockPlayerInput()
    {
        if (playerController == null)
            return;

        StarterAssetsInputs input = playerController.GetComponent<StarterAssetsInputs>();
        if (input != null)
        {
            input.enabled = false;
        }

#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.PlayerInput playerInput = playerController.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
#endif

        Debug.Log("[StoryKeyPoint] Player input locked.");
    }

    private void UnlockPlayerInput()
    {
        if (playerController == null)
            return;

        StarterAssetsInputs input = playerController.GetComponent<StarterAssetsInputs>();
        if (input != null)
        {
            input.enabled = true;
        }

#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.PlayerInput playerInput = playerController.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
#endif

        Debug.Log("[StoryKeyPoint] Player input unlocked.");
    }

    private void FreezeAllMonsters()
    {
        MonsterAI[] allMonsters = FindObjectsOfType<MonsterAI>();
        
        foreach (MonsterAI monster in allMonsters)
        {
            if (monster != null)
            {
                monster.TransitionToState(MonsterAI.MonsterState.STORY);
            }
        }

        Debug.Log($"[StoryKeyPoint] Froze {allMonsters.Length} monsters for story event.");
    }

    private void UnfreezeAllMonsters()
    {
        MonsterAI[] allMonsters = FindObjectsOfType<MonsterAI>();
        
        foreach (MonsterAI monster in allMonsters)
        {
            if (monster != null && monster.CurrentState == MonsterAI.MonsterState.STORY)
            {
                monster.TransitionToState(MonsterAI.MonsterState.IDLE);
            }
        }

        Debug.Log($"[StoryKeyPoint] Unfroze {allMonsters.Length} monsters after story event.");
    }

    /// <summary>
    /// Call this method to end the event and restore player control.
    /// Can be called from UnityEvent, animation events, or other scripts.
    /// </summary>
    public void EndEvent()
    {
        if (isEventEnded)
            return;

        isEventEnded = true;

        UnlockPlayerInput();

        UnfreezeAllMonsters();

        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log($"[StoryKeyPoint] {gameObject.name} event ended.");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
