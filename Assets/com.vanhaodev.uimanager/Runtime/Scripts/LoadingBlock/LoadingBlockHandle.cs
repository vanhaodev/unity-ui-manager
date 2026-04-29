using System;

namespace vanhaodev.uimanager
{
    public readonly struct LoadingBlockHandle : IDisposable
    {
        private readonly UIManager _manager;

        internal LoadingBlockHandle(UIManager manager)
        {
            _manager = manager;
        }

        public void Dispose()
        {
            _manager?.EndLoadingBlock();
        }
    }
}
