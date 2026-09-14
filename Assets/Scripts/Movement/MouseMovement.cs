using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator mouseAnimator;

    [SerializeField] private AudioSource moveAudioSource;

    private Vector3 moveStart;
    private Vector3 moveTarget;
    private float moveTimer;

    private float moveDuration;
    private int direction;

    private void Start()
    {
    transform.position = new Vector3(-12.5f, 11f, 0f);

    ChooseNextTarget();

    if(moveAudioSource != null)
    {
        moveAudioSource.loop = true;
        moveAudioSource.Play();
    }
    }


private void Update()
{
    moveTimer += Time.deltaTime;

    float progress =
        moveTimer / moveDuration;

    transform.position = Vector3.Lerp(
        moveStart,
        moveTarget,
        progress
    );

    if (progress >= 1f)
    {
        direction++;

        if (direction > 3)
        {
            direction = 0;
        }

        ChooseNextTarget();
    }
}

private void ChooseNextTarget()
{
    moveStart = transform.position;
    moveTimer = 0f;

    if(direction == 0)
        {
            moveTarget = new Vector3(-12.5f, 13f, 0f);
            PlayAnimation("Mouse_Up");
        }
    else if(direction == 1)
        {
            moveTarget = new Vector3(-7.5f, 13f, 0f);
            PlayAnimation("Mouse_Right");
        }
    else if(direction == 2)
        {
            moveTarget = new Vector3(-7.5f, 9f, 0f);
            PlayAnimation("Mouse_Down");
        }
        else
        {
            moveTarget = new Vector3(-12.5f, 9f, 0f);
            PlayAnimation("Mouse_Left");
        }
   float distance = Vector3.Distance(moveStart, moveTarget);

    moveDuration = distance / moveSpeed;
}

private void PlayAnimation(string animationName)
{
    if (mouseAnimator != null)
    {
        mouseAnimator.Play(animationName);
    }

}

}
