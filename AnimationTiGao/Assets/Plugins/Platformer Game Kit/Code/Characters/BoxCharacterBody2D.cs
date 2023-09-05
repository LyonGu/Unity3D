// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System.Collections.Generic;
using UnityEngine;
using static Animancer.Validate;

//玩家主角：检测是否在地上，使用物理引擎驱动
namespace PlatformerGameKit.Characters
{
    /// <summary>Moves a ground-based <see cref="Character"/> with a <see cref="BoxCollider2D"/>.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/physics#character-body">
    /// Physics - Character Body</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters/BoxCharacterBody2D
    /// 
    [AddComponentMenu(Character.MenuPrefix + "Box Character Body 2D")]
    [HelpURL(Character.APIDocumentation + nameof(BoxCharacterBody2D))]
    public sealed class BoxCharacterBody2D : CharacterBody2D
    {
        /************************************************************************************************************************/

        [SerializeField, Range(0, 90)]
        private float _GripAngle = 49;

        /// <inheritdoc/> 确定斜坡有多陡才能仍算作地面
        /*
         *确定斜坡有多陡才能仍算作地面。 如果比这个陡峭，角色就会像墙一样滑落。
         * 该值以度为单位测量。 这并没有真正在示例场景中使用，因为它们没有任何斜坡。
         * 
         */
        public override float GripAngle
        {
            get => _GripAngle;
            set
            {
                _GripAngle = value;
                _GripProductThreshold = Mathf.Cos(value * Mathf.Deg2Rad);
            }
        }

        private float _GripProductThreshold;

        /************************************************************************************************************************/
        /*
         *当角色接地并尝试朝上方或下方此距离内的表面移动时，他们会自动捕捉到该表面并保持接地状态，而不是被其阻挡（如墙壁）或空中（如壁架）。
         * 该值应略大于台阶的实际高度，以允许物理引擎使用的浮点数学存在微小的误差。
         *
         * 爬坡高度
         */
        [SerializeField, Meters]
        private float _StepHeight = 0.55f;

        /// <inheritdoc/>
        public override float StepHeight => _StepHeight;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Enabling this adds a small performance cost to more reliably prevent the character from moving into" +
            " areas where it doesn't quite fit. Otherwise attempting to move into such an area can cause them to jitter" +
            " back and forth.")]
        private bool _VerifyAvailableSpace;

        /// <summary>
        /// Enabling this adds a small performance cost to more reliably prevent the object from moving into areas
        /// where it doesn't quite fit. Otherwise attempting to move into such an area can cause it to jitter back and
        /// forth.
        /// </summary>
        public ref bool VerifyAvailableSpace => ref _VerifyAvailableSpace;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        [SerializeField]
        [Seconds(Rule = Value.IsNotNegative)]
        [Tooltip(Strings.DebugLineDurationTooltip)]
        private float _DebugLineDuration;

        /// <summary>[Editor-Only] Determines how long scene view debug lines are shown for this object.</summary>
        public ref float DebugLineDuration => ref _DebugLineDuration;
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// A copy of the <see cref="CharacterBody2D.TerrainFilter"/> with the addition of an angle filter using the
        /// <see cref="GripAngle"/>.
        /// </summary>
        private ContactFilter2D _GroundFilter;

        private void InitializeGroundFilter()
        {
            const float UpAngle = 90;// Would change if it needs to support rotated gravity.
            _GroundFilter = TerrainFilter;
            _GroundFilter.SetNormalAngle(UpAngle - _GripAngle, UpAngle + _GripAngle);
        }

        /************************************************************************************************************************/

        private float _GravityScale;

        /// <inheritdoc/>
        public override Vector2 Gravity => Physics2D.gravity * _GravityScale;

        /************************************************************************************************************************/
        //接触地面
        private PlatformContact2D _GroundContact;

        /// <inheritdoc/>
        public override PlatformContact2D GroundContact => _GroundContact;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();

            if (Rigidbody != null && enabled && !Rigidbody.freezeRotation)
                Rigidbody.freezeRotation = true;

            if (Collider != null && Collider.bounds.size.y > 0)
                PlatformerUtilities.Clamp(ref _StepHeight, 0, Collider.bounds.size.y * 0.5f);
            else
                PlatformerUtilities.NotNegative(ref _StepHeight);

            PlatformerUtilities.Clamp(ref _GripAngle, 0, 90);

            PlatformerUtilities.NotNegative(ref _DebugLineDuration);

            GripAngle = GripAngle;
            InitializeGroundFilter();
        }
