﻿using UnityEngine;
using System.Collections;

public abstract class TimelineIndicator : MonoBehaviour {
    protected SongObject songObject;
    protected ChartEditor editor;
    [HideInInspector]
    public TimelineHandler handle;

    protected Vector2 previousScreenSize = Vector2.zero;
    protected Song previousSong = null;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        previousScreenSize.x = Screen.width;
        previousScreenSize.y = Screen.height;
    }

    protected Vector3 GetLocalPos(uint position, Song song)
    {
        float time = song.ChartPositionToTime(position, song.resolution);
        float endTime = song.length;

        if (endTime > 0)
            return handle.handlePosToLocal(time / endTime);
        else
            return Vector3.zero;
    }

    public virtual void ExplicitUpdate()
    {
        if (songObject != null && songObject.song != null)
            transform.localPosition = GetLocalPos(songObject.position, songObject.song);
    }

    protected void UpdatePreviousVals()
    {
        previousScreenSize.x = Screen.width;
        previousScreenSize.y = Screen.height;

        previousSong = editor.currentSong;
    }
}
