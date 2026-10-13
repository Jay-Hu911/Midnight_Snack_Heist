using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AudioSource moveAudioSource;

    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    private void Update()
    {
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
    }
    
}
