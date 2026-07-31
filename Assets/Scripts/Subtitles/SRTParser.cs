using System;
using System.Collections.Generic;
using UnityEngine;

public class SRTParser
{
    List<SubtitleBlock> _subtitles;
    public List<SubtitleBlock> Subtitles
    {
        get { return _subtitles; }
    }

    public SRTParser(string subtitlesString)
    {
        _subtitles = Load(subtitlesString);
    }

    public SRTParser(TextAsset textAsset)
    {
        _subtitles = Load(textAsset);
    }

    static public List<SubtitleBlock> Load(string subtitlesString)
    {
        subtitlesString = subtitlesString.Replace("WEBVTT", "");

        string[] lines = subtitlesString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        var currentState = eReadState.Index;

        var subs = new List<SubtitleBlock>();

        int currentIndex = 0;
        double currentFrom = 0, currentTo = 0;
        var currentText = string.Empty;
        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];

            switch (currentState)
            {
                case eReadState.Index:
                    {
                        int index;
                        if (int.TryParse(line, out index))
                        {
                            currentIndex = index;
                            currentState = eReadState.Time;
                        }
                        else
                        {
                            goto case eReadState.Time;
                        }
                    }
                    break;
                case eReadState.Time:
                    {
                        line = line.Replace(',', '.');
                        var parts = line.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);

                        // Parse the timestamps
                        if (parts.Length == 2)
                        {
                            TimeSpan fromTime;
                            if (TimeSpan.TryParse(parts[0], out fromTime))
                            {
                                TimeSpan toTime;
                                if (TimeSpan.TryParse(parts[1], out toTime))
                                {
                                    currentFrom = fromTime.TotalSeconds;
                                    currentTo = toTime.TotalSeconds;
                                    currentState = eReadState.Text;
                                }
                            }
                        }
                    }
                    break;

                case eReadState.Text:
                    {
                        if (currentText != string.Empty)
                            currentText += "\r\n";

                        currentText += line;

                        // When we hit an empty line, consider it the end of the text
                        if (string.IsNullOrEmpty(line) || l == lines.Length - 1)
                        {
                            if (string.IsNullOrEmpty(currentText))
                                break;

                            // Create the SubtitleBlock with the data we've aquired
                            subs.Add(new SubtitleBlock(currentIndex, currentFrom, currentTo, currentText));

                            // Reset stuff so we can start again for the next block
                            currentText = string.Empty;
                            currentState = eReadState.Index;
                        }
                    }
                    break;
            }
        }
        return subs;
    }

    static public List<SubtitleBlock> Load(TextAsset textAsset)
    {
        if (textAsset == null)
        {
            Debug.LogError("Subtitle file is null");
            return null;
        }

        return Load(textAsset.text);
    }

    public SubtitleBlock GetForTime(float time)
    {
        if (_subtitles[0].From > time)
        {
            return SubtitleBlock.Blank;
        }

        for (int i = 0; i < _subtitles.Count; i++)
        {
            if (_subtitles[i].From <= time && _subtitles[i].To >= time)
            {
                Debug.Log("Found subtitle");
                return _subtitles[i];
            }

        }

        if (time < _subtitles[_subtitles.Count - 1].To)
        {
            return SubtitleBlock.Blank;
        }

        return null;
    }

        enum eReadState
        {
            Index,
            Time,
            Text
        }
    }

public class SubtitleBlock
{
    static SubtitleBlock _blank;
    public static SubtitleBlock Blank => _blank ?? (_blank = new SubtitleBlock(-1, 0, 0, string.Empty));
    public int Index { get;  }
    public double Length { get;  }
    public double From { get;  }
    public double To { get;  }
    public string Text { get;  }

    public SubtitleBlock(int index, double from, double to, string text)
    {
        Index = index;
        From = from;
        To = to;
        Length = to - from;
        Text = text;
    }
}
