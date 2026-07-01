using Hollow.Input;
using Hollow.Player;
using UnityEngine;

namespace Hollow.GameCamera
{
    public class CameraLookAhead : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private HeroController heroController;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float verticalLookAheadDistance = 1.5f;
        [SerializeField] private float lookAheadSmooth = 5f;
        [SerializeField] private float sprintLookAheadMultiplier = 1.6f;
        [SerializeField] private bool useVerticalLookAhead;
        [SerializeField] private Vector3 baseOffset = new(0f, 1f, 0f);

        private void LateUpdate()
        {
            if (inputReader == null)
                return;

            var multiplier = 1f;
            if (heroController != null && heroController.IsSprinting)
                multiplier = sprintLookAheadMultiplier;

            var verticalOffset = useVerticalLookAhead
                ? inputReader.MoveInput.y * verticalLookAheadDistance
                : 0f;

            var desired = baseOffset + new Vector3(
                inputReader.MoveInput.x * lookAheadDistance * multiplier,
                verticalOffset,
                0f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, desired, lookAheadSmooth * Time.deltaTime);
        }
    }
}
