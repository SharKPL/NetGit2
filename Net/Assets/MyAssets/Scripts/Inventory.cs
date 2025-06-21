using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class Inventory : NetworkBehaviour
{
    private static Inventory instance;
    public static Inventory Instance { get { return instance; } }



    private Dictionary<string,List<Item>> itemList;

    public Dictionary<string, List<Item>> ItemList { get { return itemList; } }

    public override void OnStartClient()
    {
        base.OnStartClient();
        itemList = new Dictionary<string, List<Item>>();

        if (isLocalPlayer)
        {
            if (instance == null)
                instance = this;
        }
    }
    //private void Awake()
    //{
    //    if (instance == null)
    //    {
    //        instance = this;
    //    }
    //}

    //private void Start()
    //{
    //    itemList = new Dictionary<string, List<Item>>();
    //}
    //public void AddItem(Item item)
    //{
    //    Debug.Log($"Add {item.ItemName}");
    //    if (itemList.ContainsKey(item.ItemName))
    //    {
    //        itemList[item.ItemName].Add(item);
    //    }
    //    else
    //    {
    //        List<Item> list = new List<Item>();
    //        list.Add(item);
    //        itemList.Add(item.ItemName, list);
    //    }
    //    GlobalEventManager.TakeItemEvent?.Invoke(item.ItemName);
    //    item.gameObject.transform.SetParent(transform);
    //    item.gameObject.transform.transform.position = transform.position;
    //    item.gameObject.SetActive(false);
    //}



    [ClientRpc]
    private void RpcAddItem(uint itemNetId, string itemName)
    {
        // У хоста уже добавлено на сервере
        //if (isServer && isLocalPlayer)
        //    return;
        if (!NetworkClient.spawned.TryGetValue(itemNetId, out NetworkIdentity identity))
        {
            Debug.LogWarning($"[CLIENT] Item with netId {itemNetId} not found.");
            return;
        }

        Item item = identity.GetComponent<Item>();
        if (item == null) return;

        // Добавить в инвентарь
        if (!itemList.ContainsKey(itemName))
            itemList[itemName] = new List<Item>();
        itemList[itemName].Add(item);

        item.transform.SetParent(transform);
        item.transform.position = transform.position;
        item.gameObject.SetActive(false);

        if (isLocalPlayer)
            GlobalEventManager.TakeItemEvent?.Invoke(itemName);
    }

    [Command(requiresAuthority = false)]
    public void CmdAddItem(Item item)
    {
        if (!item.TryGetComponent(out NetworkIdentity netIdentity))
        {
            Debug.LogError("Item has no NetworkIdentity!");
            return;
        }

        item.transform.SetParent(transform);
        item.transform.position = transform.position;
        item.gameObject.SetActive(false);

        // Добавить на сервере для хоста
        //if (!itemList.ContainsKey(item.ItemName))
        //    itemList[item.ItemName] = new List<Item>();
        //itemList[item.ItemName].Add(item);

        RpcAddItem(netIdentity.netId, item.ItemName);
    }

    [ClientRpc]
    private void RpcRemoveItem(uint itemNetId, string itemName, bool active)
    {

        if (!NetworkClient.spawned.TryGetValue(itemNetId, out NetworkIdentity identity))
        {
            Debug.LogWarning($"[CLIENT] Item with netId {itemNetId} not found.");
            return;
        }

        Item item = identity.GetComponent<Item>();
        if (item == null) return;

        if (itemList.ContainsKey(itemName))
        {
            itemList[itemName].Remove(item);
            if (itemList[itemName].Count == 0)
                itemList.Remove(itemName);
        }

        item.transform.parent = null;
        item.gameObject.SetActive(active);

        if (isLocalPlayer)
            GlobalEventManager.UpdateInventoryUI?.Invoke();
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveItem(string itemName, bool active)
    {
        if (!itemList.ContainsKey(itemName) || itemList[itemName].Count == 0)
            return;

        Item item = itemList[itemName][0];
        itemList[itemName].RemoveAt(0);
        if (itemList[itemName].Count == 0)
            itemList.Remove(itemName);

        item.transform.parent = null;
        item.gameObject.SetActive(active);

        RpcRemoveItem(item.netIdentity.netId, itemName, active);
    }


    public int GetItemCount(string name)
    {
        if (itemList.ContainsKey(name))
        {
            return itemList[name].Count;
        }
        return 0;
    }

    public bool TryGetItem(string name)
    {

        return itemList.ContainsKey(name);
    }
}
