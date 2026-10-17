using UnityEngine;
using System.Collections.Generic;

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
    private Vector2Int currentDirection = Vector2Int.zero;

    private Vector3[] exitRoute;
    private int exitRouteIndex;
    private bool isLeavingHouse;

    public int GhostNumber => ghostNumber;
    public Vector3 HomePosition => homePosition;

    public void BeginRoaming()
    {
        isRoaming = true;

        // 测试时幽灵也可能被放在普通走廊；只有在出生点才执行离巢路线。
        if ((transform.position - homePosition).sqrMagnitude > 0.01f)
            return;

        BuildExitRoute();
        exitRouteIndex = 0;
        isLeavingHouse = true;
        currentDirection = Vector2Int.zero;
    }

    public void ResetMovementForRound()
    {
        isMoving = false;
        isRoaming = false;
        isLeavingHouse = false;
        exitRouteIndex = 0;
        exitRoute = null;
        moveElapsed = 0f;
        currentDirection = Vector2Int.zero;
    }

    private void BuildExitRoute()
    {
        float side = ghostNumber == 1 || ghostNumber == 3 ? 1f : -1f;
        float gateX = Mathf.Sign(homePosition.x) * 0.5f;
        float laneX = Mathf.Sign(homePosition.x) * 1.5f;
        float firstY = side > 0f ? 1f : 0f;
        float z = homePosition.z;

        List<Vector3> route = new List<Vector3>();
        route.Add(new Vector3(homePosition.x, firstY, z));

        if (Mathf.Abs(homePosition.x - gateX) > 0.01f)
            route.Add(new Vector3(gateX, firstY, z));

        if (side < 0f)
            route.Add(new Vector3(gateX, -1f, z));

        route.Add(new Vector3(gateX, side * 2f, z)); // Ghost Wall
        route.Add(new Vector3(gateX, side * 3f, z));
        route.Add(new Vector3(laneX, side * 3f, z));
        route.Add(new Vector3(laneX, side * 4f, z));
        route.Add(new Vector3(laneX, side * 5f, z));
        route.Add(new Vector3(laneX, side * 6f, z)); // 普通走廊

        exitRoute = route.ToArray();
    }
    private void Awake()
    {
        homePosition = transform.position;
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.CanInteract ||
        ghostState == null || ghostState.State == GhostStateController.GhostState.Dead)
        return;

        if (!isRoaming)
            BeginRoaming();

        if (isMoving)
        {
            AdvanceMove();
            if (isMoving) return;
        }

        if (isLeavingHouse)
        {
            if (exitRouteIndex < exitRoute.Length)
            {
                StartNextExitMove();
                return;
            }

            isLeavingHouse = false;
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
    private void StartNextExitMove()
    {
        moveStart = transform.position;
        moveTarget = exitRoute[exitRouteIndex];
        moveElapsed = 0f;
        isMoving = true;
        exitRouteIndex++;

        Vector3 difference = moveTarget - moveStart;
        if (Mathf.Abs(difference.x) > 0.01f)
            currentDirection = difference.x > 0f ? Vector2Int.right : Vector2Int.left;
        else
            currentDirection = difference.y > 0f ? Vector2Int.up : Vector2Int.down;
    }
    private void ChooseNextCell()
    {
        if (levelGenerator == null) return;

        Vector2Int reverse = -currentDirection;
        Vector2Int chosen = Vector2Int.zero;

        foreach (Vector2Int direction in Directions)
        {
            if (direction == reverse) continue;

            Vector3 next = transform.position +
                new Vector3(direction.x, direction.y, 0f);

            if (levelGenerator.CanPlayerEnter(new Vector2(next.x, next.y)))
            {
                chosen = direction;
                break;
            }
        }

        if (chosen == Vector2Int.zero && currentDirection != Vector2Int.zero)
        {
            Vector3 back = transform.position +
                new Vector3(reverse.x, reverse.y, 0f);

            if (levelGenerator.CanPlayerEnter(new Vector2(back.x, back.y)))
                chosen = reverse;
        }

        if (chosen == Vector2Int.zero) return;

        moveStart = transform.position;
        moveTarget = moveStart + new Vector3(chosen.x, chosen.y, 0f);
        moveElapsed = 0f;
        currentDirection = chosen;
        isMoving = true;
    }
}
