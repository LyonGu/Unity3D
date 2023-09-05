// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

//地面角色移动脚本，主角是用这个
namespace PlatformerGameKit.Characters
{
    /// <summary>Moves a ground-based <see cref="Character"/>.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/physics#ground-character-movement">
    /// Physics - Ground Character Movement</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters/GroundCharacterMovement
    /// 
    [AddComponentMenu(Character.MenuPrefix + "Ground Character Movement")]
    [HelpURL(Character.APIDocumentation + nameof(GroundCharacterMovement))]
    public sealed class GroundCharacterMovement : CharacterMovement
    {
        /************************************************************************************************************************/

        [SerializeField, MetersPerSecond] private float _WalkSpeed = 6;
        [SerializeField, MetersPerSecond] private float _RunSpeed = 9;
        [SerializeField, Seconds] private float _WalkSmoothing = 0;
        [SerializeField, Seconds] private float _RunSmoothing = 0.15f;
        [SerializeField, Seconds] private float _AirSmoothing = 0.3f;
        [SerializeField, Seconds] private float _FrictionlessSmoothing = 0.3f; //适用于光滑表面
        [SerializeField] private float _GripFriction = 0.4f;

        private float _SmoothingSpeed;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();
            PlatformerUtilities.NotNegative(ref _WalkSpeed);
            PlatformerUtilities.NotNegative(ref _RunSpeed);
            PlatformerUtilities.NotNegative(ref _WalkSmoothing);
            PlatformerUtilities.NotNegative(ref _RunSmoothing);
            PlatformerUtilities.NotNegative(ref _AirSmoothing);
            PlatformerUtilities.NotNegative(ref _FrictionlessSmoothing);
            PlatformerUtilities.NotNegative(ref _GripFriction);
        }
#endif

        /************************************************************************************************************************/

        protected override Vector2 UpdateVelocity(Vector2 velocity)
        {
            var brainMovement = Character.MovementDirection.x;
            var currentState = Character.StateMachine.CurrentState;

            var targetSpeed = Character.Run ? _RunSpeed : _WalkSpeed;
            //用这个MovementSpeedMultiplier变量控制是否能水平移动，妙啊，不同状态可以重载
            targetSpeed *= brainMovement * currentState.MovementSpeedMultiplier;

            if (!Character.Body.IsGrounded)
            {
                //在空中的速度计算
                velocity.x = PlatformerUtilities.SmoothDamp(velocity.x, targetSpeed, ref _SmoothingSpeed, _AirSmoothing);
                return velocity;
            }

            var direction = Vector2.right;
            var ground = Character.Body.GroundContact;

            //计算平滑系数，根据地面摩擦力
            var smoothing = CalculateGroundSmoothing(ground.Collider.friction);

            // Calculate the horizontal speed, excluding the movement of the platform.
            var platformVelocity = ground.Velocity;
            velocity -= platformVelocity;
            var currentSpeed = Vector2.Dot(direction, velocity);

            // Remove the old horizontal speed from the velocity.
            velocity -= direction * currentSpeed;

            // Move the horizontal speed towards the target.
            currentSpeed = PlatformerUtilities.SmoothDamp(currentSpeed, targetSpeed, ref _SmoothingSpeed, smoothing);

            // Add the new horizontal speed and platform velocity back into the actual velocity.
            velocity += direction * currentSpeed + platformVelocity;

            return velocity;
        }

        /************************************************************************************************************************/

        //确定角色需要多长时间才能达到所需的速度
        /*
         * 0 给出即时加速度。每当您改变方向时，您都会立即开始全速移动
         * 0.15 会使角色在改变方向时滑动一点，这样你在跑步时的精细控制就会减少
         * 0.3 会导致在冰上和空气中产生更多滑动
         */
        /// <summary>Calculates the speed smoothing time based on the running state and contact friction.</summary>
        private float CalculateGroundSmoothing(float friction)
        {
            var target = Character.Run ? _RunSmoothing : _WalkSmoothing;
            if (_GripFriction == 0)
                return target;

            return Mathf.Lerp(_FrictionlessSmoothing, target, friction / _GripFriction);
        }

        /************************************************************************************************************************/
    }
}
