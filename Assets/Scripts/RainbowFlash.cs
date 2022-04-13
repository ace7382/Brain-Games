using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RainbowFlash : MonoBehaviour
{
    public TextMeshProUGUI t;
    public float waitTime = .1f;
    public Color originalColor;

    public void StartFlash()
    {
        StartCoroutine(Flash());
    }

    public IEnumerator Flash()
    {
        originalColor = t.color;

        t.color = Color.red;

        yield return new WaitForSeconds(waitTime);

        t.color = new Color(255, 127, 0);

        yield return new WaitForSeconds(waitTime);

        t.color = Color.yellow;

        yield return new WaitForSeconds(waitTime);

        t.color = Color.green;

        yield return new WaitForSeconds(waitTime);

        t.color = Color.blue;

        yield return new WaitForSeconds(waitTime);

        t.color = new Color(75, 0, 130);

        yield return new WaitForSeconds(waitTime);

        t.color = new Color(127, 0, 255);

        yield return new WaitForSeconds(waitTime);

        t.color = originalColor;
    }
}
