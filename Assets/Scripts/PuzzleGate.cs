using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PuzzleGate : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private bool requireAllSimultaneous = true;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnAllConditionsMet;
    public UnityEngine.Events.UnityEvent OnConditionsReset;
    
    private List<PuzzleCondition> conditions;
    private HashSet<PuzzleCondition> satisfiedConditions;
    private bool allConditionsWereMet;
    
    // Debug visualization
    public int CurrentSatisfiedCount => conditions?.Where(c => c.IsSatisfied).Count() ?? 0;
    public int TotalConditionCount => conditions?.Count ?? 0;
    public bool IsAllSatisfied => conditions != null && conditions.All(c => c.IsSatisfied);
    
    private void Awake()
    {
        // ������ֱ��������� PuzzleCondition������ݹ��ռ��������ڲ�������
        conditions = new List<PuzzleCondition>();
        foreach (Transform child in transform)
        {
            var condition = child.GetComponent<PuzzleCondition>();
            if (condition != null)
            {
                conditions.Add(condition);
            }
        }

        satisfiedConditions = new HashSet<PuzzleCondition>();
        
        if (debugMode)
        {
            Debug.Log($"[PuzzleGate] {gameObject.name} found {conditions.Count} conditions");
        }
        
        // Subscribe to condition changes
        foreach (var condition in conditions)
        {
            condition.OnConditionChanged += OnConditionChanged;
        }
        
        // Initial check
        CheckPuzzleStatus();
    }
    
    private void OnConditionChanged()
    {
        CheckPuzzleStatus();
    }
    
    private void CheckPuzzleStatus()
    {
        var currentSatisfied = conditions.Where(c => c.IsSatisfied).ToList();
        
        if (requireAllSimultaneous)
        {
            bool allMet = currentSatisfied.Count == conditions.Count;
            
            if (debugMode)
            {
                Debug.Log($"[PuzzleGate] {gameObject.name}: {currentSatisfied.Count}/{conditions.Count} satisfied, allMet={allMet}, wasMet={allConditionsWereMet}");
            }
            
            if (allMet && !allConditionsWereMet)
            {
                // All conditions just became satisfied
                allConditionsWereMet = true;
                if (debugMode)
                {
                    Debug.Log($"[PuzzleGate] {gameObject.name}: ALL CONDITIONS MET!");
                }
                OnAllConditionsMet?.Invoke();
            }
            else if (!allMet && allConditionsWereMet)
            {
                // Some condition became unsatisfied after all were met
                allConditionsWereMet = false;
                if (debugMode)
                {
                    Debug.Log($"[PuzzleGate] {gameObject.name}: CONDITIONS RESET!");
                }
                OnConditionsReset?.Invoke();
            }
        }
        else
        {
            // One-way latch: each condition only needs to be satisfied once
            foreach (var condition in currentSatisfied)
            {
                satisfiedConditions.Add(condition);
            }
            
            bool allEverSatisfied = satisfiedConditions.Count == conditions.Count;
            
            if (debugMode)
            {
                Debug.Log($"[PuzzleGate] {gameObject.name}: one-way latch {satisfiedConditions.Count}/{conditions.Count} satisfied");
            }
            
            if (allEverSatisfied && !allConditionsWereMet)
            {
                // All conditions have been satisfied at least once
                allConditionsWereMet = true;
                if (debugMode)
                {
                    Debug.Log($"[PuzzleGate] {gameObject.name}: ALL CONDITIONS MET (one-way)!");
                }
                OnAllConditionsMet?.Invoke();
            }
        }
    }
}
