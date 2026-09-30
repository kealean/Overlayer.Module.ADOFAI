using Overlayer.Tag.Core;
using Overlayer.Utility.Access;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Overlayer.Module.ADOFAI.Tag.Judgment;

public static class Combo {
    private static IList Current {
        get {
            var controller = GameAccess.Controller.Get(null);
            var player = controller == null ? null : GameAccess.PlayerOne.Get(controller);
            var tracker = player == null ? null : GameAccess.MarginTracker.Get(player);
            var list = tracker == null ? null : GameAccess.HitMargins.Get(tracker);
            return list as IList ?? Array.Empty<object>();
        }
    }

    public static int ComboValue => Tail(Current, IsPerfect);
    [Tag(Name = "Combo", TagType = TagType.BlockOnNotPlaying, Desc = "Current combo")] public static int ComboTag => ComboValue;
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Max combo")] public static int MaxCombo => MaxRun(Current, IsPerfect);
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Current combo of a judgment")] public static int MarginCombo(string margin) => Tail(Current, Matches(Parse(margin)));
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Max combo of a judgment")] public static int MarginMaxCombo(string margin) => MaxRun(Current, Matches(Parse(margin)));
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Current combo of judgments (a|b|...)")] public static int MarginCombos(string margins) => Tail(Current, Matches(ParseMany(margins)));
    [Tag(TagType = TagType.ProcessFormat | TagType.BlockOnNotPlaying, Desc = "Max combo of judgments (a|b|...)")] public static int MarginMaxCombos(string margins) => MaxRun(Current, Matches(ParseMany(margins)));

    internal static int Tail(IList values, Func<object, bool> matches) {
        int count = 0;
        for(int i = values.Count - 1; i >= 0 && matches(values[i]); i--) count++;
        return count;
    }

    internal static int MaxRun(IList values, Func<object, bool> matches) {
        int best = 0, current = 0;
        foreach(object value in values) {
            current = matches(value) ? current + 1 : 0;
            if(current > best) best = current;
        }
        return best;
    }

    private static Func<object, bool> Matches(HashSet<object> set) => set.Contains;

    private static HashSet<object> Parse(string margin) {
        var set = new HashSet<object>();
        var value = GameAccess.ParseHitMargin(margin);
        if(value != null) set.Add(value);
        return set;
    }

    private static HashSet<object> ParseMany(string margins) {
        var set = new HashSet<object>();
        foreach(string value in (margins ?? string.Empty).Split('|')) {
            var parsed = GameAccess.ParseHitMargin(value.Trim());
            if(parsed != null) set.Add(parsed);
        }
        return set;
    }

    private static bool IsPerfect(object margin) {
        if(margin == null) return false;
        string name = margin.ToString();
        return name is "PerfectMinus" or "PerfectPlus" or "XPerfect" or "Auto";
    }
}
