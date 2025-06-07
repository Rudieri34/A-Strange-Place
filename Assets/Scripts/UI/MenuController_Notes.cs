using TMPro;
using UnityEngine;

public class MenuController_Notes : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noteText;
    private int currentPage = 1;

    private void Start()
    {
        noteText.pageToDisplay = currentPage;
    }

    public void ShowNewNote(string noteContent)
    {
        noteText.text = noteContent;
        currentPage = 1;
        noteText.pageToDisplay = currentPage;
    }

    public void NextPage()
    {
        if (currentPage < noteText.textInfo.pageCount)
        {
            currentPage++;
            noteText.pageToDisplay = currentPage;
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            noteText.pageToDisplay = currentPage;
        }
    }
}
