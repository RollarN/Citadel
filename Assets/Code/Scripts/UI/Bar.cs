using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    public Slider slider;

    public virtual void SetMaxValue(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public virtual void SetValue(float health)
    {
        slider.value = health;
    }
}
