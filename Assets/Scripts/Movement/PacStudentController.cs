using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private PlayerInteractionController playerInteraction;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private ParticleSystem moveParticles;

    [SerializeField] private Transform manualLevel;
    [SerializeField] private AudioClip foodMoveClip;

    [SerializeField] private GameSoundEffects soundEffects;
    [SerializeField] private ParticleSystem wallBumpParticles;
    private bool wallBumpPlayed;



    private AudioClip normalMoveClip;
    private readonly Dictionary<Vector2Int, Transform> foodsByCell = new();

    [SerializeField] private float moveSpeed = 5f;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private float moveElapsed;
    private float moveDuration;
    private bool isMoving;
    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;
    private Vector3 startingPosition;

    public bool CanMove { get; private set; } = true;
    public Vector2Int FacingDirection { get; private set; } = Vector2Int.right;

    private void Awake()
    {
        startingPosition = transform.position;
    }

    private void Start()
    {
        if (manualLevel != null)
    {
        foreach (SpriteRenderer sprite in manualLevel.GetComponentsInChildren<SpriteRenderer>())
        {
            string objectName = sprite.gameObject.name;
            if (!objectName.EndsWith("_Type05") && !objectName.EndsWith("_Type06"))
                continue;

            foodsByCell[CellKey(sprite.transform.position)] = sprite.transform;
        }
    }

        if (moveAudioSource != null)
        {
            normalMoveClip = moveAudioSource.clip;
            moveAudioSource.loop = true;
            moveAudioSource.Stop();
        }

       if(playerAnimator == null){
        return;
       }
       playerAnimator.Play("Mouse_Right",0,0f);
       playerAnimator.speed = 0f;

       if (moveParticles != null)
        moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (!CanMove) return;

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

    private void PlayMovementAnimation(Vector2Int direction)
    {
        if (playerAnimator == null) return;

        string stateName;
        if (direction == Vector2Int.up)
            stateName = "Mouse_Up";
        else if (direction == Vector2Int.down)
            stateName = "Mouse_Down";
        else if (direction == Vector2Int.left)
            stateName = "Mouse_Left";
        else
            stateName = "Mouse_Right";

        if (direction != currentInput || playerAnimator.speed == 0f)
            playerAnimator.Play(stateName, 0, 0f);

        playerAnimator.speed = 1f;
    }
    
    private bool TryStartMove(Vector2Int direction)
    {
        if (isMoving || direction == Vector2Int.zero || levelGenerator == null)
            return false;

        Vector3 start = transform.position;

        if (levelGenerator.IsTunnelExit(new Vector2(start.x, start.y), direction))
        {
            start.x = -start.x;
        }

        Vector3 target = start + new Vector3(direction.x, direction.y, 0f);
        if (!levelGenerator.CanPlayerEnter(new Vector2(target.x, target.y)))
            return false;
        if (playerInteraction != null)
        playerInteraction.CollectAt(transform.position);

        transform.position = start;
        moveStart = start;

        moveTarget = target;
        moveElapsed = 0f;
        moveDuration = Vector3.Distance(moveStart, moveTarget) / moveSpeed;
        PlayMovementAnimation(direction);
        PlayMovementSound(target);
        if (moveParticles != null && !moveParticles.isEmitting)
        moveParticles.Play();
        currentInput = direction;
        FacingDirection = direction;
        isMoving = true;
        wallBumpPlayed = false;
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

            if (playerInteraction != null)
            {
            playerInteraction.CollectAt(moveTarget);
            }
        }
    }

    private void DecideNextMove()
    {
        if(isMoving||lastInput == Vector2Int.zero)
        {
            return;
        }
        if(TryStartMove(lastInput))
        {
            return;
        }    
        if (TryStartMove(currentInput))
        {
            return;
        }
        Vector2Int bumpDirection =
        currentInput != Vector2Int.zero ? currentInput : lastInput;
        PlayWallBump(bumpDirection);
        if (playerAnimator != null)
        {
            playerAnimator.speed = 0f;
        }
        currentInput = Vector2Int.zero;

        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
        }
        if (moveParticles != null)
        {
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void PlayWallBump(Vector2Int direction)
    {
        if (wallBumpPlayed || direction == Vector2Int.zero)
            return;

        wallBumpPlayed = true;

        if (soundEffects != null)
            soundEffects.PlayWallBump();

        if (wallBumpParticles != null)
        {
            wallBumpParticles.transform.localPosition =
                new Vector3(direction.x, direction.y, 0f) * 0.45f;
            wallBumpParticles.Emit(8);
        }
    }

    private Vector2Int CellKey(Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x * 2f),
            Mathf.RoundToInt(position.y * 2f)
        );
    }

    private void PlayMovementSound(Vector3 target)
    {
        if (moveAudioSource == null) return;

        bool hasFood = foodsByCell.TryGetValue(CellKey(target), out Transform food)
                    && food != null
                    && food.gameObject.activeInHierarchy;

        AudioClip nextClip = hasFood ? foodMoveClip : normalMoveClip;
        if (nextClip == null) return;

        if (moveAudioSource.clip != nextClip)
        {
            moveAudioSource.clip = nextClip;
            moveAudioSource.Play();
        }
        else if (!moveAudioSource.isPlaying)
        {
            moveAudioSource.Play();
        }
    }

    public void BeginDeath()
    {
        SetGameplayEnabled(false);

        if (wallBumpParticles != null)
        {
            wallBumpParticles.transform.localPosition = Vector3.zero;
            wallBumpParticles.Emit(16);
        }

        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.Play("Mouse_Dead", 0, 0f);
        }
    }

    public void SetGameplayEnabled(bool enabled)
    {
        CanMove = enabled;
        if (enabled) return;

        isMoving = false;
        if (moveAudioSource != null) moveAudioSource.Stop();
        if (moveParticles != null)
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (playerAnimator != null) playerAnimator.speed = 0f;
    }

    public void ResetAfterDeath()
    {
        transform.position = startingPosition;
        moveStart = startingPosition;
        moveTarget = startingPosition;
        moveElapsed = 0f;
        moveDuration = 0f;
        isMoving = false;
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;
        FacingDirection = Vector2Int.right;
        wallBumpPlayed = false;

        if (moveAudioSource != null) moveAudioSource.Stop();
        if (moveParticles != null)
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (wallBumpParticles != null)
            wallBumpParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (playerAnimator != null)
        {
            playerAnimator.Play("Mouse_Right", 0, 0f);
            playerAnimator.speed = 0f;
        }

        CanMove = true;
    }
}
