using UnityEngine;
using TMPro;

public class TitleOption : MonoBehaviour
{
    [TextArea(2, 4)]
    public string titleText; 
    public bool isCorrect;

    [TextArea(2, 4)]
    public string feedbackText; 

    public TMP_Text buttonText; 

    private void OnValidate()
    {
        if (buttonText != null) buttonText.text = titleText;
    }

    private void Start()
    {
        if (buttonText != null) buttonText.text = titleText;
    }
}