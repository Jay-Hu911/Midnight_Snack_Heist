using UnityEngine;

public class RecreatedLevelMusicController : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip introMusic;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip defeatedMusic;

    private void Start()
    {
        PlayIntroMusic();
    }

    public void PlayIntroMusic()
    {
        PlayClip(introMusic, false);
    }

    public void PlayNormalMusic()
    {
        PlayClip(normalMusic, true);
    }

    public void PlayScaredMusic()
    {
        PlayClip(scaredMusic, true);
    }

    public void PlayDefeatedMusic()
    {
        PlayClip(defeatedMusic, true);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private void PlayClip(AudioClip clip, bool loop)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.isPlaying && musicSource.clip == clip && musicSource.loop == loop) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

}
