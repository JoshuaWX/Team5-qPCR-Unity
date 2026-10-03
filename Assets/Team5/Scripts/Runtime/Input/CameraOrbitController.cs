using UnityEngine;
using UnityEngine.InputSystem;

namespace Team5.qPCR
{
    public sealed class CameraOrbitController : MonoBehaviour
    {
        [SerializeField] private Transform focus;
        [SerializeField] private float yaw = -12f;
        [SerializeField] private float pitch = 22f;
        [SerializeField] private float distance = 10.5f;
        [SerializeField] private float rotationSpeed = 0.12f;

        public void Configure(Transform focusTarget)
        {
            focus = focusTarget;
            ApplyTransform();
        }

        private void LateUpdate()
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw += delta.x * rotationSpeed;
                    pitch = Mathf.Clamp(pitch - (delta.y * rotationSpeed), 8f, 62f);
                }

                distance = Mathf.Clamp(distance - (mouse.scroll.ReadValue().y * 0.004f), 6.5f, 14f);
            }

            ApplyTransform();
        }

        private void ApplyTransform()
        {
            if (focus == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = focus.position - (rotation * Vector3.forward * distance);
            transform.rotation = rotation;
        }
    }
}
