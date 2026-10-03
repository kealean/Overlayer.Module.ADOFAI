using Overlayer.Compat;
using Overlayer.Core;
using Overlayer.IO;
using Overlayer.Localization;
using Overlayer.Module.ADOFAI.IO;
using Overlayer.Module.ADOFAI.Patch;
using Overlayer.Module.ADOFAI.UI;
using Overlayer.ModuleAPI;
using Overlayer.Patch.Safe;
using Overlayer.Resource;
using Overlayer.UI;
using Overlayer.UI.Factory;
using Overlayer.Utility.Access;
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Overlayer.Module.ADOFAI;

public class Core : OverlayerModule {
    private IDisposable? playbackStateRegistration;
    private IDisposable? pausedStateRegistration;
    private IDisposable? textFontRegistration;
    private static TMP_FontAsset? defaultTextFont;
    public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static OverlayerLogger Logger { get; } = new(MainCore.Host.OverlayerLogger, "ADOFAI Module");
    public static SettingsFile<ADOFAISettings> ConfigFile { get; } = new(Path.Combine(MainCore.Paths.ModulePath, "ADOFAI/Settings.json"));
    public static ADOFAISettings Config => ConfigFile.Data;
    public static Translator Tr { get; private set; } = new();

    public static ResourceManager Res { get; } = new(Assembly, "Overlayer.Module.ADOFAI.Resource.Embedded.");
    public static SpriteManager Spr { get; } = new(Res);

    private static void LoadTr()
        => _ = Tr.Load(new(Path.Combine(MainCore.Paths.ModulePath, "ADOFAI/Lang")));

    private void OnLanguageChanged(string lang)
        => Tr.Language = lang;

    public static bool IsPlaying {
        get {
            var cdt = GameAccess.Conductor.Get(null);
            var ctrl = GameAccess.Controller.Get(null);
            var edt = GameAccess.ScnEditor.Get(null);

            if(cdt == null || !GameAccess.IsGameWorldFlag.Get(cdt)) {
                return false;
            }

            if(ctrl == null) {
                return false;
            }

            if(!GameAccess.Paused.Get(ctrl)) {
                return true;
            }

            return edt != null && GameAccess.EditorPausedInPlayMode.Get(edt);
        }
    }

    public override void OnInitialize() {
        Tr.SetLog(Logger.Msg);

        MainCore.Tr.OnLoadStart += LoadTr;
        MainCore.Tr.OnLanguageChanged += OnLanguageChanged;

        LoadTr();
        Tr.Language = MainCore.Tr.Language;

        ConfigFile.Load();

        SafeAccess.ModeOverride = Config.LazyAccess ? SafeResolveMode.Lazy : null;
        playbackStateRegistration = PlaybackState.Register(() => {
            return IsPlaying;
        });
        pausedStateRegistration = PlaybackState.RegisterPaused(() => {
            var controller = GameAccess.Controller.Get(null);
            return controller != null && GameAccess.Paused.Get(controller);
        });
        textFontRegistration = TextFontProvider.Register(() => {
            if(defaultTextFont == null) {
                object fontData = null;
                if(GameAccess.FontDataForLanguage.TryInvoke(null, out object result, UnityEngine.SystemLanguage.English)) {
                    fontData = result;
                }
                Font sourceFont = fontData != null && SafeAccess.TryRead(fontData, "font", out object fontObj)
                    ? fontObj as Font
                    : null;
                if(sourceFont == null) {
                    return defaultTextFont;
                }
                defaultTextFont = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    100,
                    10,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024
                );
            }
            return defaultTextFont;
        });

        GameAccess.DontShowTitles.TrySet(null, Config.HideTitle);

        SafePatchController.Add(new SP_BlockAsyncInput());
        SafePatchController.Add(new SP_BlockLegacyInput());
        SafePatchController.Add(new SP_BlockInputMethod("OptionsPanelsCLS", "CheckInputs"));
        SafePatchController.Add(new SP_BlockDirectInput("scnLevelSelect", "Update"));
        SafePatchController.Add(new SP_BlockDirectInput("scnLevelSelectTaro", "Update"));
        SafePatchController.Add(new SP_BlockDirectInput("scnTaroMenu2", "Update"));
        SafePatchController.Add(new SP_BlockDirectInput("scnTaroMenu3", "Update"));
        SafePatchController.Add(new SP_LinuxTMPKeyInput());
        SafePatchController.Add(new SP_LinuxLegacyKeyInput());
        SafePatchController.Add(new SP_ShowAutoJudgment());
        SafePatchController.Add(new SP_ResetTagState());
        SafePatchController.Add(new SP_RecordTiming());
        SafePatchController.Add(new SP_SessionAttemptLoad());
        SafePatchController.Add(new SP_SessionAttemptPlay());
        SafePatchController.Add(new SP_FileAttemptLoad());
        SafePatchController.Add(new SP_FileAttemptPlay());
        if(!Config.LazyPatches) {
            SafePatchController.ApplyAll();
        }
        // Linux input fix is never lazy: no tag triggers it, so lazy mode
        // would leave it off until the toggle is cycled. Apply() is
        // self-guarded (platform + config) and idempotent under ApplyAll.
        foreach(var patch in SafePatchController.Get<SP_LinuxTMPKeyInput>()) patch.Apply();
        foreach(var patch in SafePatchController.Get<SP_LinuxLegacyKeyInput>()) patch.Apply();

        MainCore.Cam.CustomCameraProvider = () => {
            var cam = GameAccess.Cam.Get(null);
            var camobj = cam == null ? null : GameAccess.CamObj.Get(cam);
            return camobj as UnityEngine.Camera;
        };

        MainUI.CreateInputBlocker(UICore.CanvasObj.transform);
        MainUI.CreateMenu(UICore.MenuContent);
        MainUI.CreatePage(PageFactory.CreatePageBase(100));
    }

    public override void OnDispose() {
        GameAccess.DontShowTitles.TrySet(null, false);

        Tag.Input.Key.Shutdown();

        playbackStateRegistration?.Dispose();
        playbackStateRegistration = null;
        pausedStateRegistration?.Dispose();
        pausedStateRegistration = null;
        textFontRegistration?.Dispose();
        textFontRegistration = null;

        Spr.Dispose();
        Res.Dispose();

        MainCore.Tr.OnLoadStart -= LoadTr;
        MainCore.Tr.OnLanguageChanged -= OnLanguageChanged;

        ConfigFile.Save();
    }

    public override string Name => Info.Name;
    public override string Author => Info.Author;
    public override string Version => Info.Version;
}
