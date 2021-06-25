﻿using Il2CppSystem;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyCalculator
{
    public string songID;

    public static Dictionary<string, CachedCalculation> calculatorCache = new Dictionary<string, CachedCalculation>();

    public CalculatedDifficulty expert;
    public CalculatedDifficulty advanced;
    public CalculatedDifficulty standard;
    public CalculatedDifficulty beginner;

    public DifficultyCalculator(SongList.SongData songData)
    {
        if (songData == null) return;
        this.songID = songData.songID;
        EvaluateDifficulties(songData);
    }

    public struct CachedCalculation
    {
        public float value;
        public bool is360;
        public bool hasMines;

        public CachedCalculation(float valuef = 0, bool is360b = false, bool hasMinesb = false)
        {
            value = valuef;
            is360 = is360b;
            hasMines = hasMinesb;
        }
    }

    public static CachedCalculation GetRating(string songID, string difficulty)
    {       
        var songData = SongList.I.GetSong(songID);
        var diffLower = difficulty.ToLower();
        if (calculatorCache.ContainsKey(songID + diffLower)) return calculatorCache[songID + diffLower];
        else
        {
            if (songData == null) return new CachedCalculation();
            var calc = new DifficultyCalculator(songData);
            switch (diffLower)
            {
                case "easy":
                    if (calc.beginner != null)
                    {
                        CachedCalculation data = new CachedCalculation(calc.beginner.difficultyRating, calc.beginner.is360, calc.beginner.hasMines);
                        calculatorCache.Add(songID + diffLower, data);
                        //MelonLogger.Msg(songData.title + " (beginner) span: " + Math.Abs(calc.beginner.cueExtremesX.highest - calc.beginner.cueExtremesX.lowest));
                        return data;
                    }
                    else
                    {
                        calculatorCache.Add(songID + diffLower, new CachedCalculation());
                        return new CachedCalculation();
                    }
                case "normal":
                    if (calc.standard != null)
                    {
                        CachedCalculation data = new CachedCalculation(calc.standard.difficultyRating, calc.standard.is360, calc.standard.hasMines);
                        calculatorCache.Add(songID + diffLower, data);
                        //MelonLogger.Msg(songData.title + " (normal) span: " + Math.Abs(calc.standard.cueExtremesX.highest - calc.standard.cueExtremesX.lowest));

                        return data;
                    }
                    else
                    {
                        calculatorCache.Add(songID + diffLower, new CachedCalculation());
                        return new CachedCalculation();
                    }
                case "hard":
                    if (calc.advanced != null)
                    {
                        CachedCalculation data = new CachedCalculation(calc.advanced.difficultyRating, calc.advanced.is360, calc.advanced.hasMines);
                        calculatorCache.Add(songID + diffLower, data);
                        //MelonLogger.Msg(songData.title + " (hard) span: " + Math.Abs(calc.advanced.cueExtremesX.highest - calc.advanced.cueExtremesX.lowest));

                        return data;
                    }
                    else
                    {
                        calculatorCache.Add(songID + diffLower, new CachedCalculation());
                        return new CachedCalculation();
                    }
                case "expert":
                    if (calc.expert != null)
                    {
                        CachedCalculation data = new CachedCalculation(calc.expert.difficultyRating, calc.expert.is360, calc.expert.hasMines);
                        calculatorCache.Add(songID + diffLower, data);
                        //MelonLogger.Msg(songData.title + " (expert) span: " + Math.Abs(calc.expert.cueExtremesX.highest - calc.expert.cueExtremesX.lowest));
                        return data;
                    }
                    else
                    {
                        calculatorCache.Add(songID + diffLower, new CachedCalculation());
                        return new CachedCalculation();
                    }
                default:
                    return new CachedCalculation();
            }
        }

    }

    public float GetRatingFromKataDifficulty(KataConfig.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case KataConfig.Difficulty.Easy:
                if (beginner != null) return beginner.difficultyRating;
                else return 0f;
            case KataConfig.Difficulty.Normal:
                if (standard != null) return standard.difficultyRating;
                else return 0f;
            case KataConfig.Difficulty.Hard:
                if (advanced != null) return advanced.difficultyRating;
                else return 0f;
            case KataConfig.Difficulty.Expert:
                if (expert != null) return expert.difficultyRating;
                else return 0f;
            default:
                return 0f;
        }
    }

    private void EvaluateDifficulties(SongList.SongData songData)
    {
        var expertCues = SongCues.GetCues(songData, KataConfig.Difficulty.Expert);
        if (expertCues.Length > 0 && expertCues != null)
        {
            this.expert = new CalculatedDifficulty(expertCues, songData);
        }

        var advancedCues = SongCues.GetCues(songData, KataConfig.Difficulty.Hard);
        if (advancedCues.Length > 0 && advancedCues != null)
        {
            this.advanced = new CalculatedDifficulty(advancedCues, songData);

        }
        var standardCues = SongCues.GetCues(songData, KataConfig.Difficulty.Normal);
        if (standardCues.Length > 0 && standardCues != null)
        {
            this.standard = new CalculatedDifficulty(standardCues, songData);

        }
        var beginnerCues = SongCues.GetCues(songData, KataConfig.Difficulty.Easy);
        if (beginnerCues.Length > 0 && beginnerCues != null)
        {
            this.beginner = new CalculatedDifficulty(beginnerCues, songData);
        }
    }
}
public class CalculatedDifficulty
{
    public static float spacingMultiplier = 1f;
    public static float lengthMultiplier = 0.7f;
    public static float densityMultiplier = 1f;
    public static float readabilityMultiplier = 1.2f;

