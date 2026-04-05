using UnityEngine;
using System;

public abstract class PuzzleCondition : MonoBehaviour
{
    [SerializeField] protected bool debugMode = false;
    
    protected bool isSatisfied;
    public bool IsSatisfied => isSatisfied;

    public event System.Action OnConditionChanged;

    protected void SetSatisfied(bool value)
    {
        if (isSatisfied != value)
        {
            isSatisfied = value;
            if (debugMode)
            {
                Debug.Log($"[PuzzleCondition] {gameObject.name} changed to {value}");
            }
            OnConditionChanged?.Invoke();
        }
    }
}
