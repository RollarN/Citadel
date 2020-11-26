using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBar : Bar
{
    // Start is called before the first frame update
    public Image ImageToFill;

    public override void SetValue(float amount)
    {
        ImageToFill.fillAmount = amount / 100;
    }

    // Update is called once per frame

    public override void SetMaxValue(float health)
    {
        return;
    }
}
