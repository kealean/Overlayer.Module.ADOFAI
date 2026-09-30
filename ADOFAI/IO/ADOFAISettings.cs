using Newtonsoft.Json.Linq;
using Overlayer.IO.Interface;

namespace Overlayer.Module.ADOFAI.IO;

public sealed class ADOFAISettings : ISettingsFile {
    public bool ShowAutoplayJudgment = false;
    public bool LinuxTextInputFix = true;
    public bool HideTitle = false;
    public bool BlockInputWhenOpened = true;
    public bool FileFeature = false;

    public JToken Serialize() {
        return new JObject {
            [nameof(ShowAutoplayJudgment)] = ShowAutoplayJudgment,
            [nameof(LinuxTextInputFix)] = LinuxTextInputFix,
            [nameof(HideTitle)] = HideTitle,
            [nameof(BlockInputWhenOpened)] = BlockInputWhenOpened,
            [nameof(FileFeature)] = FileFeature,
        };
    }

    public void Deserialize(JToken token) {
        ShowAutoplayJudgment = Read(token, nameof(ShowAutoplayJudgment), ShowAutoplayJudgment);
        LinuxTextInputFix = Read(token, nameof(LinuxTextInputFix), LinuxTextInputFix);
        HideTitle = Read(token, nameof(HideTitle), HideTitle);
        HideTitle = Read(token, nameof(HideTitle), HideTitle);
        FileFeature = Read(token, nameof(FileFeature), FileFeature);
    }

    private static T? Read<T>(JToken token, string key, T fallback) {
        var value = token[key];

        if(value == null) {
            return fallback;
        }

        try {
            return value.Value<T>();
        } catch {
            return fallback;
        }
    }
}
