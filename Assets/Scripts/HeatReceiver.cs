using UnityEngine;
using UnityEngine.Events;

public class HeatReceiver : MonoBehaviour
{
    public enum MaterialType
    {
        ICE,
        IRON,
        FLAMMABLE,
        HEATED
    }

    [Header("Material Properties")]
    public MaterialType materialType = MaterialType.IRON;
    
    [Header("Configuration")]
    [SerializeField] private HeatMaterialConfig materialConfig; // ScriptableObject with material references
    
    [Header("Heat Thresholds")]
    [SerializeField] private float meltTime = 2.0f; // Time to melt ICE
    [SerializeField] private float heatTime = 3.0f; // Time to heat IRON to HEATED
    [SerializeField] private float coolTime = 5.0f; // Time for HEATED to cool back to IRON
    [SerializeField] private float igniteTime = 1.5f; // Time to ignite FLAMMABLE
    [SerializeField] private float heatRadius = 2.0f; // Radius for heat emission from HEATED/IGNITED objects
    
    [Header("Events")]
    public UnityEvent OnMelted = new UnityEvent();
    public UnityEvent OnHeated = new UnityEvent();
    public UnityEvent OnIgnited = new UnityEvent();
    public UnityEvent OnStateChanged = new UnityEvent();



    private float heatAccumulation = 0f;
    private float coolDownTimer = 0f;
    private bool isIgnited = false;
    private bool isCooling = false;
    private Renderer renderer;
    private static HeatMaterialConfig defaultMaterialConfig;
    private MaterialType originalMaterialType; // Store original material type for reset
    
    // Is the object currently on fire?
    public bool IsOnFire { get; private set; }
    
    private void Start()
    {
        // Get renderer component for material changes
        renderer = GetComponent<Renderer>();
        
        // Get or create default material config if not assigned
        if (materialConfig == null)
        {
            materialConfig = GetDefaultMaterialConfig();
        }
        
        // Store original material type
        originalMaterialType = materialType;
        
        // Set default parameters based on material type
        SetDefaultParametersByMaterialType();
        
        if (renderer != null)
        {
            // Set initial material based on material type
            UpdateMaterial();
        }
    }
    
    // Set default parameters based on material type
    private void SetDefaultParametersByMaterialType()
    {
        switch (materialType)
        {
            case MaterialType.ICE:
                meltTime = 2.0f; // 融化时间3秒
                heatRadius = 0f; // 冰不导热
                break;
            case MaterialType.IRON:
                heatTime = 2.0f; // 加热时间3秒
                coolTime = 5.0f; // 冷却时间5秒
                heatRadius = 10f; // 导热半径0.5
                break;
            case MaterialType.FLAMMABLE:
                igniteTime = 1.5f; // 点燃时间1.5秒
                heatRadius = 20f; // 燃烧导热半径4
                break;
            case MaterialType.HEATED:
                // HEATED inherits parameters from IRON
                heatTime = 2.0f;
                coolTime = 5.0f;
                heatRadius = 10f;
                break;
        }
    }
    
    // Get or create default material config
    private HeatMaterialConfig GetDefaultMaterialConfig()
    {
        if (defaultMaterialConfig == null)
        {
            // Try to find existing HeatMaterialConfig asset
            HeatMaterialConfig[] configs = Resources.FindObjectsOfTypeAll<HeatMaterialConfig>();
            if (configs.Length > 0)
            {
                defaultMaterialConfig = configs[0];
            }
            else
            {
                // Create a new default config if none exists
                defaultMaterialConfig = ScriptableObject.CreateInstance<HeatMaterialConfig>();
                // Note: This will only exist in memory, not as an asset
                // In a real project, you should create an asset file in the editor
            }
        }
        return defaultMaterialConfig;
    }
    
    // Entry point for all heat application
    public void ReceiveHeat(float deltaHeat)
    {
        // Reset cooldown timer when receiving heat
        coolDownTimer = 0f;
        isCooling = false;

        // Apply heat based on material type
        switch (materialType)
        {
            case MaterialType.ICE:
                HandleIceHeat(deltaHeat);
                break;
            case MaterialType.IRON:
                HandleIronHeat(deltaHeat);
                break;
            case MaterialType.FLAMMABLE:
                HandleFlammableHeat(deltaHeat);
                break;
            case MaterialType.HEATED:
                // HEATED objects don't accumulate additional heat
                break;
        }
        OnStateChanged?.Invoke();
    }

    private void HandleIceHeat(float deltaHeat)
    {
        heatAccumulation += deltaHeat;
        
        if (heatAccumulation >= meltTime)
        {
            // Destroy or deactivate ICE
            OnMelted?.Invoke();
            //gameObject.SetActive(false);
        }
    }
    
    private void HandleIronHeat(float deltaHeat)
    {
        heatAccumulation += deltaHeat;
        
        if (heatAccumulation >= heatTime)
        {
            // Change to HEATED material
            materialType = MaterialType.HEATED;
            UpdateMaterial();
            OnHeated?.Invoke();
        }
    }
    
    private void HandleFlammableHeat(float deltaHeat)
    {
        if (!isIgnited)
        {
            heatAccumulation += deltaHeat;
            
            if (heatAccumulation >= igniteTime)
            {
                // Ignite FLAMMABLE object
                isIgnited = true;
                IsOnFire = true;
                UpdateMaterial();
                OnIgnited?.Invoke();
            }
        }
    }
    
