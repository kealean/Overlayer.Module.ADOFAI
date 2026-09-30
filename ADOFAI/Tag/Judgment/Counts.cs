using Overlayer.Tag.Core;
using System;

namespace Overlayer.Module.ADOFAI.Tag.Judgment;

public static class Counts {
    private static scrMarginTracker? Tracker => scrController.instance?.playerOne?.marginTracker;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Too Early")]     public static int TE => CurrentCount(HitMargin.TooEarly);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Very Early")]    public static int VE => CurrentCount(HitMargin.VeryEarly);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Early Perfect")] public static int EP => CurrentCount(HitMargin.EarlyPerfect);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Perfect Minus")] public static int PM => CurrentCount(HitMargin.PerfectMinus);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "XPerfect")]      public static int XP => CurrentCount(HitMargin.XPerfect) + A;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Perfect Plus")]  public static int PP => CurrentCount(HitMargin.PerfectPlus);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Late Perfect")]  public static int LP => CurrentCount(HitMargin.LatePerfect);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Very Late")]     public static int VL => CurrentCount(HitMargin.VeryLate);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Too Late")]      public static int TL => CurrentCount(HitMargin.TooLate);

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Auto")]                           public static int A => CurrentCount(HitMargin.Auto);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Pure XPerfect (excluding Auto)")] public static int PXP => CurrentCount(HitMargin.XPerfect);

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Perfect (XP + IP)")]                  public static int P => XP + IP;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Fast (TE + VE + EP + PM)")]           public static int Fast => TE + VE + EP + PM;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Slow (PP + LP + VL + TL)")]           public static int Slow => PP + LP + VL + TL;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Inner Perfects (PM + PP)")]           public static int IP => PM + PP;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Outer Perfects (EP + LP)")]           public static int OP => EP + LP;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Very Early & Very Late (VE + VL)")]   public static int V => VE + VL;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Too Early & Too Late (TE + TL)")]     public static int T => TE + TL;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Number of Misses")]       public static int Miss => CurrentCount(HitMargin.FailMiss);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Number of Overloads")]    public static int Overload => CurrentCount(HitMargin.FailOverload);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Total Deaths/Fails")]     public static int Fail => Tracker?.GetDeaths() ?? 0;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Number of Multipresses")] public static int Multipress => CurrentCount(HitMargin.Multipress);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Number of OverPress")]    public static int OverPress => CurrentCount(HitMargin.OverPress);

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Accuracy (0-1)")]  public static double Accuracy => Ratio(Tracker?.percentAcc);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "XAccuracy (0-1)")] public static double XAccuracy => Ratio(Tracker?.percentXAcc);
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Accuracy (%)")]  public static double AccuracyPercent => Accuracy * 100d;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "XAccuracy (%)")] public static double XAccuracyPercent => XAccuracy * 100d;

    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "X-Score")]       public static int XScore => Tracker?.xScore ?? 0;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Max X-Score")]   public static int MaxXScore => Tracker?.maxXScore ?? 0;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Last X-Score")]  public static int LastXScore => Tracker?.lastXScore ?? 0;

    private static int CurrentCount(HitMargin margin) => Tracker?.GetHits(margin) ?? 0;
    private static double Ratio(float? value) => value ?? double.NaN;
}