#endif

        /************************************************************************************************************************/

        protected override void Awake()
        {
            base.Awake();

            GripAngle = GripAngle;
            InitializeGroundFilter();

            OnGroundedChanged += (isGrounded) =>
            {
                // When grounded, store the old gravity scale and zero it.
                if (isGrounded)
                {
                    _GravityScale = Rigidbody.gravityScale;
                    Rigidbody.gravityScale = 0;
                }
                else// Then revert it when becoming airborne again.
                {
                    Rigidbody.gravityScale = _GravityScale;
                    _GroundContact = default;
                }
            };
        }

        /************************************************************************************************************************/

        private bool _HasUpdatedSinceLastCollision;

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            if (_HasUpdatedSinceLastCollision)
            {
                if (Rigidbody.IsAwake())
                    IsGrounded = false;
            }
            else if (IsGrounded && TrySnapToGround())
            {
                var velocity = Velocity;
                velocity.y = 0;
                Velocity = velocity; //设置速度 物理引擎驱动移动
                IsGrounded = true;
            }
            else
            {
                _HasUpdatedSinceLastCollision = true;
            }
        }

        /************************************************************************************************************************/

        private static readonly List<ContactPoint2D> Contacts = new List<ContactPoint2D>(16);

        private void OnCollisionEnter2D(Collision2D collision) => OnCollisionStay2D(collision);

        private void OnCollisionStay2D(Collision2D collision)
        {
            _HasUpdatedSinceLastCollision = false;
            if (IsGrounded ||
                !enabled)
                return;
            //判断是否在地上
            var count = collision.GetContacts(Contacts);
            for (int i = 0; i < count; i++)
            {
                var contact = Contacts[i];
                if (contact.normal.y >= _GripProductThreshold)
                {
                    _GroundContact = contact;
                    using (new Context<ContactPoint2D>(contact))
                        IsGrounded = true;

#if UNITY_EDITOR
                    PlatformerUtilities.DrawRay(
                        contact, new Color(0, 1, 0.5f), _DebugLineDuration);
#endif
                    return;
                }
#if UNITY_EDITOR
                else
                {
                    PlatformerUtilities.DrawRay(
                        contact, new Color(0, 0.75f, 0), _DebugLineDuration);
                }
#endif
            }
        }

        /************************************************************************************************************************/

        private bool TrySnapToGround()
        {
            // Don't even bother casting if the step height is too small because it would be super unreliable anyway.
            if (_StepHeight <= Physics2D.defaultContactOffset)
                return false;

            var position = Position;
            var velocity = Velocity;

            var size = Collider.bounds.size;

            var castDistance = _StepHeight * 2;
            var castSize = new Vector2(size.x, size.y - castDistance);

            var origin = position;
            origin.x += velocity.x * Time.deltaTime;
            origin.y += _StepHeight;
            origin.y += castSize.y * 0.5f;

            // Find where to snap to the ground.

#if UNITY_EDITOR
            PlatformerUtilities.DrawBoxCast(
                origin, castSize, Vector2.down * castDistance, new Color(0, 1, 0), _DebugLineDuration);
#endif

            var count = Physics2D.BoxCast(
                origin, castSize, Rotation, Vector2.down, _GroundFilter, PlatformerUtilities.OneRaycastHit, castDistance);
            if (count == 0)// No Hit.
                return false;

            var hit = PlatformerUtilities.OneRaycastHit[0];

            // Exclude anything overlapping the origin.
            // This means we are running into a wall at around head height.
            if (hit.distance == 0)
            {
#if UNITY_EDITOR
                PlatformerUtilities.DrawRay(
                    hit, new Color(0.375f, 0, 0), _DebugLineDuration);
#endif

                return false;
            }

            // Available Space.
            if (_VerifyAvailableSpace && velocity.x != 0)// If we aren't moving, we obviously already fit where we are standing.
            {
                castSize.y = size.y - castSize.y;
                origin.y = hit.point.y + size.y - castSize.y * 0.5f;
                if (Physics2D.OverlapBox(origin, castSize, Rotation, _GroundFilter.layerMask) != null)
                {
#if UNITY_EDITOR
                    PlatformerUtilities.DrawBox(
                        origin, castSize, new Color(0, 0, 0.375f), _DebugLineDuration);
#endif

                    return false;
                }
            }

            _GroundContact = hit;

            var contactPoint = hit.point;
            if (_GroundContact.HasRigidbody)
                contactPoint += _GroundContact.Velocity * Time.deltaTime;

            // Snap the position if the height difference is actually notable or you are moving.
            // Otherwise while standing still, let the Rigidbody go to sleep to improve performance.
            if (velocity.x != 0 || Mathf.Abs(position.y - contactPoint.y) > Physics2D.defaultContactOffset * 2)
            {
                origin.y = contactPoint.y + Physics2D.defaultContactOffset;

                // Set the position so that corners in the way don't block the movement then call MovePosition so that
                // the Rigidbody interpolation works and so it doesn't apply the velocity again this frame.
                Position = origin;
                Rigidbody.MovePosition(origin);

#if UNITY_EDITOR
                PlatformerUtilities.DrawRay(
                    hit, new Color(0, 1, 0), _DebugLineDuration);
                PlatformerUtilities.DrawLine(
                    position, origin, new Color(0, 1, 0), _DebugLineDuration);
            }
            else
            {
                PlatformerUtilities.DrawRay(
                    hit, new Color(0.25f, 0.5f, 0.5f), _DebugLineDuration);
#endif
            }

            return true;
        }

        /************************************************************************************************************************/
    }
}
