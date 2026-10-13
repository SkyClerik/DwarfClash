using UnityEngine.UIElements;
using UnityEngine;
using System;
using Toolbox;

namespace Gameplay.UI
{
    public class UserInterfaceShare : Singleton<UserInterfaceShare>
    {
        public Action<UIDocument> OnOpenPage;

        public void PageOpen(UIDocument uIDocument)
        {
            Debug.Log($"[Event] Пользователь открывает новую вкладку");
            OnOpenPage?.Invoke(uIDocument);
        }
    }
}
