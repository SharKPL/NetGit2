using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip clip;

    private void Start()
    {
        audioSource.loop = true;
        AudioManager.PlayLoopSound(audioSource, clip);
    }
}
