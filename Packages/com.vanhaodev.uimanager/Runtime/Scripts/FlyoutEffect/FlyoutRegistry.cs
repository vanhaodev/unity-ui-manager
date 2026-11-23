using System.Collections.Generic;

namespace vanhaodev.uimanager
{
    /// <summary>
    /// Registry for FlyoutTarget components. Owned by UIManager instance.
    /// </summary>
    public class FlyoutRegistry
    {
        private readonly Dictionary<string, FlyoutTarget> _targets = new();

        public void Register(string key, FlyoutTarget target)
        {
            if (string.IsNullOrEmpty(key)) return;
            _targets[key] = target;
        }

        public void Unregister(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            _targets.Remove(key);
        }

        public FlyoutTarget Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            _targets.TryGetValue(key, out var target);
            return target;
        }

        public bool TryGet(string key, out FlyoutTarget target)
        {
            target = null;
            if (string.IsNullOrEmpty(key)) return false;
            return _targets.TryGetValue(key, out target);
        }

        public void Clear() => _targets.Clear();
    }
}
