using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeTrailController : MonoBehaviour
{
    public IEnumerator Trail()
    {
        while (true)
        { 
            transform.position = InputManager.instance.PrimaryPosition();
            yield return null;
        }
    }
}
