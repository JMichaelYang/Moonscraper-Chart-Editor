﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayProperties : MonoBehaviour {
    public Text songNameText;
    public Slider hyperspeedSlider;
    public InputField snappingStep;
    public Text noteCount;
    public Text gameSpeed;
    public Slider gameSpeedSlider;
    public Toggle clapToggle; 
    public Toggle metronomeToggle;
    public Slider highwayLengthSlider;
    public Transform maxHighwayLength;
    public float minHighwayLength = 11.75f;
    [SerializeField]
    BGFadeHeightController bgFade;

    ChartEditor editor;
    int prevNoteCount = -1;
    string prevSongName, prevChartName;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        hyperspeedSlider.value = Globals.hyperspeed;
        highwayLengthSlider.value = Globals.highwayLength;

        snappingStep.onValidateInput = Step.validateStepVal;
        snappingStep.text = Globals.step.ToString();

        OnEnable();
    }

    void OnEnable()
    {
        clapToggle.isOn = (Globals.clapSetting != Globals.ClapToggle.NONE);
        metronomeToggle.isOn = Globals.metronomeActive;
    }

    void Update()
    {
        if (prevChartName != editor.currentChart.name || prevSongName != editor.currentSong.name)
            songNameText.text = editor.currentSong.name + " - " + editor.currentChart.name;

        // Disable sliders during play
        bool interactable = (Globals.applicationMode != Globals.ApplicationMode.Playing);
        hyperspeedSlider.interactable = interactable;
        gameSpeedSlider.interactable = interactable;
        highwayLengthSlider.interactable = interactable;

        if (snappingStep.text != string.Empty)
            snappingStep.text = Globals.step.ToString();

        if (editor.currentChart.note_count != prevNoteCount)
            noteCount.text = "Notes: " + editor.currentChart.note_count.ToString();

        // Shortcuts
        if (!Globals.modifierInputActive && !Globals.IsTyping)
        {
            if (Input.GetButtonDown("ToggleClap"))
                clapToggle.isOn = !clapToggle.isOn;

            if (Input.GetButtonDown("Toggle Metronome"))
                metronomeToggle.isOn = !metronomeToggle.isOn;
        }

        prevNoteCount = editor.currentChart.note_count;
        prevSongName = editor.currentSong.name;
        prevChartName = editor.currentChart.name;
    }

    public void SetHyperspeed(float value)
    {
        Globals.hyperspeed = value;
    }

    public void SetGameSpeed(float value)
    {
        value = Mathf.Round(value / 5.0f) * 5;
        Globals.gameSpeed = value / 100.0f;
        gameSpeed.text = "Speed- x" + Globals.gameSpeed.ToString();
    }

    public void SetHighwayLength(float value)
    {
        Globals.highwayLength = value;

        Vector3 pos = Vector3.zero;
        pos.y = value * 5 + minHighwayLength;
        maxHighwayLength.transform.localPosition = pos;

        bgFade.AdjustHeight();
    }

    public void ToggleClap(bool value)
    {
        if (value)
            Globals.clapSetting = Globals.clapProperties;
        else
            Globals.clapSetting = Globals.ClapToggle.NONE;

        Debug.Log("Clap toggled: " + value);
    }

    public void ToggleMetronome(bool value)
    {
        Globals.metronomeActive = value;

        Debug.Log("Metronome toggled: " + value);
    }

    public void IncrementSnappingStep()
    {
        Globals.snappingStep.Increment();
    }

    public void DecrementSnappingStep()
    {
        Globals.snappingStep.Decrement();
    }

    public void SetStep(string value)
    {
        if (value != string.Empty)
        {
            StepInputEndEdit(value);
        }
    }

    public void StepInputEndEdit(string value)
    {
        int stepVal;
        const int defaultControlsStepVal = 16;

        if (value == string.Empty)
            stepVal = defaultControlsStepVal;
        else
        {
            try
            {
                stepVal = int.Parse(value);

                if (stepVal < Step.MIN_STEP)
                    stepVal = Step.MIN_STEP;
                else if (stepVal > Step.FULL_STEP)
                    stepVal = Step.FULL_STEP;
            }
            catch
            {
                stepVal = defaultControlsStepVal;
            }
        }

        Globals.step = stepVal;
        snappingStep.text = Globals.step.ToString();
    }
}
