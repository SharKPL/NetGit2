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
        if (!isLocalPlayer) return;
        if (instance == null)
        {
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
    public void AddItem(Item item)
    {
        Debug.Log($"Add {item.ItemName}");
        if (itemList.ContainsKey(item.ItemName))
        {
            itemList[item.ItemName].Add(item);
        }
        else
        {
            List<Item> list = new List<Item>();
            list.Add(item);
            itemList.Add(item.ItemName, list);
        }
        GlobalEventManager.TakeItemEvent?.Invoke(item.ItemName);
        item.gameObject.transform.SetParent(transform);
        item.gameObject.transform.transform.position = transform.position;
        item.gameObject.SetActive(false);
    }



    [TargetRpc]
    private void TargetAddItem(NetworkConnection target,Item item)
    {
        
        Debug.Log($"Add {item.ItemName}");
        if (itemList.ContainsKey(item.ItemName))
        {
            itemList[item.ItemName].Add(item);
            Debug.Log($"��������{item.ItemName}");
        }
        else
        {
            Debug.Log($"����������{item.ItemName}");
            List<Item> list = new List<Item>();
            list.Add(item);
            itemList.Add(item.ItemName, list);
        }
        


        GlobalEventManager.TakeItemEvent?.Invoke(item.ItemName);
    }

    [Command(requiresAuthority = false)]
    public void CmdAddItem(Item item)
    {
        Debug.LogError("ni");
        item.gameObject.transform.SetParent(transform);
        item.gameObject.transform.transform.position = transform.position;
        item.gameObject.SetActive(false);
        TargetAddItem(connectionToClient,item);
    }

    [TargetRpc]
    private void TargetRemoveItem(NetworkConnection target,string itemName, bool active)
    {

        itemList[itemName].RemoveAt(0);
        if (itemList[itemName].Count == 0)
        {
            itemList.Remove(itemName);
            Debug.Log($"Remove {itemName}");
        }


        GlobalEventManager.UpdateInventoryUI?.Invoke();

    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveItem(string itemName, bool active)
    {
        Debug.LogError($"connectionToClient null:{connectionToClient is null}");
        if (!itemList.ContainsKey(itemName)) return;
        itemList[itemName][0].transform.parent = null;
        itemList[itemName][0].gameObject.SetActive(active);
        TargetRemoveItem(connectionToClient,itemName, active);
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
