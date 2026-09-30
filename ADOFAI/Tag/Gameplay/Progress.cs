using Overlayer.Tag.Core;
using Overlayer.Utility.Access;
using System;
using System.Collections;

namespace Overlayer.Module.ADOFAI.Tag.Gameplay;

public static class Progress {
    private static object Controller => GameAccess.Controller.Get(null);

    internal static IList Floors() {
        var maker = GameAccess.LevelMaker.Get(null);
        return maker == null ? null : GameAccess.FloorList.Get(maker) as IList;
    }

    private static readonly Type CheckpointType = Type.GetType("ffxCheckpoint, Assembly-CSharp");

    private static bool HasCheckpoint(object floor) {
        if(floor == null || CheckpointType == null) return false;
        return SafeAccess.TryCall(floor, "GetComponents", out object result, CheckpointType)
            && result is Array arr && arr.Length > 0;
    }

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Tile progress (0-1)")] public static double TileProgress => GameAccess.PercentComplete.Get(Controller, 0f);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Tile progress (%)")] public static double TileProgressPercent => TileProgress * 100d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Start progress (0-1)")] public static double StartProgress => TotalTile == 0 ? 0 : (double)StartTile / TotalTile;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Start progress (%)")] public static double StartProgressPercent => StartProgress * 100d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Best progress (0-1)")] public static double BestProgress {
        get {
            Status.GameplayState.BestProgress = Math.Max(Status.GameplayState.BestProgress, TileProgress);
            return Status.GameplayState.BestProgress;
        }
    }
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Best progress (%)")] public static double BestProgressPercent => BestProgress * 100d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Time-based progress (0-1)")] public static double ActualProgress {
        get {
            var floors = Floors();
            var floor = Controller == null ? null : GameAccess.CurrFloor.Get(Controller);
            if(floors == null || floors.Count < 2 || floor == null) return 0;
            double start = GameAccess.EntryTime.Get(floors[0]);
            double end = GameAccess.EntryTime.Get(floors[floors.Count - 1]);
            double now = GameAccess.EntryTime.Get(floor);
            return end <= start ? 0 : Math.Max(0, Math.Min(1, (now - start) / (end - start)));
        }
    }
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Time-based progress (%)")] public static double ActualProgressPercent => ActualProgress * 100d;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Checkpoints used")] public static int CheckpointsUsed => GameAccess.CheckpointsUsedCount.Get(Controller, 0);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Current checkpoint")] public static int CurCheckpoint {
        get {
            var floors = Floors();
            if(floors == null) return 0;
            int seq = Controller == null ? 0 : GameAccess.CurrentSeqID.Get(Controller);
            int count = 0;
            foreach(object floor in floors) {
                if(floor != null && GameAccess.SeqID.Get(floor) <= seq && HasCheckpoint(floor)) count++;
            }
            return count;
        }
    }
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Total checkpoints")] public static int TotalCheckpoints {
        get {
            var floors = Floors();
            if(floors == null) return 0;
            int count = 0;
            foreach(object floor in floors) {
                if(HasCheckpoint(floor)) count++;
            }
            return count;
        }
    }

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Start tile")] public static int StartTile {
        get {
            var floors = Floors();
            if(floors == null || floors.Count == 0) return 0;
            int checkpoint = GameAccess.CheckpointNum.Get(null);
            return Math.Min(checkpoint + 1, floors.Count);
        }
    }
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Current tile")] public static int CurTile
        => Controller == null ? 0 : GameAccess.CurrentSeqID.Get(Controller) + 1;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Tiles left")] public static int LeftTile => Math.Max(0, TotalTile - CurTile);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Total tiles")] public static int TotalTile => Floors()?.Count ?? 0;
}
