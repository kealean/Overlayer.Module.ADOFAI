using ADOFAI;
using Overlayer.Tag.Core;
using System;
using static scrMisc;

namespace Overlayer.Module.ADOFAI.Tag;

public static class TimingWindow {
    [Tag(Desc = "XPerfect window (ms)")]              public static double XPms => TimeBounds().XPerfect * 1000d;
    [Tag(Desc = "Inner Perfect window (ms, +-30deg)")]      public static double IPms => TimeBounds().Pure * 1000d;
    [Tag(Desc = "Outer Perfect window (ms)")]     public static double OPms => TimeBounds().Perfect * 1000d;
    [Tag(Desc = "Very window (ms)")]          public static double Vms => TimeBounds().Counted * 1000d;

    [Tag(Desc = "XPerfect window (deg)")]             public static double XPdeg => AngleBounds().XPerfect;
    [Tag(Desc = "Inner Perfect window (deg)")]              public static double IPdeg => AngleBounds().Pure;
    [Tag(Desc = "Outer Perfect window (deg)")]    public static double OPdeg => AngleBounds().Perfect;
    [Tag(Desc = "Very window (deg)")]         public static double Vdeg => AngleBounds().Counted;

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
    private const double XPerfectMin = 0.01666666753590107;

    private static double BpmTimesSpeed =>
        (scrConductor.instance?.bpm ?? 0) * (scrController.instance?.playerOne?.planetarySystem?.speed ?? 0);
    private static double Pitch => scrConductor.instance?.song?.pitch ?? 0;
    private static double MarginMult => scrController.instance?.currFloor?.nextfloor?.marginScale ?? 1d;
}
