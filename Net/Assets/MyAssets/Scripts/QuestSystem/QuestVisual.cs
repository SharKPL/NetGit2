using TMPro;
using UnityEngine;

public class QuestVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text questName;
    [SerializeField] private TMP_Text questDescription;
    [SerializeField] private TMP_Text questLocation;
    [SerializeField] private TMP_Text questStep;

    [SerializeField] private Vector3 questCoordinates = Vector3.zero;

    public void UpdateQuest(QuestInfo quest)
    {
        if (quest == null || quest.IsCompleted())
        {
            questName.text = "Квесты закончились";
            questDescription.text = "";
            questLocation.text = "";
            questStep.text = "";
            questCoordinates = Vector3.zero;
            return;
        }
        questName.text = quest.GetQuestName();
        questDescription.text = quest.GetQuestDescription();
        questLocation.text = $"Локация: {quest.GetQuestLocation()}";
        string conditionsText = "Надо выполнить:\n";
        if (quest.GetQuestStepType() == QuestStepType.All)
        {

            foreach (var step in quest.GetAllSteps())
            {
                conditionsText += $"- {step.GetQuestStepName()} ({(step.IsCompleted() ? "Завершено" : "В процессе")})\n";
            }
            questStep.text = conditionsText;
        }
        else
        {
            conditionsText += $"- {quest.GetCurrentStep().GetQuestStepName()} ({(quest.GetCurrentStep().IsCompleted() ? "Завершено" : "В процессе")})\n";
            questStep.text = conditionsText;
        }
        questCoordinates = quest.GetQuestCoordinates();

    }
}
