using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveToTarget : MonoBehaviour
{
    public RectTransform    target;
    public float            timeToTarget = .15f;
    public float            destinationThreshold = .05f;
    public bool             deleteOnDestination;
    public UnityEvent       onDestinationReached;

    private Vector3         veloc = Vector3.zero;

    void Update()
    {
        if (target == null)
            Destroy(gameObject);

        //transform.position = Vector3.Lerp(transform.position, target.position, 3.5f * Time.deltaTime);

        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref veloc, timeToTarget);

        if (Mathf.Abs(transform.position.x - target.position.x) < destinationThreshold &&
            Mathf.Abs(transform.position.y - target.position.y) < destinationThreshold)
        {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            OnDestinationReached();
        }
    }

    private void OnDestinationReached()
    {
        if (onDestinationReached != null)
            onDestinationReached.Invoke();

        if (deleteOnDestination)
            Destroy(gameObject);
        else
            Destroy(gameObject.GetComponent<MoveToTarget>());
    }
}
