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


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        itemList = new Dictionary<string, List<Item>>();
    }
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



    [ClientRpc]
    private void RpcAddItem(Item item)
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
        item.gameObject.transform.SetParent(transform);
        item.gameObject.transform.transform.position = transform.position;
        item.gameObject.SetActive(false);
        if (!isLocalPlayer) return;
        GlobalEventManager.TakeItemEvent?.Invoke(item.ItemName);
    }

    [Command]
    public void CmdAddItem(Item item)
    {
        RpcAddItem(item);
    }

    [ClientRpc]
    private void RpcRemoveItem(string itemName)
    {
        if (!itemList.ContainsKey(itemName)) return;
        itemList[itemName][0].transform.parent = null;
        itemList[itemName][0].gameObject.SetActive(true);
        itemList[itemName].RemoveAt(0);
        if (itemList[itemName].Count > 0) return;
        itemList.Remove(itemName);
        Debug.Log($"Remove {itemName}");
    }

    [Command]
    public void CmdRemoveItem(string itemName)
    {
        RpcRemoveItem(itemName);
    }

    public void RemoveItem(string itemName)
    {
        if (!itemList.ContainsKey(itemName)) return;
        itemList[itemName][0].transform.parent = null;
        itemList[itemName][0].gameObject.SetActive(true);
        itemList[itemName].RemoveAt(0);
        if (itemList[itemName].Count > 0) return;
        itemList.Remove(itemName);
        Debug.Log($"Remove {itemName}");

    }

    public int GetItemCount(string name)
    {
        if (itemList.ContainsKey(name))
        {
            return itemList[name].Count;
        }
        return 0;
    }
}
