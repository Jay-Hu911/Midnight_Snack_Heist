using UnityEngine;

[RequireComponent(typeof(GhostStateController))]
public class GhostController : MonoBehaviour
{
    [SerializeField, Range(1, 4)] private int ghostNumber;
    [SerializeField] private Animator ghostAnimator;
    [SerializeField] private GhostStateController ghostState;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private PacStudentController player;
    [SerializeField] private GameManager gameManager;

    private Vector3 homePosition;

    public int GhostNumber => ghostNumber;
    public Vector3 HomePosition => homePosition;

    private void Awake()
    {
        homePosition = transform.position;
    }
}
