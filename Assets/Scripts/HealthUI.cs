using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public Animator healthAnimator;

    public void PlayOuch()
    {
        if (healthAnimator != null)
            healthAnimator.SetTrigger("Ow");
    }
}