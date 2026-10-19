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
    private float moveProgress;
    private bool isMoving;
    private bool isRoaming;
    private Vector2Int currentDirection = Vector2Int.zero;

    private Vector3[] exitRoute;
    private int exitRouteIndex;
    private bool isLeavingHouse;
    private bool isOuterPatrolling;
    private Dictionary<Vector2Int, int> stepsToOuterCorner;
    private static readonly Vector2Int OuterCornerCell = new Vector2Int(12, -13);

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
        moveProgress = 0f;
        currentDirection = Vector2Int.zero;
        isOuterPatrolling = false;
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
        if (gameManager == null || !gameManager.CanInteract || ghostState == null)
            return;

        if (ghostState.State == GhostStateController.GhostState.Dead)
        {
            MoveDeadHome();
            return;
        }

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

    private float GetCurrentSpeed()
    {
        if (ghostState.State == GhostStateController.GhostState.Normal)
            return moveSpeed;

        return moveSpeed * 0.5f;
    }

    private void MoveDeadHome()
    {
        // 返巢不走格子路线，因此也不会被墙壁阻挡。
        isMoving = false;
        isRoaming = false;
        isLeavingHouse = false;
        currentDirection = Vector2Int.zero;
        transform.position = Vector3.MoveTowards(
            transform.position, homePosition, GetCurrentSpeed() * Time.deltaTime);

        if (transform.position == homePosition)
        {
            ghostState.Revive(gameManager.FearRemaining);
            gameManager.RefreshMusic();
        }
    }

    private void AdvanceMove()
    {
        float distance = Vector3.Distance(moveStart, moveTarget);
        moveProgress += GetCurrentSpeed() * Time.deltaTime / Mathf.Max(0.01f, distance);
        transform.position = Vector3.Lerp(moveStart, moveTarget, Mathf.Clamp01(moveProgress));

        if (moveProgress >= 1f)
        {
            transform.position = moveTarget;
            isMoving = false;
        }
    }

    private void StartNextExitMove()
    {
        moveStart = transform.position;
        moveTarget = exitRoute[exitRouteIndex];
        moveProgress = 0f;
        isMoving = true;
        exitRouteIndex++;

        Vector3 difference = moveTarget - moveStart;
        if (Mathf.Abs(difference.x) > 0.01f)
            currentDirection = difference.x > 0f ? Vector2Int.right : Vector2Int.left;
        else
            currentDirection = difference.y > 0f ? Vector2Int.up : Vector2Int.down;
    }

    private List<Vector2Int> GetLegalDirections()
    {
        List<Vector2Int> choices = new List<Vector2Int>();
        Vector2Int reverse = -currentDirection;

        foreach (Vector2Int direction in Directions)
        {
            if (direction == reverse) continue;

            Vector3 next = transform.position +
                new Vector3(direction.x, direction.y, 0f);

            if (levelGenerator.CanPlayerEnter(new Vector2(next.x, next.y)))
                choices.Add(direction);
        }

        if (choices.Count == 0 && currentDirection != Vector2Int.zero)
        {
            Vector3 back = transform.position +
                new Vector3(reverse.x, reverse.y, 0f);

            if (levelGenerator.CanPlayerEnter(new Vector2(back.x, back.y)))
                choices.Add(reverse);
        }

        return choices;
    }

    private Vector2Int ChooseByDistance(List<Vector2Int> choices, bool moveAway)
    {
        if (player == null)
            return choices[Random.Range(0, choices.Count)];

        float currentDistance = (player.transform.position - transform.position).sqrMagnitude;
        List<Vector2Int> preferred = new List<Vector2Int>();

        foreach (Vector2Int direction in choices)
        {
            Vector3 next = transform.position +
                new Vector3(direction.x, direction.y, 0f);
            float nextDistance = (player.transform.position - next).sqrMagnitude;

            if (moveAway && nextDistance >= currentDistance - 0.0001f)
                preferred.Add(direction);
            else if (!moveAway && nextDistance <= currentDistance + 0.0001f)
                preferred.Add(direction);
        }

        List<Vector2Int> pool = preferred.Count > 0 ? preferred : choices;
        return pool[Random.Range(0, pool.Count)];
    }

    private Vector2Int CellKey(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x - 0.5f),
                              Mathf.RoundToInt(position.y));
    }

    private void BuildOuterDistances()
    {
        stepsToOuterCorner = new Dictionary<Vector2Int, int>();
        Queue<Vector2Int> pending = new Queue<Vector2Int>();
        stepsToOuterCorner[OuterCornerCell] = 0;
        pending.Enqueue(OuterCornerCell);

        while (pending.Count > 0)
        {
            Vector2Int cell = pending.Dequeue();
            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = cell + direction;
                if (stepsToOuterCorner.ContainsKey(next)) continue;
                Vector2 center = new Vector2(next.x + 0.5f, next.y);
                if (!levelGenerator.CanPlayerEnter(center)) continue;

                stepsToOuterCorner[next] = stepsToOuterCorner[cell] + 1;
                pending.Enqueue(next);
            }
        }
    }

    private Vector2Int ChooseClockwise(List<Vector2Int> choices)
    {
        if (stepsToOuterCorner == null)
            BuildOuterDistances();

        if (!isOuterPatrolling && CellKey(transform.position) == OuterCornerCell)
            isOuterPatrolling = true;

        if (!isOuterPatrolling)
        {
            Vector2Int best = choices[0];
            int bestSteps = int.MaxValue;
            foreach (Vector2Int direction in choices)
            {
                Vector3 next = transform.position +
                    new Vector3(direction.x, direction.y, 0f);
                if (stepsToOuterCorner.TryGetValue(CellKey(next), out int steps) &&
                    steps < bestSteps)
                {
                    best = direction;
                    bestSteps = steps;
                }
            }
            return best;
        }

        // Keep the outside wall on the ghost's left while moving clockwise.
        Vector2Int left = new Vector2Int(-currentDirection.y, currentDirection.x);
        if (choices.Contains(left)) return left;
        if (choices.Contains(currentDirection)) return currentDirection;
        Vector2Int right = new Vector2Int(currentDirection.y, -currentDirection.x);
        if (choices.Contains(right)) return right;
        return choices[0];
    }

    private void ChooseNextCell()
    {
        if (levelGenerator == null) return;

        List<Vector2Int> choices = GetLegalDirections();
        if (choices.Count == 0) return;

        bool scared = ghostState.State == GhostStateController.GhostState.Scared ||
                      ghostState.State == GhostStateController.GhostState.Recovering;
        int behavior = scared ? 1 : ghostNumber;
        if (ghostNumber == 4 && scared)
            isOuterPatrolling = false;

        Vector2Int chosen;
        if (behavior == 1)
            chosen = ChooseByDistance(choices, true);
        else if (behavior == 2)
            chosen = ChooseByDistance(choices, false);
        else if (behavior == 4)
            chosen = ChooseClockwise(choices);
        else
            chosen = choices[Random.Range(0, choices.Count)];

        moveStart = transform.position;
        moveTarget = moveStart + new Vector3(chosen.x, chosen.y, 0f);
        moveProgress = 0f;
        currentDirection = chosen;
        isMoving = true;
    }
}
