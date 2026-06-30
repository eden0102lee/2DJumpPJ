using UnityEngine;

namespace Hollow.Player
{
    public class GroundWallSensor : MonoBehaviour
    {
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform wallCheck;
        [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.05f);
        [SerializeField] private Vector2 wallCheckSize = new(0.05f, 0.8f);
        [SerializeField] private float wallCheckOffset = 0.4f;
        [SerializeField] private LayerMask groundLayer;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }
        public int WallDirection { get; private set; }
        public Vector2 GroundNormal { get; private set; }

        private void Awake()
        {
            if (groundCheck == null)
            {
                var groundObj = new GameObject("GroundCheck");
                groundObj.transform.SetParent(transform);
                groundObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                groundCheck = groundObj.transform;
            }

            if (wallCheck == null)
            {
                var wallObj = new GameObject("WallCheck");
                wallObj.transform.SetParent(transform);
                wallCheck = wallObj.transform;
            }
        }

        public void UpdateSensor(int facingDirection)
        {
            var groundPos = groundCheck.position;
            IsGrounded = Physics2D.OverlapBox(groundPos, groundCheckSize, 0f, groundLayer);

            if (IsGrounded)
            {
                var hit = Physics2D.Raycast(groundPos, Vector2.down, groundCheckSize.y + 0.1f, groundLayer);
                GroundNormal = hit.collider != null ? hit.normal : Vector2.up;
            }
            else
            {
                GroundNormal = Vector2.up;
            }

            wallCheck.localPosition = new Vector3(wallCheckOffset * facingDirection, 0f, 0f);
            var wallPos = wallCheck.position;
            IsTouchingWall = Physics2D.OverlapBox(wallPos, wallCheckSize, 0f, groundLayer);
            WallDirection = IsTouchingWall ? facingDirection : 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }

            if (wallCheck != null)
            {
                Gizmos.color = IsTouchingWall ? Color.cyan : Color.yellow;
                Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
            }
        }
    }
}
