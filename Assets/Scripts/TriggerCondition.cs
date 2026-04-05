using UnityEngine;

public class TriggerCondition : PuzzleCondition
{
    [Header("Trigger Settings")]
    [SerializeField] private string requiredTag = "Player";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredTag))
        {
            SetSatisfied(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredTag))
        {
            SetSatisfied(false);
        }
    }
}
