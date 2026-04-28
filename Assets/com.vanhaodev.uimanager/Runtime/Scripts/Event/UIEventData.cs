using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.uimanager.events
{
    public struct UIEventData
    {
        public string Key;
        public GameObject Target;
        public List<object> Args;
    }
}