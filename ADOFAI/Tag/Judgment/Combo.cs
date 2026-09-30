using Overlayer.Tag.Core;
using System;
using System.Collections.Generic;

namespace Overlayer.Module.ADOFAI.Tag.Judgment;

public static class Combo {
    private static IReadOnlyList<HitMargin> Current => scrController.instance?.playerOne?.marginTracker?.hitMargins is { } values
        ? values
        : Array.Empty<HitMargin>();

    public static int ComboValue => Tail(Current, IsPerfect);
    [Tag(Name = "Combo", TagType = TagType.BlockOnNotPlaying, Desc = "Current combo")] public static int ComboTag => ComboValue;
    [Tag(TagType = TagType.BlockOnNotPlaying, Desc = "Max combo")] public static int MaxCombo => MaxRun(Current, IsPerfect);
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Current combo of a judgment")] public static int MarginCombo(HitMargin margin) => Tail(Current, hit => hit == margin);
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Max combo of a judgment")] public static int MarginMaxCombo(HitMargin margin) => MaxRun(Current, hit => hit == margin);
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Current combo of judgments (a|b|...)")] public static int MarginCombos(string margins) => Tail(Current, Parse(margins));
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Max combo of judgments (a|b|...)")] public static int MarginMaxCombos(string margins) => MaxRun(Current, Parse(margins));

    internal static int Tail(IReadOnlyList<HitMargin> values, Func<HitMargin, bool> matches) {
        int count = 0;
        for(int i = values.Count - 1; i >= 0 && matches(values[i]); i--) count++;
        return count;
    }

    internal static int MaxRun(IReadOnlyList<HitMargin> values, Func<HitMargin, bool> matches) {
        int best = 0, current = 0;
        foreach(HitMargin value in values) {
            current = matches(value) ? current + 1 : 0;
            if(current > best) best = current;
        }
        return best;
    }

    private static Func<HitMargin, bool> Parse(string margins) {
        var set = new HashSet<HitMargin>();
        foreach(string value in margins.Split('|')) {
            if(Enum.TryParse(value, true, out HitMargin margin)) set.Add(margin);
        }
        return set.Contains;
    }

    private static bool IsPerfect(HitMargin margin) => margin is HitMargin.PerfectMinus or HitMargin.PerfectPlus or HitMargin.XPerfect or HitMargin.Auto;
}
