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

    [SerializeField] private float moveSpeed = 4.5f;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    private Vector3 homePosition;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private float moveElapsed;
    private bool isMoving;
    private bool isRoaming;

    public int GhostNumber => ghostNumber;
    public Vector3 HomePosition => homePosition;

    public void BeginRoaming()
    {
        isRoaming = true;
    }

    private void Awake()
    {
        homePosition = transform.position;
    }

    private void Update()
    {
        if (!isRoaming || gameManager == null || !gameManager.CanInteract ||
            ghostState == null || ghostState.State == GhostStateController.GhostState.Dead)
            return;

        if (isMoving)
        {
            AdvanceMove();
            if (isMoving) return;
        }

        ChooseNextCell();
    }

    private void AdvanceMove()
    {
        moveElapsed += Time.deltaTime;
        float duration = Vector3.Distance(moveStart, moveTarget) / Mathf.Max(0.01f, moveSpeed);
        float progress = Mathf.Clamp01(moveElapsed / duration);
        transform.position = Vector3.Lerp(moveStart, moveTarget, progress);

        if (progress >= 1f)
        {
            transform.position = moveTarget;
            isMoving = false;
        }
    }

    private void ChooseNextCell()
    {
        if (levelGenerator == null) return;

        foreach (Vector2Int direction in Directions)
        {
            Vector3 next = transform.position + new Vector3(direction.x, direction.y, 0f);
            if (!levelGenerator.CanPlayerEnter(new Vector2(next.x, next.y)))
                continue;

            moveStart = transform.position;
            moveTarget = next;
            moveElapsed = 0f;
            isMoving = true;
            return;
        }
    }
}
