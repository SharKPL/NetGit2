using UnityEngine;

[CreateAssetMenu(fileName = "QuestStep", menuName = "Quest/QuestStep")]
public class QuestStep : ScriptableObject
{
    [SerializeField] private string stepName;

    [SerializeField] private bool isCompleted = false;



    public string GetQuestStepName()
    {
        return stepName;
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }

    public void CompleteStep()
    {
        isCompleted = true;
    }

    public void ResetStep()
    {
        isCompleted = false;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        stepName = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

}
