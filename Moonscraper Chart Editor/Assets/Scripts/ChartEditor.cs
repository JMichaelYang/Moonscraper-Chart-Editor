﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour {
    [Header("Prefabs")]
    public GameObject note;
    public GameObject section;
    [Header("Indicator Parents")]
    public GameObject sectionIndicators;
    [Header("Misc.")]
    public Button play;
    public Text songNameText;
    public Transform strikeline;

    AudioSource musicSource;

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

    MovementController movement;

    // Use this for initialization
    void Awake () {
        currentSong = new Song();
        currentChart = currentSong.expert_single;
        musicSource = GetComponent<AudioSource>();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
    }

    void Update()
    {
    }

    // Wrapper function
    public void LoadSong()
    {
        StartCoroutine(_LoadSong());
    }

    public void Save()
    {
        if (currentSong != null)
            currentSong.Save("test.chart");
    }

    public void Play()
    {
        float strikelinePos = strikeline.position.y;
        musicSource.time = Song.WorldYPositionToTime(strikelinePos) + currentSong.offset;       // No need to add audio calibration as position is base on the strikeline position
        play.interactable = false;
        movement.applicationMode = MovementController.ApplicationMode.Playing;
        musicSource.Play();
    }

    public void Stop()
    {
        play.interactable = true;
        movement.applicationMode = MovementController.ApplicationMode.Editor;
        musicSource.Stop();
    }

    IEnumerator _LoadSong()
    {
        float totalLoadTime = 0;

        try
        {
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart");

            totalLoadTime = Time.realtimeSinceStartup;

            currentSong = new Song(currentFileName);
            Debug.Log("Song load time: " + (Time.realtimeSinceStartup - totalLoadTime));

            float objectLoadTime = Time.realtimeSinceStartup;

            // Remove objects from previous chart
            foreach (GameObject chartObject in GameObject.FindGameObjectsWithTag("Chart Object"))
            {
                Destroy(chartObject);
            }
            foreach (GameObject songObject in GameObject.FindGameObjectsWithTag("Song Object"))
            {
                Destroy(songObject);
            }

            currentChart = currentSong.expert_single;

            // Create the actual objects
            CreateSongObjects(currentSong);
            CreateChartObjects(currentChart);

            Debug.Log("Chart objects load time: " + (Time.realtimeSinceStartup - objectLoadTime));

            songNameText.text = currentSong.name;    
        }
        catch (System.Exception e)
        {
            // Most likely closed the window explorer, just ignore for now.
            currentFileName = string.Empty;
            currentSong = new Song();
            Debug.LogError(e.Message);

            yield break;
        }

        while (currentSong.musicStream != null && currentSong.musicStream.loadState != AudioDataLoadState.Loaded)
        {
            Debug.Log("Loading audio...");
            yield return null;
        }

        if (currentSong.musicStream != null)
        {
            musicSource.clip = currentSong.musicStream;
            movement.SetPosition(0);
        }

        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
    }

    public void AddNewNoteToCurrentChart(Note note, GameObject parent)
    {
        // Insert note into current chart
        int position = currentChart.Add(note);

        // Create note object
        NoteController controller = CreateNoteObject(note, parent);
    }

    // Create Sections, bpms, events and time signature objects
    GameObject CreateSongObjects(Song song)
    {
        GameObject parent = new GameObject();
        parent.name = "Song Objects";
        parent.tag = "Song Object";

        return parent;
    }

    // Create note, starpower and chart event objects
    GameObject CreateChartObjects(Chart chart, GameObject notePrefab)
    {
        GameObject parent = new GameObject();
        parent.name = "Chart Objects";
        parent.tag = "Chart Object";

        Note[] notes = chart.notes;

        for (int i = 0; i < notes.Length; ++i)
        {
            NoteController controller = CreateNoteObject(notes[i], parent);

            controller.UpdateSongObject();
        }

        return parent;
    }

    GameObject CreateChartObjects(Chart chart)
    {
        return CreateChartObjects(chart, note);
    }

    NoteController CreateNoteObject(Note note, GameObject parent = null)
    {
        // Convert the chart data into gameobject
        GameObject noteObject = Instantiate(this.note);

        if (parent)
            noteObject.transform.parent = parent.transform;

        // Attach the note to the object
        NoteController controller = noteObject.GetComponent<NoteController>();

        // Link controller and note together
        controller.Init(movement, note);

        return controller;
    }
}