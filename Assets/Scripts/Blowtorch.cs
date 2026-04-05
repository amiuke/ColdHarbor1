using UnityEngine;

public class Blowtorch : MonoBehaviour
{
    [Header("Blowtorch Settings")]
    [SerializeField] private Transform raycastOrigin; // Camera or hand transform
    [SerializeField] private float raycastDistance = 10.0f;
    [SerializeField] private float heatPerSecond = 1.0f;
    [SerializeField] private float flameWidth = 0.4f; // Width of the flame cylinder
    [SerializeField] private Material flameMaterial; // Material for the flame (will use ignited material if not set)
    
    [Header("Fuel System")]
    [SerializeField] private float fuelConsumptionTime = 10.0f; // Time in seconds to consume 1 fuel
    
    [Header("Input")]
    [SerializeField] private string fireButton = "Fire1"; // Default left mouse button
    
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    
    private WeldingManager weldingManager;
    private RaycastHit currentHit;
    private bool isFiring = false;
    private float fireTime = 0f;
    private GameObject flameObject;
    private MeshRenderer flameRenderer;
    
    private void Awake()
    {
        // Get reference to WeldingManager if it exists on the same GameObject
        weldingManager = GetComponent<WeldingManager>();
        
        // Create flame object
        CreateFlameObject();
    }
    
    private void CreateFlameObject()
    {
        // Create cylinder for flame
        flameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flameObject.name = "BlowtorchFlame";
        flameObject.transform.parent = raycastOrigin;
        flameObject.transform.localPosition = Vector3.zero;
        // Rotate cylinder 90 degrees around X-axis to make it extend along Z-axis
        flameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Get renderer
        flameRenderer = flameObject.GetComponent<MeshRenderer>();
        
        // Set initial flame state
        flameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Check if fire button is being held and has fuel
        bool fireInput = Input.GetButton(fireButton);
        isFiring = fireInput && inventory != null && inventory.CurrentFuel > 0;
        
        if (isFiring)
        {
            // Update fire time
            fireTime += Time.deltaTime;
            
            // Check for fuel consumption
            if (fireTime >= fuelConsumptionTime)
            {
                ConsumeFuel();
                fireTime = 0f;
            }
            
            FireRaycast();
            UpdateFlame();
        }
        else
        {
            // Reset welding progress when not firing
            if (weldingManager != null)
            {
                weldingManager.ResetWeldProgress();
            }
            
            // Deactivate flame
            if (flameObject != null)
            {
                flameObject.SetActive(false);
            }
        }
    }
    
    private void FireRaycast()
    {
        // Cast ray from origin
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        
        if (Physics.Raycast(ray, out currentHit, raycastDistance))
        {
            // Check if hit object has HeatReceiver component
            HeatReceiver heatReceiver = currentHit.collider.GetComponent<HeatReceiver>();
            
            if (heatReceiver != null)
            {
                // Apply heat to the object
                heatReceiver.ReceiveHeat(heatPerSecond * Time.deltaTime);
                
                // Update welding manager with current hit
                if (weldingManager != null)
                {
                    weldingManager.UpdateWeldTarget(currentHit.collider);
                }
            }
            else
            {
                // Reset welding progress if not hitting a HeatReceiver
                if (weldingManager != null)
                {
                    weldingManager.ResetWeldProgress();
                }
            }
        }
        else
        {
            // Reset welding progress if raycast doesn't hit anything
            if (weldingManager != null)
            {
                weldingManager.ResetWeldProgress();
            }
        }
    }
    
    private void UpdateFlame()
    {
        if (flameObject == null)
            return;
        
        // Activate flame
        if (!flameObject.activeInHierarchy)
        {
            flameObject.SetActive(true);
            flameObject.GetComponent<Collider>().isTrigger = true; // Disable collider on flame object
            // Set flame material
            if (flameMaterial == null)
            {
                // Try to get ignited material from HeatMaterialConfig
                HeatMaterialConfig[] configs = Resources.FindObjectsOfTypeAll<HeatMaterialConfig>();
                if (configs.Length > 0 && configs[0].ignitedMaterial != null)
                {
                    flameMaterial = configs[0].ignitedMaterial;
                }
            }
            
            if (flameRenderer != null && flameMaterial != null)
            {
                flameRenderer.material = flameMaterial;
            }
        }
        
        // Calculate flame length
        float flameLength = raycastDistance;
        if (currentHit.collider != null)
        {
            flameLength = currentHit.distance;
        }
        
        // Update flame scale and position
        // Cylinder is rotated 90 degrees around X-axis, so it extends along Z-axis
        // Scale the Y-axis (which now corresponds to Z-axis in world space)
        flameObject.transform.localScale = new Vector3(flameWidth, flameLength / 2, flameWidth);
        // Position the flame so it starts at the origin and extends forward along Z-axis
        flameObject.transform.localPosition = new Vector3(0, 0, flameLength / 2);
    }
    
    private void ConsumeFuel()
    {
        if (inventory != null && inventory.CurrentFuel > 0)
        {
            inventory.AddFuel(-1f); // Use negative value to consume fuel
            Debug.Log("Fuel consumed. Remaining: " + inventory.CurrentFuel);
        }
    }
    
    // Draw raycast in scene view for debugging
    private void OnDrawGizmos()
    {
        if (raycastOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * raycastDistance);
        }
    }
}
