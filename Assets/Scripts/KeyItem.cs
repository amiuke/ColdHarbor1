using UnityEngine;
using UnityEngine.Events;

public class KeyItem : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnKeyUsed;
    
    private bool isUsed = false;
    
    /// <summary>
    /// Call this method to destroy the key item
    /// Can be called from UnityEvent on the door, PuzzleGate, or other scripts
    /// </summary>
    public void UseKey()
    {
        if (isUsed)
            return;
        
        isUsed = true;
        
        OnKeyUsed?.Invoke();
        
        Debug.Log($"[KeyItem] {gameObject.name} has been used and destroyed.");
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Alternative method name for UnityEvent binding
    /// </summary>
    public void DestroyKey()
    {
        UseKey();
    }
}
