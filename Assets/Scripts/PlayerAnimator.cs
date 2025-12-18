using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement player;

    private static readonly int Player_Idle = Animator.StringToHash("Player_Idle");
    private static readonly int Player_Move = Animator.StringToHash("Player_Move");
    private static readonly int Player_Jump = Animator.StringToHash("Player_Jump");
    private static readonly int Player_Fall = Animator.StringToHash("Player_Fall");
    private static readonly int Player_Climb = Animator.StringToHash("Player_Climb");
    private static readonly int Player_Crouch = Animator.StringToHash("Player_Crouch");
    private static readonly int Player_LookUp = Animator.StringToHash("Player_LookUp");

    private static readonly int Player_MAG_Idle = Animator.StringToHash("Player_MAG_Idle");
    private static readonly int Player_MAG_Move = Animator.StringToHash("Player_MAG_Move");
    private static readonly int Player_MAG_Jump = Animator.StringToHash("Player_MAG_Jump");
    private static readonly int Player_MAG_Fall = Animator.StringToHash("Player_MAG_Fall");
    private static readonly int Player_MAG_LookUp = Animator.StringToHash("Player_MAG_LookUp");
    private static readonly int Player_MAG_JumpLookUp = Animator.StringToHash("Player_MAG_JumpLookUp");
    private static readonly int Player_MAG_FallLookUp = Animator.StringToHash("Player_MAG_FallLookUp");
    private static readonly int Player_MAG_MoveLookUp = Animator.StringToHash("Player_MAG_MoveLookUp");
    private static readonly int Player_MAG_Climb = Animator.StringToHash("Player_MAG_Climb");
    private static readonly int Player_MAG_Crouch = Animator.StringToHash("Player_MAG_Crouch");

    private static readonly int Player_Hurt = Animator.StringToHash("Player_Hurt");
    private static readonly int Player_Down = Animator.StringToHash("Player_Down");
    private static readonly int Player_Win = Animator.StringToHash("Player_Win");

    private int currentState = -1;
    private bool isDisabled = false;

    void Update()
    {
        if (player == null || animator == null || player.rb == null || isDisabled) return;

        int newState = GetState();
        if (newState != currentState)
        {
            animator.CrossFade(newState, 0.01f);
            currentState = newState;
        }
    }

    private int GetState()
    {
        bool grounded = player.isGrounded;
        bool climbing = player.isClimbing;
        bool crouching = player.isCrouching;
        bool lookUp = player.LookUp;
        bool useMagnet = player.useMagnet;
        bool lookUpMag = player.LookUpMAG;
        float xVel = player.rb.linearVelocity.x;
        float yVel = player.rb.linearVelocity.y;

        if (climbing) return useMagnet ? Player_MAG_Climb : Player_Climb;

        if (crouching) return useMagnet ? Player_MAG_Crouch : Player_Crouch;

        if (!grounded)
        {
            if (yVel > 0.01f)
            {
                if (useMagnet && lookUpMag) return Player_MAG_JumpLookUp;
                if (useMagnet) return Player_MAG_Jump;
                return Player_Jump;
            }
            else if (yVel < -0.01f)
            {
                if (useMagnet && lookUpMag) return Player_MAG_FallLookUp;
                if (useMagnet) return Player_MAG_Fall;
                return Player_Fall;
            }
        }

        if (Mathf.Abs(xVel) > 0.01f)
        {
            if (useMagnet && lookUpMag) return Player_MAG_MoveLookUp;
            if (useMagnet) return Player_MAG_Move;
            return Player_Move;
        }

        if (useMagnet && lookUpMag) return Player_MAG_LookUp;
        if (useMagnet) return Player_MAG_Idle;
        if (lookUp) return Player_LookUp;

        return Player_Idle;
    }

    public void PlayHurt()
    {
        if (!isDisabled)
            StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        animator.Play(Player_Hurt);
        SFXManager.Instance.PlaySFX(SFXManager.Instance.Hurt);
        yield return new WaitForSeconds(0.5f);
    }

    public void PlayDown()
    {
        if (!isDisabled)
            StartCoroutine(DownRoutine());
    }

    private IEnumerator DownRoutine()
    {
        isDisabled = true;
        if (player != null) player.enabled = false;
        animator.Play(Player_Down);
        SFXManager.Instance.PlaySFX(SFXManager.Instance.Ouch);
        yield return new WaitForSeconds(2f);
        if (player != null) Destroy(player.gameObject);
    }

    public void PlayWin()
    {
        if (!isDisabled)
            StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        isDisabled = true;
        if (player != null) player.enabled = false;
        animator.Play(Player_Win);
        SFXManager.Instance.PlaySFX(SFXManager.Instance.Win);
        yield return new WaitForSeconds(2f);
        if (player != null) Destroy(player.gameObject);
    }
}