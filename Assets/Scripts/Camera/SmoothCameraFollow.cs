using UnityEngine;

namespace Hollow.GameCamera
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 1f, -10f);
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
