using UnityEngine;

public class ObjectStateCondition : PuzzleCondition
{
    public enum CheckMode { MaterialMatch, IsOnFire, IsMelted }

    [Header("Detection Settings")]
    [SerializeField] private HeatReceiver targetObject;
    [SerializeField] private CheckMode checkMode = CheckMode.MaterialMatch;

    [Header("State Settings")]
    [SerializeField] private HeatReceiver.MaterialType requiredMaterial;
    [SerializeField] private bool expectedValue = true;

    private void Awake()
    {
        // Auto-fetch HeatReceiver if not assigned
        if (targetObject == null)
        {
            targetObject = GetComponent<HeatReceiver>();
        }
    }

    private void OnEnable()
    {
        if (targetObject != null)
        {
            // 订阅各种状态变更事件
            targetObject.OnMelted.AddListener(UpdateState);
            targetObject.OnHeated.AddListener(UpdateState);
            targetObject.OnIgnited.AddListener(UpdateState);

            // 确保 HeatReceiver 有 OnStateChanged
            targetObject.OnStateChanged.AddListener(UpdateState); 
        }
    }

    private void Start()
    {
        // 在 Start 中初始化，确保在 Awake 之后
        UpdateState();
    }

    private void OnDisable()
    {
        if (targetObject != null)
        {
            targetObject.OnMelted.RemoveListener(UpdateState);
            targetObject.OnHeated.RemoveListener(UpdateState);
            targetObject.OnIgnited.RemoveListener(UpdateState);
            targetObject.OnStateChanged.RemoveListener(UpdateState);
        }
    }

    public void UpdateState()
    {
        if (targetObject == null) return;

        bool currentState = false;

        switch (checkMode)
        {
            case CheckMode.MaterialMatch:
                // 检查材料类型匹配，例如 targetObject.materialType == MaterialType.HEATED
                currentState = (targetObject.materialType == requiredMaterial);
                break;

            case CheckMode.IsOnFire:
                // 直接获取是否着火
                currentState = targetObject.IsOnFire;
                break;

            case CheckMode.IsMelted:
                // 检查是否融化（IceRespawner 会禁用 Renderer）
                var renderer = targetObject.GetComponent<Renderer>();
                currentState = (renderer != null && !renderer.enabled);
                break;
        }

        bool result = currentState == expectedValue;
        
        // SetSatisfied 只在状态改变时通知 PuzzleGate
        SetSatisfied(result);
    }
}