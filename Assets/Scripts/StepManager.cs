using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepManager : MonoBehaviour
{
    public TextMeshProUGUI stepDescriptionText;
    public TextMeshProUGUI stepNumberText;
    private StepsTutorial stepsTutorial;
    private int currentStepIndex = 0;
    public GameObject canvas;
    public TextMeshProUGUI sttText;
    public Image stepImage; // Public field for the sprite
    public Sprite defaultStep; // Public field for the default sprite

    public void Initialize(StepsTutorial steps)
    {
        canvas.SetActive(true);
        stepsTutorial = steps;
        currentStepIndex = 0;
        sttText.text = "Press the record button to start recording your voice";
        stepImage.sprite = defaultStep;

        // Find the current step
        for (int i = 0; i < stepsTutorial.steps.Length; i++)
        {
            if (stepsTutorial.steps[i].is_current_step)
            {
                currentStepIndex = i;
                break;
            }
        }

        UpdateUI();
    }

    public void NextStep()
    {
        if (stepsTutorial != null && currentStepIndex < stepsTutorial.steps.Length - 1)
        {
            stepsTutorial.steps[currentStepIndex].is_current_step = false;
            currentStepIndex++;
            stepsTutorial.steps[currentStepIndex].is_current_step = true;
            UpdateUI();
        }
    }

    public void PreviousStep()
    {
        if (stepsTutorial != null && currentStepIndex > 0)
        {
            stepsTutorial.steps[currentStepIndex].is_current_step = false;
            currentStepIndex--;
            stepsTutorial.steps[currentStepIndex].is_current_step = true;
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