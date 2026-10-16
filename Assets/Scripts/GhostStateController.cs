using UnityEngine;

public class GhostStateController : MonoBehaviour
{
    public enum GhostState { Normal, Scared, Recovering, Dead }

    [SerializeField] private Animator ghostAnimator;
    [SerializeField] private string catId;

    public GhostState State { get; private set; } = GhostState.Normal;
    public bool CanBeEaten => State == GhostState.Scared || State == GhostState.Recovering;

    private float deathRemaining;

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
        deathRemaining = 3f;
        ghostAnimator.Play(catId + "_Dead");
    }

    public bool TickDeath(float deltaTime, float fearRemaining)
    {
        if (State != GhostState.Dead) return false;

        deathRemaining = Mathf.Max(0f, deathRemaining - deltaTime);
        if (deathRemaining > 0f) return false;

        State = GhostState.Normal;
        if (fearRemaining > 3f)
            EnterScared();
        else if (fearRemaining > 0f)
            EnterRecovering();
        else
            EnterNormal();

        return true;
    }
}
