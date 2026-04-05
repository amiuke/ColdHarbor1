using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class SignState : MonoBehaviour
{
    public bool completed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnConditionCompleted()
    {
        completed = true;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }
}
