using UnityEngine;
using Mirror;

public class Item : NetworkBehaviour
{
    [SerializeField] private string itemName;

    public string ItemName { get { return itemName; } }
}
