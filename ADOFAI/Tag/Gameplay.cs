using ADOFAI;
using Overlayer.Tag.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Module.ADOFAI.Tag;

public static class Gameplay {
    private static scrController? Controller => scrController.instance;
    private static scrMarginTracker? Tracker => Controller?.playerOne?.marginTracker;
    private static List<scrFloor>? Floors => scrLevelMaker.instance?.listFloors;
    private static LevelData? Level => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;

    [Tag(Desc = "Accuracy (%)")] public static double Accuracy => Percent(Tracker?.percentAcc);
    [Tag(Desc = "XAccuracy (%)")] public static double XAccuracy => Percent(Tracker?.percentXAcc);

    [Tag(Desc = "X-Score")] public static int XScore => Tracker?.xScore ?? 0;
    [Tag(Desc = "Max X-Score")] public static int MaxXScore => Tracker?.maxXScore ?? 0;
    [Tag(Desc = "Last X-Score")] public static int LastXScore => Tracker?.lastXScore ?? 0;

    [Tag(Desc = "Tile progress (%)")] public static double TileProgress => (Controller?.percentComplete ?? 0f) * 100d;
    [Tag(Desc = "Start progress (%)")] public static double StartProgress => TotalTile == 0 ? 0 : StartTile * 100d / TotalTile;
    [Tag(Desc = "Best progress (%)")] public static double BestProgress {
        get {
            GameplayState.BestProgress = Math.Max(GameplayState.BestProgress, TileProgress);
            return GameplayState.BestProgress;
        }
    }
    [Tag(Desc = "Time-based progress (%)")] public static double ActualProgress {
        get {
            var floors = Floors;
            var floor = Controller?.currFloor;
            if(floors == null || floors.Count < 2 || floor == null) return 0;
            double start = floors[0].entryTime;
            double end = floors[^1].entryTime;
            return end <= start ? 0 : Math.Max(0, Math.Min(100, (floor.entryTime - start) * 100 / (end - start)));
        }
    }

    [Tag(Desc = "Checkpoints used")] public static int CheckpointsUsed => scrController.checkpointsUsed;
    [Tag(Desc = "Current checkpoint")] public static int CurCheckpoint => Floors?.Count(floor => floor.seqID <= (Controller?.currentSeqID ?? 0) && floor.GetComponent<ffxCheckpoint>() != null) ?? 0;
    [Tag(Desc = "Total checkpoints")] public static int TotalCheckpoints => Floors?.Count(floor => floor.GetComponent<ffxCheckpoint>() != null) ?? 0;

    [Tag(Desc = "Start tile")] public static int StartTile => Floors == null || Floors.Count == 0 ? 0 : Math.Min(GCS.checkpointNum + 1, Floors.Count);
    [Tag(Desc = "Current tile")] public static int CurTile => Controller == null ? 0 : Controller.currentSeqID + 1;
    [Tag(Desc = "Tiles left")] public static int LeftTile => Math.Max(0, TotalTile - CurTile);
    [Tag(Desc = "Total tiles")] public static int TotalTile => Floors?.Count ?? 0;

    [Tag(Desc = "Speed trial pitch")] public static double SpeedPitch => GCS.currentSpeedTrial;
    [Tag(Desc = "Editor pitch")] public static double EditorPitch => (Level?.pitch ?? 100) / 100d;
    [Tag(Desc = "Difficulty (localized)")] public static string Difficulty => RDString.Get($"enum.Difficulty.{GCS.difficulty}");
    [Tag(Desc = "Difficulty (raw)")] public static string DifficultyRaw => GCS.difficulty.ToString();

    [Tag(Desc = "Playing started")] public static bool IsStarted => Controller != null && Controller.currentSeqID > GCS.checkpointNum;
    [Tag(Desc = "Autoplay enabled")] public static bool IsAutoEnabled => RDConstants.data?.auto ?? false;
    [Tag(Desc = "Practice mode enabled")] public static bool IsPracticeModeEnabled => GCS.practiceMode || (RDConstants.data?.practice ?? false);
    [Tag(Desc = "Old autoplay enabled")] public static bool IsOldAutoEnabled => RDConstants.data?.useOldAuto ?? false;
    [Tag(Desc = "No-fail enabled")] public static bool IsNoFailEnabled => Controller?.noFail ?? GCS.useNoFail;