    private void Update()
    {
        // Handle heat emission for HEATED and ignited FLAMMABLE objects
        if (materialType == MaterialType.HEATED || (materialType == MaterialType.FLAMMABLE && isIgnited))
        {
            EmitHeat();
        }
        
        // Handle cooling for HEATED objects
        if (materialType == MaterialType.HEATED)
        {
            HandleCooling();
        }
    }
    
    private void EmitHeat()
    {
        // Overlap sphere to find nearby HeatReceiver components
        Collider[] colliders = Physics.OverlapSphere(transform.position, heatRadius);
        
        foreach (Collider collider in colliders)
        {
            HeatReceiver otherHeatReceiver = collider.GetComponent<HeatReceiver>();
            if (otherHeatReceiver != null && otherHeatReceiver != this)
            {
                // Only apply heat if this object is ignited FLAMMABLE
                // HEATED IRON should not ignite FLAMMABLE objects
                if (materialType == MaterialType.FLAMMABLE && isIgnited)
                {
                    otherHeatReceiver.ReceiveHeat(Time.deltaTime);
                }
                // HEATED IRON can still melt ICE
                else if (materialType == MaterialType.HEATED && otherHeatReceiver.materialType != MaterialType.FLAMMABLE)
                {
                    otherHeatReceiver.ReceiveHeat(Time.deltaTime);
                }
            }
        }
    }
    
    private void HandleCooling()
    {
        if (!isCooling)
        {
            isCooling = true;
        }
        
        coolDownTimer += Time.deltaTime;

        if (coolDownTimer >= coolTime)
        {
            // Cool back to IRON
            materialType = MaterialType.IRON;
            UpdateMaterial();
            heatAccumulation = 0f;
            coolDownTimer = 0f;
            isCooling = false;
        }
        OnStateChanged?.Invoke();
    }

    // Update material based on current state
    private void UpdateMaterial()
    {
        if (renderer == null || materialConfig == null)
            return;
        
        switch (materialType)
        {
            case MaterialType.ICE:
                if (materialConfig.iceMaterial != null)
                    renderer.material = materialConfig.iceMaterial;
                break;
            case MaterialType.IRON:
                if (materialConfig.ironMaterial != null)
                    renderer.material = materialConfig.ironMaterial;
                break;
            case MaterialType.FLAMMABLE:
                if (isIgnited && materialConfig.ignitedMaterial != null)
                {
                    renderer.material = materialConfig.ignitedMaterial;
                    renderer.GetComponent<Collider>().isTrigger = true; // Ignited FLAMMABLE becomes non-solid
                    renderer.GetComponent<Rigidbody>().isKinematic = true; // Disable physics on ignited FLAMMABLE
                }
                else if (materialConfig.flammableMaterial != null)
                {
                    renderer.material = materialConfig.flammableMaterial;
                }
                break;
            case MaterialType.HEATED:
                if (materialConfig.heatedMaterial != null)
                    renderer.material = materialConfig.heatedMaterial;
                break;
        }
    }
    
    // Reset HeatReceiver to default state
    public void Reset()
    {
        heatAccumulation = 0f;
        IsOnFire = false;
        isIgnited = false;
        isCooling = false;
        coolDownTimer = 0f;
        
        // Reset material type to original
        materialType = originalMaterialType;
        
        // Reset parameters based on material type
        SetDefaultParametersByMaterialType();
        
        // Update material
        UpdateMaterial();

        // Reset collider and rigidbody for FLAMMABLE objects
        if (materialType == MaterialType.FLAMMABLE)
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        OnStateChanged?.Invoke();
    }

    // Called by WeldingManager when welded
    public void Weld()
    {
        OnStateChanged?.Invoke();
    }
    
    /// <summary>
    /// Directly set the material to HEATED state
    /// Can be called from other scripts to instantly heat up the object
    /// </summary>
    public void SetHeated()
    {
        // Only IRON can become HEATED
        if (materialType != MaterialType.IRON)
        {
            Debug.LogWarning($"[HeatReceiver] {gameObject.name}: Only IRON can become HEATED. Current type: {materialType}");
            return;
        }
        
        materialType = MaterialType.HEATED;
        UpdateMaterial();
        OnHeated?.Invoke();
        OnStateChanged?.Invoke();
        
        Debug.Log($"[HeatReceiver] {gameObject.name} is now HEATED.");
    }
    
    /// <summary>
    /// Directly ignite the object (for FLAMMABLE materials)
    /// </summary>
    public void Ignite()
    {
        if (materialType != MaterialType.FLAMMABLE)
        {
            Debug.LogWarning($"[HeatReceiver] {gameObject.name}: Only FLAMMABLE can be ignited. Current type: {materialType}");
            return;
        }
        
        isIgnited = true;
        IsOnFire = true;
        UpdateMaterial();
        OnIgnited?.Invoke();
        OnStateChanged?.Invoke();
        
        Debug.Log($"[HeatReceiver] {gameObject.name} is now IGNITED.");
    }
    
    /// <summary>
    /// Directly melt the object (for ICE materials)
    /// </summary>
    public void Melt()
    {
        if (materialType != MaterialType.ICE)
        {
            Debug.LogWarning($"[HeatReceiver] {gameObject.name}: Only ICE can be melted. Current type: {materialType}");
            return;
        }
        
        // Disable renderer to simulate melting
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        OnMelted?.Invoke();
        OnStateChanged?.Invoke();
        
        Debug.Log($"[HeatReceiver] {gameObject.name} has MELTED.");
    }
}
