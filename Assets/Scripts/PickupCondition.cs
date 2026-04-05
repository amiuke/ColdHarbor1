using UnityEngine;

public class PickupCondition : PuzzleCondition
{
    [Header("Pickup Settings")]
    [SerializeField] private Pickupable targetItem;
    [SerializeField] private bool requireHeld = true;
    
    private PlayerInventory inventory;
    
    private void Awake()
    {
        // Auto-fetch Pickupable if not assigned
        if (targetItem == null)
        {
            targetItem = GetComponent<Pickupable>();
        }
    }
    
    private void Start()
    {
        // Find PlayerInventory in the scene
        inventory = FindObjectOfType<PlayerInventory>();
        
        if (inventory != null)
        {
            inventory.OnEquippedItemChanged.AddListener(UpdatePickupStatus);
            
            // Initial check
            UpdatePickupStatus();
        }
    }
    
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnEquippedItemChanged.RemoveListener(UpdatePickupStatus);
        }
    }
    
    private void UpdatePickupStatus()
    {
        if (inventory == null)
            return;
        
        bool isSatisfied = false;
        
        if (requireHeld)
        {
            // Must be currently held in player's hand
            isSatisfied = inventory.EquippedKeyItem == targetItem;
        }
        else
        {
            // Just needs to have been picked up once
            // We'll use a flag on the Pickupable to track this
            isSatisfied = targetItem != null && targetItem.WasPickedUp;
        }
        
        SetSatisfied(isSatisfied);
    }
}
