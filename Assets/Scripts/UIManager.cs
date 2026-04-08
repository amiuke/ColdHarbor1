using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pickup Prompt UI")]
    [Tooltip("Panel that shows pickup prompt")]
    [SerializeField] private GameObject pickupPanel;
    
    [Tooltip("Text component for pickup prompt")]
    [SerializeField] private TextMeshProUGUI pickupText;
    
    [Header("Attack Prompt UI")]
    [Tooltip("Panel that shows attack/heating prompt")]
    [SerializeField] private GameObject attackPanel;
    
    [Tooltip("Text component for attack prompt")]
    [SerializeField] private TextMeshProUGUI attackText;
    
    [Header("Fuel UI")]
    [Tooltip("Text component for fuel amount")]
    [SerializeField] private TextMeshProUGUI fuelText;
    
    [Tooltip("Slider component for fuel bar (optional)")]
    [SerializeField] private Slider fuelSlider;
    
    [Header("References")]
    [Tooltip("Player Inventory for fuel updates")]
    [SerializeField] private PlayerInventory playerInventory;
    
    [Tooltip("Player Interactor for raycast detection")]
    [SerializeField] private PlayerInteractor playerInteractor;
    
    [Tooltip("Blowtorch for raycast detection")]
    [SerializeField] private Blowtorch blowtorch;
    
    [Tooltip("Camera/Transform for raycast origin")]
    [SerializeField] private Transform raycastOrigin;
    
    [Tooltip("Raycast distance for detecting heatable objects")]
    [SerializeField] private float raycastDistance = 10f;
    
    private static UIManager _instance;
    public static UIManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    
    private void Start()
    {
        // Validate references
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
        }
        if (playerInteractor == null)
        {
            playerInteractor = FindObjectOfType<PlayerInteractor>();
        }
        if (blowtorch == null)
        {
            blowtorch = FindObjectOfType<Blowtorch>();
        }
        if (raycastOrigin == null)
        {
            // Try to find camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                raycastOrigin = mainCamera.transform;
            }
        }
        
        // Subscribe to fuel change event
        if (playerInventory != null)
        {
            playerInventory.OnFuelChanged.AddListener(UpdateFuelUI);
            // Initial update
            UpdateFuelUI();
        }
        
        // Hide panels initially
        if (pickupPanel != null)
        {
            pickupPanel.SetActive(false);
        }
        if (attackPanel != null)
        {
            attackPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        UpdatePickupPrompt();
        UpdateAttackPrompt();
    }
    
    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnFuelChanged.RemoveListener(UpdateFuelUI);
        }
    }
    
    /// <summary>
    /// Update the pickup prompt based on raycast detection
    /// </summary>
    private void UpdatePickupPrompt()
    {
        if (pickupPanel == null || pickupText == null || playerInteractor == null)
            return;
        
        string prompt = playerInteractor.GetPickupPrompt();
        
        if (string.IsNullOrEmpty(prompt))
        {
            // No pickupable in range, hide panel
            pickupPanel.SetActive(false);
        }
        else
        {
            // Show pickup prompt
            pickupPanel.SetActive(true);
            pickupText.text = prompt;
        }
    }
    
    /// <summary>
    /// Update the attack/heating prompt based on raycast detection
    /// </summary>
    private void UpdateAttackPrompt()
    {
        if (attackPanel == null || attackText == null || raycastOrigin == null)
            return;
        
        string prompt = GetAttackPrompt();
        
        if (string.IsNullOrEmpty(prompt))
        {
            // No heatable object in range, hide panel
            attackPanel.SetActive(false);
        }
        else
        {
            // Show attack prompt
            attackPanel.SetActive(true);
            attackText.text = prompt;
        }
    }
    
    /// <summary>
    /// Get the attack/heating prompt based on what the ray is hitting
    /// </summary>
    private string GetAttackPrompt()
    {
        // Cast ray from origin
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Check if hit object has HeatReceiver component
            HeatReceiver heatReceiver = hit.collider.GetComponent<HeatReceiver>();
            
            if (heatReceiver != null)
            {
                // Check if player has fuel
                bool hasFuel = playerInventory != null && playerInventory.CurrentFuel > 0;
                
                switch (heatReceiver.materialType)
                {
                    case HeatReceiver.MaterialType.ICE:
                        return hasFuel ? "按住左键融化" : "需要燃料";
                        
                    case HeatReceiver.MaterialType.IRON:
                        return hasFuel ? "按住左键加热" : "需要燃料";
                        
                    case HeatReceiver.MaterialType.FLAMMABLE:
                        return hasFuel ? "按住左键点燃" : "需要燃料";
                        
                    case HeatReceiver.MaterialType.HEATED:
                        return "已加热";
                }
            }
            
            // Check if it's a monster
            MonsterAI monster = hit.collider.GetComponent<MonsterAI>();
            if (monster != null)
            {
                return "按左键攻击";
            }
        }
        
        return "";
    }
    
    /// <summary>
    /// Update fuel UI display
    /// </summary>
    private void UpdateFuelUI()
    {
        if (playerInventory == null)
            return;
        
        float currentFuel = playerInventory.CurrentFuel;
        float maxFuel = 10f; // Default max, you may want to expose this from PlayerInventory
        
        // Update text - display as integer
        if (fuelText != null)
        {
            fuelText.text = $"{Mathf.FloorToInt(currentFuel)}";
        }
        
        // Update slider if available
        if (fuelSlider != null)
        {
            fuelSlider.value = currentFuel / maxFuel;
        }
    }
    
    /// <summary>
    /// Show pickup panel with custom text
    /// Can be called from other scripts
    /// </summary>
    public void ShowPickupPrompt(string text)
    {
        if (pickupPanel != null && pickupText != null)
        {
            pickupPanel.SetActive(true);
            pickupText.text = text;
        }
    }
    
    /// <summary>
    /// Hide pickup panel
    /// Can be called from other scripts
    /// </summary>
    public void HidePickupPrompt()
    {
        if (pickupPanel != null)
        {
            pickupPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show attack panel with custom text
    /// Can be called from other scripts
    /// </summary>
    public void ShowAttackPrompt(string text)
    {
        if (attackPanel != null && attackText != null)
        {
            attackPanel.SetActive(true);
            attackText.text = text;
        }
    }
    
    /// <summary>
    /// Hide attack panel
    /// Can be called from other scripts
    /// </summary>
    public void HideAttackPrompt()
    {
        if (attackPanel != null)
        {
            attackPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update fuel display with specific value
    /// Can be called from other scripts
    /// </summary>
    public void UpdateFuelDisplay(float current, float max)
    {
        if (fuelText != null)
        {
            fuelText.text = $"{Mathf.FloorToInt(current)}";
        }
        
        if (fuelSlider != null)
        {
            fuelSlider.value = current / max;
        }
    }
}
