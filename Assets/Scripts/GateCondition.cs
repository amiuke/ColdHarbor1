using UnityEngine;

public class GateCondition : PuzzleCondition
{
    [Header("Gate Settings")]
    [SerializeField] private PuzzleGate targetGate;
    
    private void OnEnable()
    {
        if (targetGate != null)
        {
            targetGate.OnAllConditionsMet.AddListener(OnGateAllMet);
            targetGate.OnConditionsReset.AddListener(OnGateReset);
            
            // Initial check - if target gate is already satisfied, set this condition to satisfied
            // This is a simplification - in practice, you might want to check if all conditions are currently satisfied
        }
    }
    
    private void OnDisable()
    {
        if (targetGate != null)
        {
            targetGate.OnAllConditionsMet.RemoveListener(OnGateAllMet);
            targetGate.OnConditionsReset.RemoveListener(OnGateReset);
        }
    }
    
    private void OnGateAllMet()
    {
        SetSatisfied(true);
    }
    
    private void OnGateReset()
    {
        SetSatisfied(false);
    }
}
