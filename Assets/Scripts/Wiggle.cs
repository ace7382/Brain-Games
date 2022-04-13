using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    public float                duration;
    public float                intensity;
    public float                delay = -1;

    public bool                 rotationWiggleX;
    public bool                 rotationWiggleY;
    public bool                 rotationWiggleZ;

    public bool                 positionWiggleX;
    public bool                 positionWiggleY;
    public bool                 positionWiggleZ;

    private IEnumerator         wiggler;
    private RectTransform       rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (delay == -1)
            delay = Random.Range(0f, 4f);
    }

    private void Update()
    {
        if (delay <= 0)
        {
            if (wiggler == null)
            {
                wiggler = Jiggle();

                StartCoroutine(wiggler);
            }
        }
        else
        {
            delay -= Time.deltaTime;
        }
    }

    private IEnumerator Jiggle()
    {
        rectTransform.localEulerAngles = new Vector3(
            rotationWiggleX ? Random.Range(intensity * -1, intensity) : 0,
            rotationWiggleY ? Random.Range(intensity * -1, intensity) : 0,
            rotationWiggleZ ? Random.Range(intensity * -1, intensity) : 0
            );

        rectTransform.localPosition = new Vector3(
            positionWiggleX ? Random.Range(intensity * -1, intensity) : 0,
            positionWiggleY ? Random.Range(intensity * -1, intensity) : 0,
            positionWiggleZ ? Random.Range(intensity * -1, intensity) : 0
            );

        yield return new WaitForSeconds(duration);

        wiggler = null;
    }
}
