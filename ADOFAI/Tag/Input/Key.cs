using Overlayer.Tag.Core;
using SkyHook;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Overlayer.Module.ADOFAI.Tag.Input;

/// <summary>SkyHook OS-hook key state. Unlike Unity input (which this module
/// can block), SkyHook sees physical presses even while the game ignores them.</summary>
public static class Key {
    private static readonly HashSet<KeyLabel> _held = new();
    private static readonly object _lock = new();
    private static UnityAction<SkyHookEvent> _listener;
    private static bool _subscribed;

    private static void EnsureSubscribed() {
        if(_subscribed) return;
        lock(_lock) {
            if(_subscribed) return;
            var manager = SkyHookManager.Instance;
            if(manager == null) return;
            _listener = OnKeyEvent;
            SkyHookManager.KeyUpdated.AddListener(_listener);
            _subscribed = true;
        }
    }

    private static void OnKeyEvent(SkyHookEvent ev) {
        lock(_lock) {
            if(ev.Type == EventType.KeyPressed) {
                _held.Add(ev.Label);
            } else if(ev.Type == EventType.KeyReleased) {
                _held.Remove(ev.Label);
            }
        }
    }

    internal static void Shutdown() {
        lock(_lock) {
            if(!_subscribed) return;
            try {
                SkyHookManager.KeyUpdated.RemoveListener(_listener);
            } catch { }
            _listener = null;
            _held.Clear();
            _subscribed = false;
        }
    }

    [Tag(Desc = "Whether a key is currently held (SkyHook name: Space, A, LShift, LControl, LAlt, ArrowUp, Enter, Escape, ...)")]
    public static bool IsKeyDown(string key) {
        EnsureSubscribed();
        if(string.IsNullOrWhiteSpace(key)) return false;
        KeyLabel label;
        try {
            label = (KeyLabel)System.Enum.Parse(typeof(KeyLabel), key.Trim(), true);
        } catch {
            return false;
        }
        lock(_lock) {
            return _held.Contains(label);
        }
    }
}
