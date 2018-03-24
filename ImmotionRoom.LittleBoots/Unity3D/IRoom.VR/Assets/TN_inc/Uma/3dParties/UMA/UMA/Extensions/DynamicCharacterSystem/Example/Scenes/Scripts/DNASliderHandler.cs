﻿using UnityEngine;
using System.Collections;
using UMACharacterSystem;
using UnityEngine.UI;

public class DNASliderHandler : MonoBehaviour
{
    DnaSetter DNA;
    DynamicCharacterAvatar Avatar;
    Slider Slider;

    public void Setup(DnaSetter dna, DynamicCharacterAvatar avatar)
    {
        DNA = dna;
        Avatar = avatar;
        Slider = GetComponent<Slider>();
        Slider.value = dna.Value;
    }

    public void ValueChanged(float value)
    {
        DNA.Set(value);
        Avatar.ForceUpdate(true);
    }
}
