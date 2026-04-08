using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TeleportPoint : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Destination transform for teleportation")]
    [SerializeField] private Transform destinationPoint;
    
    [Tooltip("Player tag to detect")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Teleport cooldown time in seconds")]
    [SerializeField] private float teleportCooldown = 1f;
    
    [Header("Activation State")]
    [Tooltip("Is the teleport point currently active")]
    [SerializeField] private bool isActive = false;
    
    [Tooltip("Visual object to show/hide based on activation state")]
    [SerializeField] private GameObject visualObject;
    
    [Header("Visual Animation Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool rotateClockwise = true;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;
    [SerializeField] private float floatOffset = 0f;
    
    [Header("Events")]
    public UnityEvent OnTeleportActivated;
    public UnityEvent OnTeleportDeactivated;
    public UnityEvent OnPlayerTeleported;
    
    private Vector3 initialPosition;
    private float floatTime;
    private bool isOnCooldown = false;
    private Collider triggerCollider;
    
    public bool IsActive => isActive;
    public bool IsOnCooldown => isOnCooldown;
    
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        
        // Store initial position for floating animation
        initialPosition = transform.position;
        floatTime = floatOffset;
        
        // Update visual state
        UpdateVisualState();
    }
    
    private void Update()
    {
        // Only animate if active
        if (!isActive)
            return;
        
        // Rotation animation
        float rotationDirection = rotateClockwise ? 1f : -1f;
        transform.Rotate(Vector3.up, rotationSpeed * rotationDirection * Time.deltaTime);
        
        // Floating animation
        floatTime += Time.deltaTime * floatFrequency;
        float yOffset = Mathf.Sin(floatTime * 2f * Mathf.PI) * floatAmplitude;
        
        Vector3 newPosition = initialPosition;
        newPosition.y += yOffset;
        transform.position = newPosition;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if teleport is active and not on cooldown
        if (!isActive || isOnCooldown)
            return;
        
        // Check if player entered
        if (!other.CompareTag(playerTag))
            return;
        
        // Perform teleportation
        TeleportPlayer(other.transform);
    }
    
    private void TeleportPlayer(Transform playerTransform)
    {
        if (destinationPoint == null)
        {
            Debug.LogWarning($"[TeleportPoint] {gameObject.name}: No destination point set!");
            return;
        }
        
        // Check if player has CharacterController (common in FPS/TPS controllers)
        CharacterController charController = playerTransform.GetComponent<CharacterController>();
        if (charController != null)
        {
            // Disable CharacterController before teleporting
            charController.enabled = false;
            
            // Teleport player to destination
            playerTransform.position = destinationPoint.position;
            
            // Re-enable CharacterController after teleport
            charController.enabled = true;
        }
        else
        {
            // Normal teleport without CharacterController
            playerTransform.position = destinationPoint.position;
        }
        
        // Optional: preserve player's rotation or match destination rotation
        // playerTransform.rotation = destinationPoint.rotation;
        
        // Trigger event
        OnPlayerTeleported?.Invoke();
        
        // Start cooldown
        StartCoroutine(TeleportCooldownCoroutine());
        
        Debug.Log($"[TeleportPoint] {gameObject.name}: Player teleported to {destinationPoint.name}");
    }
    
    private IEnumerator TeleportCooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(teleportCooldown);
        isOnCooldown = false;
    }
    
    /// <summary>
    /// Toggle the activation state of the teleport point
    /// </summary>
    public void ToggleActivation()
    {
        SetActivationState(!isActive);
    }
    
    /// <summary>
    /// Set the activation state directly
    /// </summary>
    public void SetActivationState(bool active)
    {
        if (isActive == active)
            return;
        
        isActive = active;
        UpdateVisualState();
        
        if (isActive)
        {
            OnTeleportActivated?.Invoke();
            Debug.Log($"[TeleportPoint] {gameObject.name} activated.");
        }
        else
        {
            OnTeleportDeactivated?.Invoke();
            Debug.Log($"[TeleportPoint] {gameObject.name} deactivated.");
        }
    }
    
    /// <summary>
    /// Activate the teleport point
    /// </summary>
    public void Activate()
    {
        SetActivationState(true);
    }
    
    /// <summary>
    /// Deactivate the teleport point
    /// </summary>
    public void Deactivate()
    {
        SetActivationState(false);
    }
    
    private void UpdateVisualState()
    {
        // Enable/disable visual object
        if (visualObject != null)
        {
            visualObject.SetActive(isActive);
        }
        
        // Enable/disable collider
        if (triggerCollider != null)
        {
            triggerCollider.enabled = isActive;
        }
    }
    
    /// <summary>
    /// Set the destination point at runtime
    /// </summary>
    public void SetDestination(Transform destination)
    {
        destinationPoint = destination;
    }
    
    /// <summary>
    /// Reset the floating animation position
    /// Call this if the teleport point is moved
    /// </summary>
    public void UpdateInitialPosition()
    {
        initialPosition = transform.position;
    }
    
    // Draw gizmos in scene view
    private void OnDrawGizmos()
    {
        // Draw teleport point
        Gizmos.color = isActive ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw connection to destination
        if (destinationPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destinationPoint.position);
            
            // Draw destination marker
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destinationPoint.position, 0.5f);
            
            // Draw arrow direction
            Vector3 direction = (destinationPoint.position - transform.position).normalized;
            Vector3 midPoint = (transform.position + destinationPoint.position) / 2f;
            Gizmos.DrawRay(midPoint, direction * 0.5f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw activation range
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        if (triggerCollider is BoxCollider boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
        }
        else if (triggerCollider is SphereCollider sphereCol)
        {
            Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
        }
    }
}
