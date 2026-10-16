using UnityEngine;

public class GhostStateController : MonoBehaviour
{
    public enum GhostState { Normal, Scared, Recovering, Dead }

    [SerializeField] private Animator ghostAnimator;
    [SerializeField] private string catId;

    public GhostState State { get; private set; } = GhostState.Normal;

    public void EnterScared()
    {
        if (State == GhostState.Dead) return;

        State = GhostState.Scared;
        ghostAnimator.Play(catId + "_Scared_Right");
    }

    public void EnterDead()
    {
        State = GhostState.Dead;
        ghostAnimator.Play(catId + "_Dead");
    }
}
