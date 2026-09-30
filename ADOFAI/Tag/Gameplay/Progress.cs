using ADOFAI;
using Overlayer.Tag.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Module.ADOFAI.Tag.Gameplay;

public static class Progress {
    private static scrController? Controller => scrController.instance;
    private static List<scrFloor>? Floors => scrLevelMaker.instance?.listFloors;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Tile progress (0-1)")] public static double TileProgress => Controller?.percentComplete ?? 0f;
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
            var floors = Floors;
            var floor = Controller?.currFloor;
            if(floors == null || floors.Count < 2 || floor == null) return 0;
            double start = floors[0].entryTime;
            double end = floors[^1].entryTime;
            return end <= start ? 0 : Math.Max(0, Math.Min(1, (floor.entryTime - start) / (end - start)));
        }
    }
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Time-based progress (%)")] public static double ActualProgressPercent => ActualProgress * 100d;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Checkpoints used")] public static int CheckpointsUsed => scrController.checkpointsUsed;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Current checkpoint")] public static int CurCheckpoint => Floors?.Count(floor => floor.seqID <= (Controller?.currentSeqID ?? 0) && floor.GetComponent<ffxCheckpoint>() != null) ?? 0;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Total checkpoints")] public static int TotalCheckpoints => Floors?.Count(floor => floor.GetComponent<ffxCheckpoint>() != null) ?? 0;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Start tile")] public static int StartTile => Floors == null || Floors.Count == 0 ? 0 : Math.Min(GCS.checkpointNum + 1, Floors.Count);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Current tile")] public static int CurTile => Controller == null ? 0 : Controller.currentSeqID + 1;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Tiles left")] public static int LeftTile => Math.Max(0, TotalTile - CurTile);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Total tiles")] public static int TotalTile => Floors?.Count ?? 0;
}
