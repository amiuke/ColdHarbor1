using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [Header("Slide Settings")]
    [Tooltip("Target local position offset when door is open (relative to closed position)")]
    [SerializeField] private Vector3 openPositionOffset = new Vector3(-2f, 0, 0);
    
    [Tooltip("Custom open local position (if set, will use this instead of offset)")]
    [SerializeField] private bool useCustomOpenPosition = false;
    
    [Tooltip("Custom local position when door is open")]
    [SerializeField] private Vector3 customOpenLocalPosition;
    
    [SerializeField] private float openSpeed = 2f;
    
    private Vector3 closedLocalPosition;
    private Vector3 targetLocalPosition;
    private bool isOpening = false;

    private void Start()
    {
        closedLocalPosition = transform.localPosition;
        targetLocalPosition = closedLocalPosition;
    }

    private void Update()
    {
        // Smoothly move to target local position
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * openSpeed);
    }

    /// <summary>
    /// Open the door by sliding to the target local position
    /// Can be called from PlayerInteractor or PuzzleGate
    /// </summary>
    public void OpenDoor()
    {
        if (useCustomOpenPosition)
        {
            targetLocalPosition = customOpenLocalPosition;
        }
        else
        {
            targetLocalPosition = closedLocalPosition + openPositionOffset;
        }
        isOpening = true;
    }
    
    /// <summary>
    /// Close the door by sliding back to closed local position
    /// </summary>
    public void CloseDoor()
    {
        targetLocalPosition = closedLocalPosition;
        isOpening = false;
    }
    
    /// <summary>
    /// Toggle door open/closed state
    /// </summary>
    public void ToggleDoor()
    {
        if (isOpening)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    /// <summary>
    /// Set a new custom open local position at runtime
    /// </summary>
    public void SetCustomOpenPosition(Vector3 localPosition)
    {
        customOpenLocalPosition = localPosition;
        useCustomOpenPosition = true;
    }
    
    /// <summary>
    /// Set a new offset position at runtime
    /// </summary>
    public void SetOpenOffset(Vector3 offset)
    {
        openPositionOffset = offset;
        useCustomOpenPosition = false;
    }
    
    // Draw gizmos to visualize the open position
    private void OnDrawGizmosSelected()
    {
        if (transform.parent == null) return;
        
        Vector3 targetLocalPos;
        
        if (useCustomOpenPosition && Application.isPlaying)
        {
            targetLocalPos = customOpenLocalPosition;
        }
        else
        {
            // Calculate target position in world space for visualization
            targetLocalPos = openPositionOffset;
        }
        
        Vector3 targetWorldPos = transform.parent.TransformPoint(targetLocalPos);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(targetWorldPos, transform.localScale);
        Gizmos.DrawLine(transform.position, targetWorldPos);
    }
}
