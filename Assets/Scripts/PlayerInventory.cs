using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 10f;
    [SerializeField] private float initialFuel = 3f;
    
    [Header("Key Item Settings")]
    [SerializeField] private Transform rightHandBone;
    [SerializeField] private float equippedScale = 0.3f;
    [SerializeField] private float dropDistance = 1f;
    
    [Header("Events")]
    public UnityEvent OnFuelChanged;
    public UnityEvent OnEquippedItemChanged;
    
    private float currentFuel;
    private Pickupable equippedKeyItem;

    public float CurrentFuel => currentFuel;
    public Pickupable EquippedKeyItem => equippedKeyItem;
    
    private void Start()
    {
        currentFuel = initialFuel;
        OnFuelChanged?.Invoke();
    }
    
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
        OnFuelChanged?.Invoke();
    }
    
    public void EquipKeyItem(Pickupable item)
    {
        if (equippedKeyItem != null)
        {
            DropKeyItem();
        }

        equippedKeyItem = item;

        item.OnPickedUp(rightHandBone, equippedScale);
        
        OnEquippedItemChanged?.Invoke();
    }
    
    public void DropKeyItem()
    {
        if (equippedKeyItem == null)
            return;

        equippedKeyItem.OnDropped(transform.position, transform.forward, dropDistance);

        equippedKeyItem = null;
        
        OnEquippedItemChanged?.Invoke();
    }
}