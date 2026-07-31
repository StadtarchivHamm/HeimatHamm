using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderClicked : MonoBehaviour
{
    public UnityEvent Clicked = new UnityEvent();

    private void OnMouseDown()
    {
        Clicked?.Invoke();
    }
}
