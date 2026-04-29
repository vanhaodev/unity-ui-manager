using System;
using System.Threading.Tasks;
using UnityEngine;

namespace vanhaodev.uimanager
{
    public partial class UIManager
    {
        [SerializeField] private Transform _loadingBlockLayer;

        private BaseLoadingBlock _loadingBlockInstance;
        private int _loadingBlockCounter;

        public int LoadingBlockCounter => _loadingBlockCounter;
        public bool IsLoadingBlockActive => _loadingBlockCounter > 0;

        public LoadingBlockHandle LoadingBlock<T>(Action<T> onSetup = null) where T : BaseLoadingBlock
        {
            BeginLoadingBlock(onSetup);
            return new LoadingBlockHandle(this);
        }

        public async Task LoadingBlock<T>(Func<Task> action, Action<T> onSetup = null) where T : BaseLoadingBlock
        {
            BeginLoadingBlock(onSetup);
            try
            {
                await action();
            }
            finally
            {
                EndLoadingBlock();
            }
        }

        public async Task<TResult> LoadingBlock<T, TResult>(Func<Task<TResult>> action, Action<T> onSetup = null) where T : BaseLoadingBlock
        {
            BeginLoadingBlock(onSetup);
            try
            {
                return await action();
            }
            finally
            {
                EndLoadingBlock();
            }
        }

        private void BeginLoadingBlock<T>(Action<T> onSetup) where T : BaseLoadingBlock
        {
            _loadingBlockCounter++;

            if (_loadingBlockCounter == 1)
                ShowLoadingBlockInstance(onSetup);
        }

        internal void EndLoadingBlock()
        {
            if (_loadingBlockCounter <= 0) return;

            _loadingBlockCounter--;

            if (_loadingBlockCounter == 0)
                HideLoadingBlockInstance();
        }

        private void ShowLoadingBlockInstance<T>(Action<T> onSetup) where T : BaseLoadingBlock
        {
            if (_loadingBlockInstance != null && _loadingBlockInstance.IsVisible)
                return;

            var prefab = _library?.GetLoadingBlockPrefab<T>();
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] LoadingBlock not found: {typeof(T).Name}");
                return;
            }

            CreateAndShowLoadingBlock(prefab);
            onSetup?.Invoke(_loadingBlockInstance as T);
        }

        private void CreateAndShowLoadingBlock(BaseLoadingBlock prefab)
        {
            var parent = _loadingBlockLayer != null ? _loadingBlockLayer : transform;
            _loadingBlockInstance = Instantiate(prefab, parent);
            _loadingBlockInstance.Manager = this;
            _loadingBlockInstance.Show();
        }

        private void HideLoadingBlockInstance()
        {
            if (_loadingBlockInstance == null) return;

            _loadingBlockInstance.Close(() =>
            {
                if (_loadingBlockInstance != null)
                {
                    Destroy(_loadingBlockInstance.gameObject);
                    _loadingBlockInstance = null;
                }
            });
        }

        public void ForceHideLoadingBlock()
        {
            _loadingBlockCounter = 0;
            HideLoadingBlockInstance();
        }

        private void ClearLoadingBlockCache()
        {
            _loadingBlockCounter = 0;
            if (_loadingBlockInstance != null)
            {
                Destroy(_loadingBlockInstance.gameObject);
                _loadingBlockInstance = null;
            }
        }
    }
}
