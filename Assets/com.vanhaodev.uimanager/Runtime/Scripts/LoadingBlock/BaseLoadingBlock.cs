using System;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager
{
    public abstract class BaseLoadingBlock : UninteractableUI
    {
        private ILoadingBlockAnimation _loadingBlockAnimation;

        public void SetLoadingBlockAnimation(ILoadingBlockAnimation animation)
        {
            _loadingBlockAnimation = animation;
        }

        public override void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;

            if (_loadingBlockAnimation != null)
                _loadingBlockAnimation.PlayShow(gameObject, () => onComplete?.Invoke());
            else
                onComplete?.Invoke();
        }

        public override void Close(Action onComplete = null)
        {
            if (_loadingBlockAnimation != null)
            {
                _loadingBlockAnimation.PlayClose(gameObject, () =>
                {
                    IsVisible = false;
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else
            {
                IsVisible = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
        }
    }
}
