using UnityEngine;
using System.Collections;

public class IceRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 3.0f;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnRespawnBegin;
    public UnityEngine.Events.UnityEvent OnRespawnComplete;
    
    private HeatReceiver heatReceiver;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    
    private void Awake()
    {
        // Get reference to HeatReceiver
        heatReceiver = GetComponent<HeatReceiver>();
        
        // Record original transform
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }
    
    private void Start()
    {
        // Subscribe to OnMelted event
        if (heatReceiver != null)
        {
            heatReceiver.OnMelted.AddListener(StartRespawnCoroutine);
        }
    }
    
    private void StartRespawnCoroutine()
    {
        StartCoroutine(RespawnIce());
    }

    private IEnumerator RespawnIce()
    {
        OnRespawnBegin?.Invoke();

        // 1. ��������
        SetVisibilityAndPhysics(false);

        // 2. �ȴ�
        yield return new WaitForSeconds(respawnDelay);

        // 3. ����״̬
        transform.SetPositionAndRotation(originalPosition, originalRotation);
        transform.localScale = originalScale;
        ResetHeatReceiver();

        // 4. ������ʾ
        SetVisibilityAndPhysics(true);

        OnRespawnComplete?.Invoke();
    }

    private void SetVisibilityAndPhysics(bool isActive)
    {
        // ����/������Ⱦ
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = isActive;

        // ����/������ײ
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = isActive;
    }
    private void ResetHeatReceiver()
    {
        if (heatReceiver != null)
        {
            // Use the public Reset method to reset HeatReceiver to default state
            heatReceiver.Reset();
        }
    }
}
