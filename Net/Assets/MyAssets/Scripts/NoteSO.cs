using UnityEngine;

[CreateAssetMenu(fileName = "NoteSO", menuName = "Notes/NoteSO")]
public class NoteSO : ScriptableObject
{
    [TextArea(4,4)] public string text;
}
