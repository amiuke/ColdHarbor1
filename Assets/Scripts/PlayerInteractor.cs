using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interactor Settings")]
    [SerializeField] private Transform raycastOrigin; // Same as Blowtorch
    [SerializeField] private float raycastDistance = 2.0f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    
    private RaycastHit currentHit;
    private Pickupable hitPickupable;
    
    private void Update()
    {
        // Cast ray for interaction
        CastRay();
        
        // Handle interaction input
        if (Input.GetKeyDown(interactKey))
        {
            HandleInteraction();
        }
    }
    
    private void CastRay()
    {
        // Cast ray from origin
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        
        if (Physics.Raycast(ray, out currentHit, raycastDistance))
        {
            Debug.Log("Hit: " + currentHit.collider.name);
            // Check if hit object is pickupable
            hitPickupable = currentHit.collider.GetComponent<Pickupable>();
        }
        else
        {
            hitPickupable = null;
        }
    }
    
    private void HandleInteraction()
    {
        if (hitPickupable != null)
        {
            // Try to pickup the item
            hitPickupable.TryPickup(inventory);
        }
        else if (inventory.EquippedKeyItem != null)
        {
            // Drop equipped key item
            inventory.DropKeyItem();
        }
    }
    
    // Draw raycast in scene view for debugging
    private void OnDrawGizmos()
    {
        if (raycastOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * raycastDistance);
        }
    }
    
    // Get pickup prompt text
    public string GetPickupPrompt()
    {
        if (hitPickupable != null)
        {
            // Check if pickupable has HeatReceiver and is on fire
            HeatReceiver heatReceiver = hitPickupable.GetComponent<HeatReceiver>();
            if (heatReceiver != null && heatReceiver.IsOnFire)
            {
                return "Cannot pick up";
            }
            return "E: Pick up";
        }
        else if (inventory.EquippedKeyItem != null)
        {
            return "E: Drop";
        }
        return "";
    }
}
