using System.Collections;
using UnityEngine;

public class RecreatedLevelMusicController : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip introMusic;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip defeatedMusic;

    private IEnumerator Start()
    {
        musicSource.clip = introMusic;
        musicSource.loop = false;
        musicSource.Play();

        float introTime = Mathf.Min(introMusic.length, 3f);
        yield return new WaitForSeconds(introTime);

        PlayNormalMusic();
    }

    public void PlayIntroMusic()
    {
        musicSource.Stop();
        musicSource.clip = introMusic;
        musicSource.loop = false;
        musicSource.Play();
    }

    public void PlayNormalMusic()
    {
        musicSource.Stop();
        musicSource.clip = normalMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayScaredMusic()
    {
        musicSource.Stop();
        musicSource.clip = scaredMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayDefeatedMusic()
    {
        musicSource.Stop();
        musicSource.clip = defeatedMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

}
