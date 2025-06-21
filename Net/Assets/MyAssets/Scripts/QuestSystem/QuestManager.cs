using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]


public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestVisual questView;

    [SerializeField] private MarkerController markerController;
    [SerializeField] private List<QuestInfo> quests;


    private Dictionary<string, GameObject> questTriggersDict = new Dictionary<string, GameObject>();

    private int questIndex = 0;

    void Start()
    {
        Debug.LogError("QuestManagerStart");
        QuestReset();

        GlobalEventManager.OnQuestCompleted.AddListener(CompleteQuest);
        GlobalEventManager.OnQuestStepCompleted.AddListener(StepCompleted);

        GameObject[] questTriggers = GameObject.FindGameObjectsWithTag("QuestTrigger");
        foreach (var questTrigger in questTriggers)
        {
            questTriggersDict.Add(questTrigger.GetComponent<QuestTrigger>().StepName, questTrigger);
            questTrigger.SetActive(false);
        }


        UpdateQuestView();
    }

    public void QuestReset()
    {
        questIndex = 0;
        foreach (var quest in quests)
        {
            quest.QuestReset();
        }
    }

    public QuestInfo GetCurrentQuest()
    {
        if (questIndex >= quests.Count) return null;
        return quests[questIndex];
    }

    public void CompleteQuest()
    {
        GetCurrentQuest().CompleteQuest();
        if (questIndex >= quests.Count)
        {
            UpdateQuestView();
            return;
        }

        questIndex++;
        UpdateQuestView();
    }

    private void UpdateQuestView()
    {
        if (GetCurrentQuest() == null)
        {
            questView.UpdateQuest(GetCurrentQuest());
            return;
        }

        if (GetCurrentQuest().GetQuestStepType() == QuestStepType.All)
        {
            foreach (var queststep in GetCurrentQuest().GetAllSteps())
            {
                string stepName = queststep.GetQuestStepName();
                
                if (questTriggersDict.ContainsKey(stepName))
                {
                    var trigger = questTriggersDict[stepName];
                    if (!queststep.IsCompleted())
                    {
                        //markerController.GetMarker(stepName, trigger.transform);
                        trigger.SetActive(true);
                    }
                }

                // else{
                //     markerController.GetMarker(stepName, trigger.transform);
                //     trigger.SetActive(true);
                // }
            }

        }
        else
        {

            var queststep = GetCurrentQuest().GetCurrentStep();
            Debug.Log($"QuestStepIsCompleted:{queststep.IsCompleted()}");
            string stepName = queststep.GetQuestStepName();
            var trigger = questTriggersDict[stepName];
            if (!queststep.IsCompleted())
            {
                //markerController.GetMarker(stepName, trigger.transform);
                trigger.SetActive(true);
            }
            // else{
            //     markerController.GetMarker(stepName, trigger.transform);
            //     trigger.SetActive(true);
            // }

        }



        questView.UpdateQuest(GetCurrentQuest());
    }


    private void StepCompleted(string stepName)
    {
        if (questIndex >= quests.Count) return;
        if (questTriggersDict.ContainsKey(stepName))
        {
            questTriggersDict[stepName].SetActive(false);
        }

        GetCurrentQuest().StepCompleted(stepName);
        //markerController.ReturnMarker(stepName);
        UpdateQuestView();
    }

}
