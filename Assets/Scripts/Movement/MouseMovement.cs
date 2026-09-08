using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator mouseAnimator;
    [SerializeField] private AudioSource moveAudioSource;

    private readonly Vector3 startPosition = new Vector3(-12.5f, 11f, 0f);

    private readonly Vector3[] waypoints =
    {
        new Vector3(-12.5f, 13f, 0f),
        new Vector3(-7.5f, 13f, 0f),
        new Vector3(-7.5f, 9f, 0f),
        new Vector3(-12.5f, 9f, 0f)
    };

    private int targetIndex;
    private Vector3 sectionStart;
    private Vector3 sectionEnd;
    private float sectionTime;
    private float sectionDuration;

    private void Start()
    {
        transform.position = startPosition;
        targetIndex = 0;
        BeginSection();

        if (moveAudioSource != null && moveAudioSource.clip != null)
        {
            moveAudioSource.loop = true;
            moveAudioSource.Play();
        }
    }

    private void Update()
    {
        sectionTime += Time.deltaTime;

        float progress = Mathf.Clamp01(sectionTime / sectionDuration);
        transform.position = Vector3.Lerp(sectionStart, sectionEnd, progress);

        if (progress >= 1f)
        {
            transform.position = sectionEnd;
            targetIndex = (targetIndex + 1) % waypoints.Length;
            BeginSection();
        }
    }

    private void BeginSection()
    {
        sectionStart = transform.position;
        sectionEnd = waypoints[targetIndex];

        float distance = Vector3.Distance(sectionStart, sectionEnd);
        sectionDuration = distance / moveSpeed;
        sectionTime = 0f;

        PlayDirectionAnimation(sectionEnd - sectionStart);
    }

    private void PlayDirectionAnimation(Vector3 direction)
    {
        if (mouseAnimator == null)
        {
            return;
        }

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            mouseAnimator.Play(direction.x > 0f ? "Mouse_Right" : "Mouse_Left");
        }
        else
        {
            mouseAnimator.Play(direction.y > 0f ? "Mouse_Up" : "Mouse_Down");
        }
    }

    public void Configure(Animator animatorComponent, AudioSource audioSourceComponent)
    {
        mouseAnimator = animatorComponent;
        moveAudioSource = audioSourceComponent;
    }
}
