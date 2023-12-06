using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class SkinScreenController : MonoBehaviour, IScreen
    {
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null)
        {
            ViewManager.SetBackgroundVisiblity(true);

        }
        public void Hide()
        {

        }
    }
}

