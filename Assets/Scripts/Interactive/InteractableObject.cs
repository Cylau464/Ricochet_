using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public virtual void Activate()
    {
        Debug.Log("Activate");
    }
}