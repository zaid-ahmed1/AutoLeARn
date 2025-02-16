using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepManager : MonoBehaviour
{
    public TextMeshProUGUI stepDescriptionText;
    public TextMeshProUGUI stepNumberText;
    private StepsTutorial stepsTutorial;
    private int currentStepIndex = 0;

    public void Initialize(StepsTutorial steps)
    {
        stepsTutorial = steps;
        currentStepIndex = 0;
        UpdateUI();
    }

    public void NextStep()
    {
        if (stepsTutorial != null && currentStepIndex < stepsTutorial.steps.Length - 1)
        {
            currentStepIndex++;
            UpdateUI();
        }
    }

    public void PreviousStep()
    {
        if (stepsTutorial != null && currentStepIndex > 0)
        {
            currentStepIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (stepsTutorial != null && stepsTutorial.steps.Length > 0)
        {
            Step currentStep = stepsTutorial.steps[currentStepIndex];
            stepDescriptionText.text = currentStep.step_description;
            stepNumberText.text = $"{currentStep.step_number}/{stepsTutorial.steps.Length}";
        }
    }
}