using ADOFAI;
using Overlayer.Tag.Core;
using System;
using System.Collections.Generic;

namespace Overlayer.Module.ADOFAI.Tag.Gameplay;

public static class Time {
    private static scrController? Controller => scrController.instance;
    private static List<scrFloor>? Floors => scrLevelMaker.instance?.listFloors;

    [Tag(Desc = "Song time (days)")] public static int SongDay => SongTime.Days;
    [Tag(Desc = "Song time (hours)")] public static int SongHour => SongTime.Hours;
    [Tag(Desc = "Song time (minutes)")] public static int SongMinute => SongTime.Minutes;
    [Tag(Desc = "Song time (seconds)")] public static int SongSecond => SongTime.Seconds;
    [Tag(Desc = "Song time (milliseconds)")] public static int SongMilliSecond => SongTime.Milliseconds;
    [Tag(Desc = "Song length (days)")] public static int TotalDay => SongLength.Days;
    [Tag(Desc = "Song length (hours)")] public static int TotalHour => SongLength.Hours;
    [Tag(Desc = "Song length (minutes)")] public static int TotalMinute => SongLength.Minutes;
    [Tag(Desc = "Song length (seconds)")] public static int TotalSecond => SongLength.Seconds;
    [Tag(Desc = "Song length (milliseconds)")] public static int TotalMilliSecond => SongLength.Milliseconds;

    [Tag(Desc = "Map length (seconds)")] public static double MapLength => MapSpan.TotalSeconds;
    [Tag(Desc = "Map time (minutes)")] public static int MapMinute => MapElapsed.Minutes;
    [Tag(Desc = "Map time (seconds)")] public static int MapSecond => MapElapsed.Seconds;
    [Tag(Desc = "Map time (milliseconds)")] public static int MapMilliSecond => MapElapsed.Milliseconds;

    private static TimeSpan SongTime => TimeSpan.FromSeconds(Math.Max(0, scrConductor.instance?.song?.time ?? 0));
    private static TimeSpan SongLength => TimeSpan.FromSeconds(Math.Max(0, scrConductor.instance?.song?.clip?.length ?? 0));
    private static TimeSpan MapSpan {
        get {
            var floors = Floors;
            if(floors == null || floors.Count == 0) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(Math.Max(0, floors[^1].entryTime - floors[0].entryTime));
        }
    }
    private static TimeSpan MapElapsed {
        get {
            var floors = Floors;
            var floor = Controller?.currFloor;
            if(floors == null || floors.Count == 0 || floor == null) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(Math.Max(0, floor.entryTime - floors[0].entryTime));
        }
    }
}
