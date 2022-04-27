using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDisplayPageController : MonoBehaviour
{
    public Animator animator;

    //Called from the Tutorial Display Page's On Show Callback
    public void RestartAnimation()
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("Entry");
        }
    }

    //Called from the Tutorial Display Page's On Hidden Callback
    public void StopAnimation()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }
}
