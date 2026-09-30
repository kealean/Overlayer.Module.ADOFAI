using ADOFAI;
using Overlayer.ModuleAPI;
using Overlayer.Tag.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Module.ADOFAI.Tag.Gameplay;

public static class Status {
    private static scrController? Controller => scrController.instance;
    private static List<scrFloor>? Floors => scrLevelMaker.instance?.listFloors;
    private static LevelData? Level => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;

    [Tag(Desc = "Speed trial pitch")] public static double SpeedPitch => GCS.currentSpeedTrial;
    [Tag(Desc = "Difficulty (localized)")] public static string Difficulty => RDString.Get($"enum.Difficulty.{GCS.difficulty}");
    [Tag(Desc = "Difficulty (raw)")] public static string DifficultyRaw => GCS.difficulty.ToString();

    [Tag(Desc = "Playing started")] public static bool IsStarted => Controller != null && Controller.currentSeqID > GCS.checkpointNum;
    [Tag(Desc = "Autoplay enabled")] public static bool IsAutoEnabled => RDConstants.data?.auto ?? false;
    [Tag(Desc = "Practice mode enabled")] public static bool IsPracticeModeEnabled => GCS.practiceMode || (RDConstants.data?.practice ?? false);
    [Tag(Desc = "Old autoplay enabled")] public static bool IsOldAutoEnabled => RDConstants.data?.useOldAuto ?? false;
    [Tag(Desc = "No-fail enabled")] public static bool IsNoFailEnabled => Controller?.noFail ?? GCS.useNoFail;
    [Tag(Desc = "Current tile is auto")] public static bool IsAutoTile {
        get {
            var floors = Floors;
            int idx = Progress.CurTile;
            return floors != null && idx >= 0 && idx < floors.Count && floors[idx].auto;
        }
    }
    [Tag(Desc = "Speed trial mode")] public static bool IsSpeedTrialEnabled => GCS.speedTrialMode;
    [Tag(Desc = "Level editor open")] public static bool IsLevelEditor => ADOBase.isLevelEditor;
    [Tag(Desc = "Official level")] public static bool IsOfficialLevel => ADOBase.isOfficialLevel;
    [Tag(Desc = "In game world")] public static bool IsGameWorld => scrConductor.instance?.isGameWorld ?? false;
    [Tag(Desc = "Paused")] public static bool IsPaused => Controller?.paused ?? false;
    [Tag(Desc = "Attempts")] public static int Attempts {
        get {
            if(scnGame.instance == null) {
                var c = Controller;
                var cond = scrConductor.instance;
                return c != null && cond != null && ADOBase.sceneName.Contains("-") && !c.noFail && cond.isGameWorld
                    ? Persistence.GetWorldAttempts(scrController.currentWorld)
                    : 0;
            }
            if(scnEditor.instance != null) return 0;
            var level = Level;
            return level == null ? 0 : Persistence.GetCustomWorldAttempts(level.Hash);
        }
    }

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Last hit timing (ms)")] public static double TimingMs => GameplayState.Timing;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Average hit timing (ms)")] public static double TimingAvgMs => GameplayState.Timings.Count == 0 ? 0 : GameplayState.Timings.Average();
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Timing Window Scale")] public static double MarginScale => Controller?.currFloor?.marginScale ?? 1;

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
}