    [Tag(Desc = "Last hit timing (ms)")] public static double TimingMs => GameplayState.Timing;
    [Tag(Desc = "Average hit timing (ms)")] public static double TimingAvgMs => GameplayState.Timings.Count == 0 ? 0 : GameplayState.Timings.Average();
    [Tag(Desc = "Timing Window Scale")] public static double MarginScale => Controller?.currFloor?.marginScale ?? 1;

    [Tag(Desc = "Song time (minutes)")] public static int SongMinute => SongTime.Minutes;
    [Tag(Desc = "Song time (seconds)")] public static int SongSecond => SongTime.Seconds;
    [Tag(Desc = "Song time (milliseconds)")] public static int SongMilliSecond => SongTime.Milliseconds;
    [Tag(Desc = "Song length (minutes)")] public static int TotalMinute => SongLength.Minutes;
    [Tag(Desc = "Song length (seconds)")] public static int TotalSecond => SongLength.Seconds;
    [Tag(Desc = "Song length (milliseconds)")] public static int TotalMilliSecond => SongLength.Milliseconds;

    [Tag(Desc = "Map length (seconds)")] public static double MapLength => MapSpan.TotalSeconds;
    [Tag(Desc = "Map time (minutes)")] public static int MapMinute => MapElapsed.Minutes;
    [Tag(Desc = "Map time (seconds)")] public static int MapSecond => MapElapsed.Seconds;
    [Tag(Desc = "Map time (milliseconds)")] public static int MapMilliSecond => MapElapsed.Milliseconds;

    [Tag(Desc = "Song title")] public static string Title => Strip(Level?.song);
    [Tag(Desc = "Song artist")] public static string Artist => Strip(Level?.artist);
    [Tag(Desc = "Song author")] public static string Author => Strip(Level?.author);
    [Tag(Desc = "Song title (raw)")] public static string TitleRaw => Level?.song ?? string.Empty;
    [Tag(Desc = "Song artist (raw)")] public static string ArtistRaw => Level?.artist ?? string.Empty;
    [Tag(Desc = "Song author (raw)")] public static string AuthorRaw => Level?.author ?? string.Empty;

    [Tag(Desc = "Tile BPM (with pitch)")] public static double TileBpm => BaseBpm * CurrentSpeed * SongPitch;
    [Tag(Desc = "Current BPM (with pitch)")] public static double CurBpm => RealBpm * SongPitch;
    [Tag(Desc = "Keys per second")] public static double KPS => CurBpm / 60d;
    [Tag(Desc = "Tile BPM (pitch excluded)")] public static double TileBpmWithoutPitch => BaseBpm * CurrentSpeed;
    [Tag(Desc = "Current BPM (pitch excluded)")] public static double CurBpmWithoutPitch => RealBpm;
    [Tag(Desc = "Keys per second (pitch excluded)")] public static double KPSWithoutPitch => CurBpmWithoutPitch / 60d;

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
    private static double BaseBpm => Level?.bpm ?? scrConductor.instance?.bpm ?? 0;
    private static double CurrentSpeed => Controller?.currFloor?.speed ?? 0;
    private static double SongPitch => scrConductor.instance?.song?.pitch ?? 1;
    private static double RealBpm {
        get {
            var floor = Controller?.currFloor;
            if(floor?.nextfloor == null) return BaseBpm * CurrentSpeed;
            double duration = floor.nextfloor.entryTime - floor.entryTime;
            return duration <= 0 ? BaseBpm * CurrentSpeed : 60d / duration;
        }
    }

    private static double Percent(float? value) => value.HasValue && !float.IsNaN(value.Value) ? value.Value * 100d : 0;
    private static string Strip(string? value) => string.IsNullOrEmpty(value) ? string.Empty : RDUtils.RemoveRichTags(value);
}

internal static class GameplayState {
    internal static double Timing;
    internal static double BestProgress;
    internal static readonly List<double> Timings = new();

    internal static void RecordTiming(double value) {
        Timing = value;
        Timings.Add(value);
    }

    internal static void Reset() {
        Timing = 0;
        Timings.Clear();
    }
}
