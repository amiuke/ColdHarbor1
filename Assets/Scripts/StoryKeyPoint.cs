using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class StoryKeyPoint : PuzzleCondition
{
    [Header("Story Key Point Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnEnd = true; // If true, destroy the object; if false, just disable

    [Header("Events")]
    public UnityEvent OnActivated;

    private Renderer[] renderers;
    private Collider[] colliders;
    private ThirdPersonController playerController;
    private bool isActivated = false;
    private bool isEventEnded = false;

    private void Awake()
    {
        // Get all renderers and colliders
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        // Ensure this collider is a trigger
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if already activated or ended
        if (isActivated || isEventEnded)
            return;

        // Check if the entering object is the player
        if (!other.CompareTag(playerTag))
            return;

        // Get player controller
        playerController = other.GetComponent<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[StoryKeyPoint] {gameObject.name}: Player entered but ThirdPersonController not found!");
            return;
        }

        // Activate the key point
        ActivateKeyPoint();
    }

    private void ActivateKeyPoint()
    {
        isActivated = true;

        // Disable visual components
        DisableVisuals();

        // Lock player input
        LockPlayerInput();

        // Trigger event
        OnActivated?.Invoke();

        // Set condition as satisfied
        SetSatisfied(true);

        Debug.Log($"[StoryKeyPoint] {gameObject.name} activated!");
    }

    private void DisableVisuals()
    {
        // Disable all renderers
        foreach (Renderer rend in renderers)
        {
            if (rend != null)
                rend.enabled = false;
        }

        // Disable all colliders
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

        // Disable the StarterAssetsInputs component to lock input
        StarterAssetsInputs input = playerController.GetComponent<StarterAssetsInputs>();
        if (input != null)
        {
            input.enabled = false;
        }

        // Also disable PlayerInput if using Input System
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

        // Re-enable the StarterAssetsInputs component
        StarterAssetsInputs input = playerController.GetComponent<StarterAssetsInputs>();
        if (input != null)
        {
            input.enabled = true;
        }

        // Re-enable PlayerInput if using Input System
#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.PlayerInput playerInput = playerController.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
#endif

        Debug.Log("[StoryKeyPoint] Player input unlocked.");
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

        // Unlock player input
        UnlockPlayerInput();

        // Destroy or disable the key point
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

    // Optional: Visualize the trigger area in editor
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
