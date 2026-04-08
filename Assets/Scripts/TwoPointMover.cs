using UnityEngine;
using System.Collections;

public class TwoPointMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 pointA;
    [SerializeField] private Vector3 pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool isLooping = true;
    [SerializeField] private bool startAtPointA = true;
    
    [Header("Wait Settings")]
    [Tooltip("Time to wait at each end point before turning back (in seconds)")]
    [SerializeField] private float waitTimeAtEnd = 3f;
    
    [Header("State")]
    [SerializeField] private bool isMoving = false;
    
    private bool movingToB = true;
    private bool hasCompleted = false;
    private bool isWaiting = false;
    
    public bool IsMoving => isMoving;
    public bool HasCompleted => hasCompleted;
    public bool IsWaiting => isWaiting;
    
    private void Start()
    {
        ResetToStart();
        // Set initial position
        if (startAtPointA)
        {
            transform.position = pointA;
            movingToB = true;
        }
        else
        {
            transform.position = pointB;
            movingToB = false;
        }
    }
    
    private void Update()
    {
        if (!isMoving || isWaiting)
            return;
        
        // If not looping and already completed, don't move
        if (!isLooping && hasCompleted)
            return;
        
        MoveBetweenPoints();
    }
    
    private void MoveBetweenPoints()
    {
        Vector3 target = movingToB ? pointB : pointA;
        Vector3 direction = (target - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target);
        
        // Check if we've reached the target
        float moveDistance = moveSpeed * Time.deltaTime;
        
        if (moveDistance >= distanceToTarget)
        {
            // We've reached the target
            transform.position = target;
            
            // Start waiting at the end point
            StartCoroutine(WaitAtEndPoint());
        }
        else
        {
            // Move towards target
            transform.position += direction * moveDistance;
        }
    }
    
    private IEnumerator WaitAtEndPoint()
    {
        isWaiting = true;
        
        // Wait for specified time
        yield return new WaitForSeconds(waitTimeAtEnd);
        
        // Switch direction after waiting
        movingToB = !movingToB;
        
        // If not looping and we've reached the end point
        if (!isLooping)
        {
            // Check if we've completed a full cycle (A->B->A or B->A->B)
            if ((startAtPointA && !movingToB) || (!startAtPointA && movingToB))
            {
                hasCompleted = true;
            }
        }
        
        isWaiting = false;
    }
    
    // Public methods to control movement
    public void StartMoving()
    {
        isMoving = true;
    }
    
    public void StopMoving()
    {
        isMoving = false;
    }
    
    public void ToggleMovement()
    {
        isMoving = !isMoving;
    }
    
    public void ResetToStart()
    {
        isMoving = false;
        hasCompleted = false;
        isWaiting = false;
        
        if (startAtPointA)
        {
            transform.position = pointA;
            movingToB = true;
        }
        else
        {
            transform.position = pointB;
            movingToB = false;
        }
    }
    
    public void SetPoints(Vector3 newPointA, Vector3 newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
    }
    
    /// <summary>
    /// Set the wait time at end points at runtime
    /// </summary>
    public void SetWaitTime(float time)
    {
        waitTimeAtEnd = time;
    }
    
    // Draw gizmos in scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);
        
        // Draw current position
        if (Application.isPlaying)
        {
            Gizmos.color = isWaiting ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
