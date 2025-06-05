using UnityEngine;
using Mirror;

public class PlayerAudio : NetworkBehaviour
{
    public AudioSource playerAudioSource;
    private void Start()
    {
        if (isLocalPlayer)
        {
            playerAudioSource = GetComponentInChildren<AudioSource>();
        }
    }
}