    public float difficultyRating;
    public float spacing;
    public float density;
    public float readability;

    public (float lowest, float highest) cueExtremesX = (0, 0);
    public (float lowest, float highest) cueExtremesY = (0, 0);
    public bool is360 = false;
    public bool hasMines = false;


    float length;

    public CalculatedDifficulty(SongCues.Cue[] cues, SongList.SongData songData)
    {
        EvaluateCues(cues, songData);

        //autodetect 360
        is360 = (Math.Abs(cueExtremesX.highest - cueExtremesX.lowest) >= 20);
        

    }

    public static Dictionary<Target.TargetBehavior, float> objectDifficultyModifier = new Dictionary<Target.TargetBehavior, float>()
    {
        { Target.TargetBehavior.Standard, 1f },
        { Target.TargetBehavior.Vertical, 1.2f },
        { Target.TargetBehavior.Horizontal, 1.3f },
        { Target.TargetBehavior.Hold, 1f },
        { Target.TargetBehavior.ChainStart, 1.2f },
        { Target.TargetBehavior.Chain, 0.2f },
        { Target.TargetBehavior.Melee, 0.6f }
    };

    List<SongCues.Cue> leftHandCues = new List<SongCues.Cue>();
    List<SongCues.Cue> rightHandCues = new List<SongCues.Cue>();
    List<SongCues.Cue> eitherHandCues = new List<SongCues.Cue>();
    List<SongCues.Cue> allCues = new List<SongCues.Cue>();

    public void EvaluateCues(SongCues.Cue[] cues, SongList.SongData songData)
    {
        this.length = AudioDriver.TickSpanToMs(songData, cues[0].tick, cues[cues.Length - 1].tick);
        if (cues.Length >= 15 && this.length > 30000f)
        {
            SplitCues(cues);
            CalculateSpacing();
            CalculateDensity();
            CalculateReadability();
            difficultyRating = ((spacing + readability) / length) * 500f + (length / 100000f * lengthMultiplier);
        }
        else difficultyRating = 0f;
    }

    void CalculateReadability()
    {
        //init vals
        cueExtremesX.lowest = GetTrueCoordinates(allCues[0]).x;
        cueExtremesX.highest = GetTrueCoordinates(allCues[0]).x;

        cueExtremesY.lowest = GetTrueCoordinates(allCues[0]).y;
        cueExtremesY.highest = GetTrueCoordinates(allCues[0]).y;

        for (int i = 0; i < allCues.Count; i++)
        {
            float modifierValue = 0f;
            objectDifficultyModifier.TryGetValue(allCues[i].behavior, out modifierValue);
            readability += modifierValue * readabilityMultiplier;

            //detect mines
            if(allCues[i].behavior == Target.TargetBehavior.Dodge)
            {
                hasMines = true;
            }
            
            //calculate extremes
            float truCoord = 0;

            //x extremes
            truCoord = GetTrueCoordinates(allCues[i]).x;

            if (truCoord < cueExtremesX.lowest)
                cueExtremesX.lowest = truCoord;

            if (truCoord > cueExtremesX.highest)
                cueExtremesX.highest = truCoord;

            //y extremes
            truCoord = GetTrueCoordinates(allCues[i]).y;

            if (truCoord < cueExtremesY.lowest)
                cueExtremesY.lowest = truCoord;

            if (truCoord > cueExtremesY.highest)
                cueExtremesY.highest = truCoord;
        }
        //readability /= length;
    }

    void CalculateSpacing()
    {
        GetSpacingPerHand(leftHandCues);
        GetSpacingPerHand(rightHandCues);
        //spacing /= length;
    }

    void CalculateDensity()
    {
        density = (float)allCues.Count / length;
    }

    private void GetSpacingPerHand(List<SongCues.Cue> cues)
    {
        for (int i = 1; i < cues.Count; i++)
        {
            float dist = Vector2.Distance(GetTrueCoordinates(cues[i]), GetTrueCoordinates(cues[i - 1]));
            float distMultiplied = cues[i].behavior == Target.TargetBehavior.Melee ? float.Epsilon :
                dist * spacingMultiplier;
            spacing += distMultiplied;
        }
    }

    Vector2 GetTrueCoordinates(SongCues.Cue cue)
    {
        float x = cue.pitch % 12;
        float y = (int)(cue.pitch / 12);
        x += cue.gridOffset.x;
        y += cue.gridOffset.y;
        return new Vector2(x, y);
    }

    void SplitCues(SongCues.Cue[] cues)
    {
        for (int i = 0; i < cues.Length; i++)
        {
            allCues.Add(cues[i]);
            switch (cues[i].handType)
            {
                case Target.TargetHandType.Left:
                    leftHandCues.Add(cues[i]);
                    break;
                case Target.TargetHandType.Right:
                    rightHandCues.Add(cues[i]);
                    break;
                case Target.TargetHandType.Either:
                    eitherHandCues.Add(cues[i]);
                    break;
                default:
                    break;
            }
        }
    }
}