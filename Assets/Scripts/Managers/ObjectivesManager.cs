using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using NaughtyAttributes;
using InnominatumDigital.Base;

public class ObjectivesManager : SingletonBase<ObjectivesManager>
{
    public List<Objective> ObjectivesList;
    public ObjectiveTag CurrentActiveObjectiveTag;
    public List<ObjectiveTag> CompletedObjectives;

    [Header("References")]
    [SerializeField] private List<Objective> _objectivesAssets;

    [SerializeField] private GameObject _newObjectivePopup;
    [SerializeField] private TextMeshProUGUI _newObjectiveDescriptionText;
      
    [SerializeField] private GameObject _currentObjectivePopup;
    [SerializeField] private TextMeshProUGUI _currentObjectiveDescriptionText;

    private bool isPopupPlaying = false;

    private void Start()
    {
        InitializeObjectives();
    }

    private void InitializeObjectives()
    {
        CompletedObjectives = SaveManager.Instance.CompletedObjectives;

        ObjectivesList = new List<Objective>();
        foreach (var objectiveAsset in _objectivesAssets)
        {
            Objective clonedObjective = Instantiate(objectiveAsset);                                                 
            if (CompletedObjectives.Contains(clonedObjective.objectiveTag))
                clonedObjective.isCompleted = true;
            ObjectivesList.Add(clonedObjective);
        }
    }

    public void UpdateObjectiveProgress(ObjectiveTag objective, int progressValue)
    {
        GetObjective(objective)?.UpdateProgress(progressValue);
    }

    public Objective GetObjective(ObjectiveTag objectiveTag)
    {
        return ObjectivesList.FirstOrDefault(a => a.objectiveTag == objectiveTag);
    }

    public Objective GetCurrentObjective()
    {
        return GetObjective(CurrentActiveObjectiveTag);
    }

    public void SetNewActiveObjective(ObjectiveTag objectiveTag, bool shouldShowPopup = true)
    {
        CurrentActiveObjectiveTag = objectiveTag;
        ObjectivesList.FirstOrDefault(a => a.objectiveTag == objectiveTag).isActive = true;
        UpdateActiveObjectiveDisplayInformation();
        UpdateNewObjectiveDisplayInformation();
        if(shouldShowPopup)
            ShowNewObjectivePopup();
    }

    public void UpdateActiveObjectiveDisplayInformation()
    {
        string progressText = "";
        if (GetCurrentObjective().targetedProgress > 1)
            progressText = $" ({GetCurrentObjective().currentProgress}/{GetCurrentObjective().currentProgress})";
        _currentObjectiveDescriptionText.text = $"{GetCurrentObjective().taskDescription}{progressText}";
    }

    public void UpdateNewObjectiveDisplayInformation()
    {
        _newObjectiveDescriptionText.text = GetObjective(CurrentActiveObjectiveTag).taskDescription;
    }

    public async void ShowObjectivePopup()
    {
        if (isPopupPlaying)
            return;

        isPopupPlaying = true;
        ScreenManager.Instance.ShowPopup("ObjectivePopup", hasOpeningAnimation: true, popupGameObject: _currentObjectivePopup);
        await Task.Delay(2500);
        ScreenManager.Instance.HidePopup("ObjectivePopup", hasClosingAnimation: true, shouldDestroyGameObject: false);
        await Task.Delay(1000);
        isPopupPlaying = false;
    }

    public async void ShowNewObjectivePopup()
    {
        ScreenManager.Instance.ShowPopup("NewObjectivePopup", hasOpeningAnimation: true, popupGameObject: _newObjectivePopup);
        await Task.Delay(2500);
        ScreenManager.Instance.HidePopup("NewObjectivePopup", hasClosingAnimation: true, shouldDestroyGameObject: false);
    }
}

