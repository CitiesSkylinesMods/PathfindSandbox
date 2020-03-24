using UnityEngine;

namespace PathfindSandbox.UI {
    public class SelectionManager {
        private static SelectionManager _manager;

        public static SelectionManager Manager {
            get => _manager ?? (_manager = new SelectionManager());
            set => _manager = value;
        }

        private PopupWindow _activePopup;

        public bool IsWindowActive => _activePopup != null;
        
        public void ActivateWindow(int id, PopupWindow popup) {
            _activePopup = popup;
        }

        public void SetValue(string value) {
            _activePopup?.Data.UpdateInput(value);
        }

        public void MoveFocus(bool forward) {
            if (forward) {
                _activePopup?.Data.FocusNextInput();
            } else {
                _activePopup?.Data.FocusPreviousInput();
            }
        }
    }
}