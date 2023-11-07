using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ShieldSectionUi : MonoBehaviour
{

    Outline outline;
    RectTransform rt;
    public static ShieldSectionUi selected = null;

    float stepAngle;
    int totalSteps;
    int currentStep;
    float stepSpeed;
    public void Setup(int totalSteps, int startingStep, float stepAngle, float rotSpeed)
    {
        rt = GetComponent<RectTransform>();
        outline = GetComponent<Outline>();
        currentStep = startingStep;
        this.stepAngle = stepAngle;
        this.totalSteps = totalSteps;
        this.stepSpeed = 2f - rotSpeed;
        float angle = startingStep * stepAngle;
        Rotate(angle, 0f);
    }

    public void Select()
    {
        if (selected == this) return;
        selected?.Deselect();
        outline.DOFade(.5f, .5f);
        selected = this;
    }

    void Deselect()
    {
        outline.DOFade(0f, .5f);
    }

    public void RotateRight()
    {
        currentStep = (currentStep - 1) % totalSteps;
        Rotate(currentStep * stepAngle, stepSpeed);
    }

    public void RotateLeft()
    {
        currentStep = (currentStep + 1) % totalSteps;
        Rotate(currentStep * stepAngle, stepSpeed);
    }

    void Rotate(float angle, float duration)
    {
        rt.DORotate(new Vector3(0, 0, angle), duration, RotateMode.Fast)
            .SetEase(Ease.InOutQuint)
            .OnComplete( delegate {
                ShieldCircularPuzzleController.instance.CheckCorrectness();
            });
    }

    public bool IsCorrect()
    {
        return currentStep == 0;
    }

}
