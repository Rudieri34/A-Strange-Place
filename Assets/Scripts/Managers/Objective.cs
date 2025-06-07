using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives/New Objective", order = 0)]
public class Objective : ScriptableObject
{
    public ObjectiveTag objectiveTag;
    public string taskDescription;
    public int targetedProgress = 1;
    [HideInInspector] public int currentProgress;
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isCompleted;
    public ObjectiveTag nextObjective = ObjectiveTag.No_Objective;

    public void UpdateProgress(int value)
    {
        if (isActive)
        {
            currentProgress += value;
            SetProgress(currentProgress >= targetedProgress);
        }
    }

    public void SetProgress(bool value)
    {
        if (isActive)
        {
            isCompleted = value;
            SaveManager.Instance.AddNewCompletedObjective(objectiveTag);
            if (value)
            {
                isActive = false;
                ObjectivesManager.Instance.SetNewActiveObjective(nextObjective);
            }
        }
    }

}

public enum ObjectiveTag
{
    No_Objective,
    O1_Collect_Cameras_Van,
    O2_Enter_Hotel_and_Explore,
    O3_Set_Investigation_Board,
    O4_Explore_Hotel_Pt1,
    O5_Update_Investigation,
    O6_Install_Camera,
    O7_Check_Camera,
    O8_Explore_Hotel_Pt2,
    O9_Beat_The_Last_Drop,
    O10_Explore_Hotel_Pt3
}