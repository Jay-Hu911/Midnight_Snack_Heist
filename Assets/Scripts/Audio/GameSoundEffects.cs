using UnityEngine;

public class GameSoundEffects : MonoBehaviour
{
    public AudioSource soundSource;

    public AudioClip mouseMove;
    public AudioClip eatCrumb;
    public AudioClip eatBonus;
    public AudioClip defeatCat;
    public AudioClip wallBump;
    public AudioClip mouseDeath;

    public void PlayMouseMove()
    {
        soundSource.PlayOneShot(mouseMove);
    }

    public void PlayEatCrumb()
    {
        soundSource.PlayOneShot(eatCrumb);
    }

    public void PlayEatBonus()
    {
        soundSource.PlayOneShot(eatBonus);
    }

    public void PlayDefeatCat()
    {
        soundSource.PlayOneShot(defeatCat);
    }

    public void PlayWallBump()
    {
        soundSource.PlayOneShot(wallBump);
    }

    public void PlayMouseDeath()
    {
        soundSource.PlayOneShot(mouseDeath);
    }
}