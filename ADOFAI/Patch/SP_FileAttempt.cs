using HarmonyLib;
using Overlayer.Module.ADOFAI.IO.File;
using Overlayer.Patch.Safe;
using System.Reflection;

namespace Overlayer.Module.ADOFAI.Patch;

public class SP_FileAttemptLoad() : SafeConditionalPatch(nameof(SP_FileAttemptLoad)) {
    protected override bool ShouldApply() => Core.Config.FileFeature;

    protected override MethodBase GetTargetMethod()
        => SafePatch.GetMethodSafe("scnGame", "LoadLevel");

    protected override HarmonyMethod Postfix() => new HarmonyMethod(typeof(SP_FileAttemptLoad)
        .GetMethod(nameof(PostfixImpl), BindingFlags.Static | BindingFlags.NonPublic));

    private static void PostfixImpl() => FileStoreState.Current.Load();
}

public class SP_FileAttemptPlay() : SafeConditionalPatch(nameof(SP_FileAttemptPlay)) {
    protected override bool ShouldApply() => Core.Config.FileFeature;

    protected override MethodBase GetTargetMethod()
        => SafePatch.GetMethodSafe("scnGame", "Play");

    protected override HarmonyMethod Postfix() => new HarmonyMethod(typeof(SP_FileAttemptPlay)
        .GetMethod(nameof(PostfixImpl), BindingFlags.Static | BindingFlags.NonPublic));

    private static void PostfixImpl(int seqID) {
        FileStoreState.Current.Data.Increase(seqID);
        FileStoreState.Current.Save();
    }
}
