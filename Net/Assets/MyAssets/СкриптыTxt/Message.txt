using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    [SerializeField] private TMP_Text msgText;

    private CustomPool<Message> poolLink;

    public void SetMsgText(string text)
    {
        msgText.text = text;
    }

    public void SetPoolLink(CustomPool<Message> link)
    {
        poolLink = link;
    }
    public void ReleaseSelf()
    {
        poolLink.Release(this);
    }
}
