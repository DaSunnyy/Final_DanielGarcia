using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAnimator pa = collision.GetComponent<PlayerAnimator>();
            if (pa != null)
            {
                pa.PlayWin();
            }
        }
    }
}