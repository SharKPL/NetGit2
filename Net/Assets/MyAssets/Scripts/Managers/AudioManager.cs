using Mirror;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    public AudioManager Instance;


    public static void PlaySound(AudioSource source, AudioClip clip)
    {
        Debug.Log("PlaySound");
        source.clip = clip;
        source.PlayOneShot(clip);
    }


    public static void PlayLoopSound(AudioSource source, AudioClip clip)
    {
        Debug.Log("PlaySound");
        source.clip = clip;
        source.Play();
    }

}
