using UnityEngine;

public class GhostStateController : MonoBehaviour
{
    public enum GhostState { Normal, Scared, Recovering, Dead }

    [SerializeField] private Animator ghostAnimator;
    [SerializeField] private string catId;

    public GhostState State { get; private set; } = GhostState.Normal;
    public bool CanBeEaten => State == GhostState.Scared || State == GhostState.Recovering;

    private Vector3 startingPosition;

    private void Awake()
    {
        startingPosition = transform.position;
    }

    public void SetPaused(bool paused)
    {
        if (ghostAnimator != null)
            ghostAnimator.speed = paused ? 0f : 1f;
    }

    public void ResetForRound()
    {
        transform.position = startingPosition;
        State = GhostState.Normal;
        SetPaused(false);
        if (ghostAnimator != null)
            ghostAnimator.Play(catId + "_Normal_Right", 0, 0f);
    }

    public void EnterNormal()
    {
        if (State == GhostState.Dead) return;

        State = GhostState.Normal;
        ghostAnimator.Play(catId + "_Normal_Right");
    }

    public void EnterScared()
    {
        if (State == GhostState.Dead) return;

        State = GhostState.Scared;
        ghostAnimator.Play(catId + "_Scared_Right");
    }

    public void EnterRecovering()
    {
        if (State == GhostState.Dead) return;

        State = GhostState.Recovering;
        ghostAnimator.Play(catId + "_Recovering");
    }

    public void EnterDead()
    {
        if (State == GhostState.Dead) return;

        State = GhostState.Dead;
        ghostAnimator.Play(catId + "_Dead");
    }

    public void Revive(float fearRemaining)
    {
        if (State != GhostState.Dead) return;

        State = GhostState.Normal;

        if (fearRemaining > 3f)
            EnterScared();
        else if (fearRemaining > 0f)
            EnterRecovering();
        else
            EnterNormal();
    }
}
