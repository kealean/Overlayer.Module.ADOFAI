using ADOFAI;
using Overlayer.Tag.Core;
using System;
using UnityEngine;

namespace Overlayer.Module.ADOFAI.Tag.Gameplay;

public static class Song {
    private static scrController? Controller => scrController.instance;
    private static LevelData? Level => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;

    [Tag(Desc = "Song title")] public static string Title => Strip(Level?.song);
    [Tag(Desc = "Song artist")] public static string Artist => Strip(Level?.artist);
    [Tag(Desc = "Song author")] public static string Author => Strip(Level?.author);
    [Tag(Desc = "Song title (raw)")] public static string TitleRaw => Level?.song ?? string.Empty;
    [Tag(Desc = "Song artist (raw)")] public static string ArtistRaw => Level?.artist ?? string.Empty;
    [Tag(Desc = "Song author (raw)")] public static string AuthorRaw => Level?.author ?? string.Empty;

    [Tag(Desc = "Level name text")] public static string LevelNameText => Strip(Controller?.txtLevelName?.text);
    [Tag(Desc = "Level name text (raw)")] public static string LevelNameTextRaw => Controller?.txtLevelName?.text ?? string.Empty;
    [Tag(Desc = "Default text color")] public static string DefaultTextColor(bool noAlpha = false)
        => ToHex(Level?.defaultTextColor ?? UnityEngine.Color.white, noAlpha);
    [Tag(Desc = "Default text shadow color")] public static string DefaultTextShadowColor(bool noAlpha = false)
        => ToHex(Level?.defaultTextShadowColor ?? UnityEngine.Color.black, noAlpha);
    [Tag(Desc = "Level name text color")] public static string LevelNameTextColor(bool noAlpha = false)
        => ToHex(scrVfx.instance?.currentColourScheme.colourText ?? UnityEngine.Color.white, noAlpha);
    [Tag(Desc = "Level name text shadow color")] public static string LevelNameTextShadowColor(bool noAlpha = false)
        => ToHex(scrVfx.instance?.currentColourScheme.colourTextShadow ?? UnityEngine.Color.black, noAlpha);

    [Tag(Desc = "Editor pitch")] public static double EditorPitch => (Level?.pitch ?? 100) / 100d;
    [Tag(Desc = "Tile BPM (with pitch)")] public static double TileBpm => BaseBpm * CurrentSpeed * SongPitch;
    [Tag(Desc = "Current BPM (with pitch)")] public static double CurBpm => RealBpm * SongPitch;
    [Tag(Desc = "Keys per second")] public static double KPS => CurBpm / 60d;
    [Tag(Desc = "Tile BPM (pitch excluded)")] public static double TileBpmWithoutPitch => BaseBpm * CurrentSpeed;
    [Tag(Desc = "Current BPM (pitch excluded)")] public static double CurBpmWithoutPitch => RealBpm;
    [Tag(Desc = "Keys per second (pitch excluded)")] public static double KPSWithoutPitch => CurBpmWithoutPitch / 60d;

    private static double BaseBpm => Level?.bpm ?? scrConductor.instance?.bpm ?? 0;
    private static double CurrentSpeed => Controller?.currFloor?.speed ?? 0;
    private static double SongPitch => scrConductor.instance?.song?.pitch ?? 1;
    private static double RealBpm {
        get {
            var floor = Controller?.currFloor;
            if(floor?.nextfloor == null) return BaseBpm * CurrentSpeed;
            double duration = floor.nextfloor.entryTime - floor.entryTime;
            return duration <= 0 ? BaseBpm * CurrentSpeed : 60d / duration;
        }
    }

    private static string Strip(string? value) => string.IsNullOrEmpty(value) ? string.Empty : RDUtils.RemoveRichTags(value);
    private static string ToHex(UnityEngine.Color color, bool noAlpha)
        => noAlpha ? ColorUtility.ToHtmlStringRGB(color) : ColorUtility.ToHtmlStringRGBA(color);
}
