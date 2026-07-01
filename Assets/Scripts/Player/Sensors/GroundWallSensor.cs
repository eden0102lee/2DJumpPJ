using UnityEngine;

namespace Hollow.Player
{
    public class GroundWallSensor : MonoBehaviour
    {
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform leftWallCheck;
        [SerializeField] private Transform rightWallCheck;
        [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.05f);
        [SerializeField] private Vector2 wallCheckSize = new(0.05f, 0.8f);
        [SerializeField] private float wallCheckOffset = 0.4f;
        [SerializeField] private LayerMask groundLayer;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingLeftWall { get; private set; }
        public bool IsTouchingRightWall { get; private set; }
        public bool IsTouchingWall => IsTouchingLeftWall || IsTouchingRightWall;
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

            if (leftWallCheck == null)
            {
                var leftObj = new GameObject("LeftWallCheck");
                leftObj.transform.SetParent(transform);
                leftObj.transform.localPosition = new Vector3(-wallCheckOffset, 0f, 0f);
                leftWallCheck = leftObj.transform;
            }

            if (rightWallCheck == null)
            {
                var rightObj = new GameObject("RightWallCheck");
                rightObj.transform.SetParent(transform);
                rightObj.transform.localPosition = new Vector3(wallCheckOffset, 0f, 0f);
                rightWallCheck = rightObj.transform;
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

            IsTouchingLeftWall = Physics2D.OverlapBox(leftWallCheck.position, wallCheckSize, 0f, groundLayer);
            IsTouchingRightWall = Physics2D.OverlapBox(rightWallCheck.position, wallCheckSize, 0f, groundLayer);

            if (IsTouchingLeftWall && IsTouchingRightWall)
                WallDirection = facingDirection;
            else if (IsTouchingLeftWall)
                WallDirection = -1;
            else if (IsTouchingRightWall)
                WallDirection = 1;
            else
                WallDirection = 0;
        }

        public int GetWallDirectionFromInput(float moveInputX)
        {
            if (Mathf.Abs(moveInputX) < 0.01f)
                return 0;

            var inputDir = moveInputX > 0f ? 1 : -1;
            if (inputDir < 0 && IsTouchingLeftWall)
                return -1;
            if (inputDir > 0 && IsTouchingRightWall)
                return 1;

            return 0;
        }

        public bool IsTouchingWallInDirection(int direction)
        {
            if (direction < 0)
                return IsTouchingLeftWall;
            if (direction > 0)
                return IsTouchingRightWall;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }

            if (leftWallCheck != null)
            {
                Gizmos.color = IsTouchingLeftWall ? Color.cyan : Color.yellow;
                Gizmos.DrawWireCube(leftWallCheck.position, wallCheckSize);
            }

            if (rightWallCheck != null)
            {
                Gizmos.color = IsTouchingRightWall ? Color.cyan : Color.yellow;
                Gizmos.DrawWireCube(rightWallCheck.position, wallCheckSize);
            }
        }
    }
}
