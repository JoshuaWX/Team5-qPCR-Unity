using TMPro;
using UnityEngine.EventSystems;

namespace Team5.qPCR
{
    public static class DesktopInputGuard
    {
        public static bool IsEditingText
        {
            get
            {
                var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
                var input = selected == null ? null : selected.GetComponentInParent<TMP_InputField>();
                return input != null && input.isFocused;
            }
        }
        public static bool PointerOverUi => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        public static bool HasSelectedControl => EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null;
    }
}
