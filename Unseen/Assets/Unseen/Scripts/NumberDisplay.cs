using UnityEngine;
using TMPro;

public class NumberDisplay : MonoBehaviour
{
    public TextMeshPro displayText;
    public int correctNumber = 5;
    public CodeDoor codeDoor;

    private int currentNumber = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void IncrementNumber()
    {
        currentNumber++;
        if (currentNumber > 9) currentNumber = 0;
        UpdateDisplay();
        CheckCode();
    }

    public void DecrementNumber()
    {
        currentNumber--;
        if (currentNumber < 0) currentNumber = 9;
        UpdateDisplay();
        CheckCode();
    }

    void UpdateDisplay()
    {
        displayText.text = currentNumber.ToString();
    }

    void CheckCode()
    {
        if (codeDoor != null)
        {
            codeDoor.CheckAllNumbers();
        }
    }

    public bool IsCorrect()
    {
        return currentNumber == correctNumber;
    }
}