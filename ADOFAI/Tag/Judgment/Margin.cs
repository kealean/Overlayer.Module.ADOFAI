using ADOFAI;
using Overlayer.Tag.Core;
using System;
using static scrMisc;

namespace Overlayer.Module.ADOFAI.Tag.Judgment;

public static class Margin {
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "XPerfect margin (ms)")] public static double MarginXPms => TimeBounds().XPerfect * 1000d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Inner Perfect margin (ms, +-30deg)")] public static double MarginIPms => TimeBounds().Pure * 1000d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Outer Perfect margin (ms)")] public static double MarginOPms => TimeBounds().Perfect * 1000d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Very margin (ms)")] public static double MarginVms => TimeBounds().Counted * 1000d;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "XPerfect margin (deg)")] public static double MarginXPdeg => AngleBounds().XPerfect;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Inner Perfect margin (deg)")] public static double MarginIPdeg => AngleBounds().Pure;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Outer Perfect margin (deg)")] public static double MarginOPdeg => AngleBounds().Perfect;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Very margin (deg)")] public static double MarginVdeg => AngleBounds().Counted;

    [Tag(Desc = "Manual margin (ms): judgment, bpm, speed = 1, scale = 1")]
    public static double MarginCalcMs(HitMargin judgment, double bpm, double speed = 1, double scale = 1)
        => PickManual(judgment, ManualTimeBounds(bpm, speed, scale)) * 1000d;

    [Tag(Desc = "Manual margin (deg): judgment, bpm, speed = 1, scale = 1")]
    public static double MarginCalcDeg(HitMargin judgment, double bpm, double speed = 1, double scale = 1)
        => PickManual(judgment, ManualAngleBounds(bpm, speed, scale));

    private static double PickManual(HitMargin judgment, scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> bounds)
        => judgment switch {
            HitMargin.XPerfect => bounds.XPerfect,
            HitMargin.PerfectMinus or HitMargin.PerfectPlus => bounds.Pure,
            HitMargin.EarlyPerfect or HitMargin.LatePerfect => bounds.Perfect,
            HitMargin.VeryEarly or HitMargin.VeryLate => bounds.Counted,
            HitMargin.TooEarly or HitMargin.TooLate => double.PositiveInfinity,
            _ => double.NaN,
        };

    private static scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> ManualTimeBounds(double bpm, double speed, double scale) {
        var mins = ManualMinimums(speed);
        double effBpm = bpm * speed;
        double pitch = Pitch;
        return new scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> {
            Counted = Math.Max(mins.Counted, AngleToTime(60d * Deg2Rad, effBpm) / pitch * scale),
            Perfect = Math.Max(mins.Perfect, AngleToTime(45d * Deg2Rad, effBpm) / pitch * scale),
            Pure = Math.Max(mins.Pure, AngleToTime(30d * Deg2Rad, effBpm) / pitch * scale),
            XPerfect = Math.Max(XPerfectMin, AngleToTime(12.5d * Deg2Rad, effBpm) / pitch * scale),
        };
    }

    private static scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> ManualAngleBounds(double bpm, double speed, double scale) {
        var mins = ManualMinimums(speed);
        double effBpm = bpm * speed;
        double pitch = Pitch;
        return new scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> {
            Counted = Math.Max(60d * scale, TimeToAngleInRad(mins.Counted, effBpm, pitch) * Rad2Deg),
            Perfect = Math.Max(45d * scale, TimeToAngleInRad(mins.Perfect, effBpm, pitch) * Rad2Deg),
            Pure = Math.Max(30d * scale, TimeToAngleInRad(mins.Pure, effBpm, pitch) * Rad2Deg),
            XPerfect = Math.Max(12.5d * scale, TimeToAngleInRad(XPerfectMin, effBpm, pitch) * Rad2Deg),
        };
    }

    private static scrMisc.HitMarginGeneralValuesStruct<double> ManualMinimums(double speed) {
        double countedBase = GCS.difficulty switch {
            Difficulty.Lenient => 0.091,
            Difficulty.Normal => 0.065,
            Difficulty.Strict => 0.04,
            _ => 0.065,
        };
        return new scrMisc.HitMarginGeneralValuesStruct<double> {
            Counted = Math.Max(countedBase / speed, HardCap),
            Perfect = Math.Max(0.03 / speed, HardCap),
            Pure = Math.Max(0.02 / speed, HardCap),
        };
    }

    private static scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> TimeBounds() {
        var mins = GetMinimumTimes(GCS.difficulty);
        double bpm = BpmTimesSpeed;
        double pitch = Pitch;
        double mult = MarginMult;
        if(bpm <= 0 || pitch <= 0) return default;
        return new scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> {
            Counted = Math.Max(mins.Counted, AngleToTime(60d * Deg2Rad, bpm) / pitch * mult),
            Perfect = Math.Max(mins.Perfect, AngleToTime(45d * Deg2Rad, bpm) / pitch * mult),
            Pure = Math.Max(mins.Pure, AngleToTime(30d * Deg2Rad, bpm) / pitch * mult),
            XPerfect = Math.Max(XPerfectMin, AngleToTime(12.5d * Deg2Rad, bpm) / pitch * mult),
        };
    }

    private static scrMisc.HitMarginGeneralWithXPerfectValuesStruct<double> AngleBounds() {
        double bpm = BpmTimesSpeed;
        double pitch = Pitch;
        if(bpm <= 0 || pitch <= 0) return default;
        return GetAdjustedAngleBoundaryInDeg(GCS.difficulty, BpmTimesSpeed, Pitch, MarginMult);
    }

    private const double Deg2Rad = 0.01745329238474369;
    private const double Rad2Deg = 57.295780181884766;
    private const double XPerfectMin = 0.01666666753590107;
    private const double HardCap = 0.025;

    private static double BpmTimesSpeed =>
        (scrConductor.instance?.bpm ?? 0) * (scrController.instance?.playerOne?.planetarySystem?.speed ?? 0);
    private static double Pitch => scrConductor.instance?.song?.pitch ?? 0;
    private static double MarginMult => scrController.instance?.currFloor?.nextfloor?.marginScale ?? 1d;
}
