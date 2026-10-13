using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AudioSource moveAudioSource;

    [SerializeField] private float moveSpeed = 5f;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private float moveElapsed;
    private float moveDuration;
    private bool isMoving;
    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    private void Update()
    {   
        AdvanceMove();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.wasPressedThisFrame)
            lastInput = Vector2Int.up;
        else if (keyboard.aKey.wasPressedThisFrame)
            lastInput = Vector2Int.left;
        else if (keyboard.sKey.wasPressedThisFrame)
            lastInput = Vector2Int.down;
        else if (keyboard.dKey.wasPressedThisFrame)
            lastInput = Vector2Int.right;

        DecideNextMove();
    }
    
    private bool TryStartMove(Vector2Int direction)
    {
        if (isMoving || direction == Vector2Int.zero || levelGenerator == null)
            return false;

        Vector3 target = transform.position + new Vector3(direction.x, direction.y, 0f);
        if (!levelGenerator.CanPlayerEnter(new Vector2(target.x, target.y)))
            return false;

        moveStart = transform.position;
        moveTarget = target;
        moveElapsed = 0f;
        moveDuration = Vector3.Distance(moveStart, moveTarget) / moveSpeed;
        currentInput = direction;
        isMoving = true;
        return true;
    }

    private void AdvanceMove()
    {
        if (!isMoving) return;

        moveElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(moveElapsed / moveDuration);
        transform.position = Vector3.Lerp(moveStart, moveTarget, progress);

        if (progress >= 1f)
        {
            transform.position = moveTarget;
            isMoving = false;
        }
    }

    private void DecideNextMove()
    {
        if(isMoving||lastInput == Vector2Int.zero)
            return;
        if(TryStartMove(lastInput))
            return;
        if(TryStartMove(currentInput))
            return;
        currentInput = Vector2Int.zero;
    }
}
