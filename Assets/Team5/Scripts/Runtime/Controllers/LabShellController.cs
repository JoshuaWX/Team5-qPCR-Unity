using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Team5.qPCR
{
    public enum InteractionMode
    {
        Desktop,
        XR
    }

    public sealed class LabShellController : MonoBehaviour
    {
        [SerializeField] private GameObject removableCeiling;
        [SerializeField] private GameObject upperFrontWall;
        [SerializeField] private GameObject desktopCameraRoot;
        [SerializeField] private GameObject xrOriginRoot;
        [SerializeField] private GameObject xrSimulatorRoot;
        [SerializeField] private GameObject desktopUiRoot;
        [SerializeField] private GameObject xrWorldUiRoot;
        [SerializeField] private InteractionMode startingMode = InteractionMode.Desktop;

        public InteractionMode CurrentMode { get; private set; }
        private bool desktopOverview = true;

        public void SetDesktopOverview(bool overview)
        {
            desktopOverview = overview;
            var enclosed = CurrentMode == InteractionMode.XR || !overview;
            if (removableCeiling != null) removableCeiling.SetActive(enclosed);
            if (upperFrontWall != null) upperFrontWall.SetActive(enclosed);
        }

        public void Configure(
            GameObject ceiling,
            GameObject frontWall,
            GameObject desktopRoot,
            GameObject xrRoot,
            GameObject simulatorRoot)
        {
            Configure(ceiling, frontWall, desktopRoot, xrRoot, simulatorRoot, null, null);
        }

        public void Configure(
            GameObject ceiling,
            GameObject frontWall,
            GameObject desktopRoot,
            GameObject xrRoot,
            GameObject simulatorRoot,
            GameObject desktopUi,
            GameObject xrWorldUi)
        {
            removableCeiling = ceiling;
            upperFrontWall = frontWall;
            desktopCameraRoot = desktopRoot;
            xrOriginRoot = xrRoot;
            xrSimulatorRoot = simulatorRoot;
            desktopUiRoot = desktopUi;
            xrWorldUiRoot = xrWorldUi;
        }

        private void Start()
        {
            var mode = startingMode;
#if !UNITY_EDITOR
            if (XRSettings.isDeviceActive)
            {
                mode = InteractionMode.XR;
            }
#endif
            SetMode(mode);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (Application.isEditor && keyboard != null && keyboard.f8Key.wasPressedThisFrame)
            {
                SetMode(CurrentMode == InteractionMode.Desktop ? InteractionMode.XR : InteractionMode.Desktop);
            }
        }

        public void SetMode(InteractionMode mode)
        {
            CurrentMode = mode;
            var xr = mode == InteractionMode.XR;
            if (removableCeiling != null)
            {
                removableCeiling.SetActive(xr || !desktopOverview);
            }

            if (upperFrontWall != null)
            {
                upperFrontWall.SetActive(xr || !desktopOverview);
            }

            if (desktopCameraRoot != null)
            {
                desktopCameraRoot.SetActive(!xr);
            }

            if (xrOriginRoot != null)
            {
                xrOriginRoot.SetActive(xr);
            }

            if (xrSimulatorRoot != null)
            {
                xrSimulatorRoot.SetActive(xr && Application.isEditor);
            }

            if (desktopUiRoot != null)
            {
                desktopUiRoot.SetActive(!xr);
            }

            if (xrWorldUiRoot != null)
            {
                xrWorldUiRoot.SetActive(xr);
            }
        }
    }
}
