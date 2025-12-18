using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("References")]
    public Animator playerAnimator;
    public HealthUI healthUI;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        healthUI?.PlayOuch();

        if (playerAnimator != null && currentHealth > 0)
        {
            playerAnimator.Play("Player_Hurt");
            SFXManager.Instance.PlaySFX(SFXManager.Instance.Hurt);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (playerAnimator != null)
                playerAnimator.Play("Player_Down");

            SFXManager.Instance.PlaySFX(SFXManager.Instance.Ouch);

            StartCoroutine(PlayerDownRoutine());
        }
    }

    private IEnumerator PlayerDownRoutine()
    {
        isDead = true;

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}