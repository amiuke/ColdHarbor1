using UnityEngine;

public class WeldingManager : MonoBehaviour
{
    [Header("Welding Settings")]
    [SerializeField] private float weldTime = 2.0f; // Time required to weld
    [SerializeField] private float weldDistanceThreshold = 1.0f; // Max distance between objects to weld
    
    [Header("Debug")]
    [SerializeField] private float weldProgress = 0f; // Current weld progress (0-1)
    
    private Collider currentTarget; // Current raycast target
    private Collider weldCandidate; // Nearest weldable object
    
    // Update the current weld target and find potential weld candidates
    public void UpdateWeldTarget(Collider target)
    {
        currentTarget = target;
        
        // Check if current target is weldable
        if (currentTarget.CompareTag("Weldable"))
        {
            // Find nearest weldable object
            FindNearestWeldable();
            
            // If a weld candidate is found, progress welding
            if (weldCandidate != null)
            {
                ProgressWelding();
            }
            else
            {
                // Reset progress if no candidate found
                ResetWeldProgress();
            }
        }
        else
        {
            // Reset progress if target is not weldable
            ResetWeldProgress();
        }
    }
    
    private void FindNearestWeldable()
    {
        // Get all weldable objects in the scene
        GameObject[] weldableObjects = GameObject.FindGameObjectsWithTag("Weldable");
        
        float closestDistance = Mathf.Infinity;
        Collider closestCollider = null;
        
        foreach (GameObject obj in weldableObjects)
        {
            // Skip the current target itself
            if (obj == currentTarget.gameObject)
                continue;
            
            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null)
            {
                // Calculate distance between objects
                float distance = Vector3.Distance(currentTarget.transform.position, objCollider.transform.position);
                
                // Check if within weld distance threshold
                if (distance < weldDistanceThreshold && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = objCollider;
                }
            }
        }
        
        weldCandidate = closestCollider;
    }
    
    private void ProgressWelding()
    {
        // Increase weld progress
        weldProgress += Time.deltaTime / weldTime;
        
        // Clamp progress between 0 and 1
        weldProgress = Mathf.Clamp01(weldProgress);
        
        // Check if welding is complete
        if (weldProgress >= 1.0f)
        {
            Weld(currentTarget, weldCandidate);
            ResetWeldProgress();
        }
    }
    
    // Reset weld progress
    public void ResetWeldProgress()
    {
        weldProgress = 0f;
        currentTarget = null;
        weldCandidate = null;
    }
    
    // Perform welding between two objects
    private void Weld(Collider objA, Collider objB)
    {
        // Get HeatReceiver components and call Weld() method
        HeatReceiver heatReceiverA = objA.GetComponent<HeatReceiver>();
        HeatReceiver heatReceiverB = objB.GetComponent<HeatReceiver>();
        
        if (heatReceiverA != null)
            heatReceiverA.Weld();
        
        if (heatReceiverB != null)
            heatReceiverB.Weld();
        
        // Get Rigidbody components
        Rigidbody rbA = objA.GetComponent<Rigidbody>();
        Rigidbody rbB = objB.GetComponent<Rigidbody>();
        
        if (rbA != null && rbB != null)
        {
            // Create FixedJoint between the two objects
            FixedJoint joint = objA.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rbB;
            
            // Merge Rigidbodies if one is kinematic
            if (rbA.isKinematic || rbB.isKinematic)
            {
                // Make both kinematic
                rbA.isKinematic = true;
                rbB.isKinematic = true;
            }
        }
    }
    
    // Draw debug gizmos for weld candidates
    private void OnDrawGizmos()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget.transform.position, weldDistanceThreshold);
            
            if (weldCandidate != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(currentTarget.transform.position, weldCandidate.transform.position);
            }
        }
    }
}
