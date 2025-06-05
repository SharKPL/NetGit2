using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    private float currentHealth;
    [SerializeField]
    private float maxHealth = 100f;
    public System.Action OnDeath;
    [SerializeField]
    private float respawnDelay = 3f;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            CmdTakeDamage(20);
        }
    }
    [Command(requiresAuthority =false)]
    public void CmdTakeDamage(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            RpcDie();
            Transform startPos = NetworkManager.singleton.GetStartPosition();
            if (startPos != null)
            {
                transform.position = startPos.position;
            }
            else
            {
                transform.position = Vector3.zero;
            }
            GetComponent<CharacterInput>()?.ResetMovement();
            currentHealth = maxHealth;
        }
    }

    void OnHealthChanged(float oldHealth, float newHealth)
    {
        
        if (netIdentity.isClient)
        {
            GlobalEventManager.PlayerHealthChanged?.Invoke(newHealth / maxHealth);
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (OnDeath != null)
            OnDeath.Invoke();
        Debug.Log($"Player {netId} died");
    }


    public void Heal(float amount)
    {
        if (!isServer) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
