using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfo", menuName = "Quest/QuestInfo")]
public class QuestInfo : ScriptableObject
{
    [SerializeField] private string questName;

    [TextArea(3, 10)]
    [SerializeField] private string questDescription;

    [SerializeField] private string questlocation;

    [SerializeField] private QuestStepType questStepType;
    [SerializeField] private List<QuestStep> steps;


    [SerializeField] private int stepIndex = 0;

    [SerializeField] private Vector3 quest_coordinates = Vector3.zero;

    [SerializeField] private bool isCompleted = false;



    private void OnValidate()
    {
#if UNITY_EDITOR
        questName = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void StepCompleted(string stepName)
    {

        foreach (var step in steps)
        {
            if (step.GetQuestStepName() == stepName) step.CompleteStep();
        }
        NextStep();
        CheckAllStepsCompleted();
    }

    public void CheckAllStepsCompleted()
    {
        foreach (var step in steps)
        {
            if (!step.IsCompleted()) return;
        }
        GlobalEventManager.OnQuestCompleted.Invoke();

    }

    public QuestStep GetCurrentStep()
    {
        return steps[stepIndex];
    }

    public List<QuestStep> GetAllSteps()
    {
        return steps;
    }

    public void NextStep()
    {

        if (stepIndex + 1 >= steps.Count || questStepType == QuestStepType.All) return;
        stepIndex++;
    }

    public string GetQuestName()
    {
        return questName;
    }

    public string GetQuestDescription()
    {
        return questDescription;
    }

    public string GetQuestLocation()
    {
        return questlocation;
    }

    public QuestStepType GetQuestStepType()
    {
        return questStepType;
    }

    public Vector3 GetQuestCoordinates()
    {
        return quest_coordinates;
    }

    public void CompleteQuest()
    {
        isCompleted = true;
    }

    public void QuestReset()
    {
        stepIndex = 0;
        isCompleted = false;
        foreach (var step in steps)
        {
            step.ResetStep();
        }
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }

}


public enum QuestStepType
{
    All,
    InOrder
}
