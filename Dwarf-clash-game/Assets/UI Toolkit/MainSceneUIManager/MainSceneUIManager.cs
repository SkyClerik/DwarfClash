using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class MainSceneUIManager : Toolbox.Singleton<MainSceneUIManager>
    {
        private Dictionary<Type, object> _pages = new Dictionary<Type, object>();

        public Dictionary<Type, object> Pages => _pages;

        public bool TryGetPage<T>(out T page) where T : MonoBehaviour
        {
            page = null;
            if (_pages.TryGetValue(typeof(T), out object value))
            {
                if (value is T)
                    page = (T)value;

                return true;
            }

            return false;
        }
    }
}
