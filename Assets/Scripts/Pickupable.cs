using UnityEngine;

public class Pickupable : MonoBehaviour
{
    public enum PickupType
    {
        FUEL,
        KEY_ITEM,
        REWARD
    }
    
    [Header("Pickup Settings")]
    public PickupType type;
    [SerializeField] private float fuelAmount = 1f;
    [SerializeField] private HeatReceiver heatReceiver;

    private Rigidbody rb;
    private Collider col;

    private Vector3 originalScale;
    private int originalLayer;

    private bool isEquipped = false;
    private bool wasPickedUp = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        originalScale = transform.localScale;
        originalLayer = gameObject.layer;

        if (heatReceiver == null)
        {
            heatReceiver = GetComponent<HeatReceiver>();
        }
    }

    public bool TryPickup(PlayerInventory inventory)
    {
        //  防止重复拾取
        if (isEquipped) return false;

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            return false;
        }
        
        switch (type)
        {
            case PickupType.FUEL:
                inventory.AddFuel(fuelAmount);
                Destroy(gameObject);
                return true;

            case PickupType.KEY_ITEM:
                inventory.EquipKeyItem(this);
                return true;

            case PickupType.REWARD:
                inventory.AddReward(1);
                Destroy(gameObject);
                return true;
        }

        return false;
    }

    // 被拿起
    public void OnPickedUp(Transform parent, float equippedScale)
    {
        isEquipped = true;
        wasPickedUp = true; // Mark as picked up

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * equippedScale;

        //关键：避免射线再次命中
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    // 被丢下
    public void OnDropped(Vector3 position, Vector3 forward, float dropDistance)
    {
        isEquipped = false;
        transform.SetParent(null);
        transform.position = position + forward * dropDistance;

        if (rb != null) rb.isKinematic = false;
        if (col != null) col.enabled = true;

        transform.localScale = originalScale;

        gameObject.layer = originalLayer; // ← 用存好的原始 Layer，而不是硬编码 "Default"
    }
    
    // Check if the item was ever picked up
    public bool WasPickedUp => wasPickedUp;
}