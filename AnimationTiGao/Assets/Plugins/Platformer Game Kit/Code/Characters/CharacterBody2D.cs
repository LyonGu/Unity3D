// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer;
using PlatformerGameKit.Characters.Brains;
using System;
using UnityEngine;
//角色移动以及碰撞检测基类，不同角色自己继承
namespace PlatformerGameKit.Characters
{
    /// <summary>Manages the <see cref="Collider2D"/> and <see cref="Rigidbody2D"/> of a <see cref="Character"/>.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/physics#character-body">
    /// Physics - Character Body</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters/CharacterBody2D
    /// 
    [AddComponentMenu(Character.MenuPrefix + "Character Body 2D")]
    [HelpURL(Character.APIDocumentation + nameof(CharacterBody2D))]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public class CharacterBody2D : MonoBehaviour
    {
        /************************************************************************************************************************/

        //执行顺序 CharacterBrain > CharacterBody2D > states
        /// <summary>Run after being controlled (CharacterBrain类) but before states react to <see cref="IsGrounded"/>.</summary>
        public const int DefaultExecutionOrder = CharacterBrain.DefaultExecutionOrder + 1000;

        /************************************************************************************************************************/

        [SerializeField]
        private Collider2D _Collider;

        /// <summary>[<see cref="SerializeField"/>] The character's <see cref="Collider2D"/>.</summary>
        public Collider2D Collider => _Collider;

        /************************************************************************************************************************/

        [SerializeField]
        private Rigidbody2D _Rigidbody;

        /// <summary>[<see cref="SerializeField"/>] The character's <see cref="Rigidbody2D"/>.</summary>
        public Rigidbody2D Rigidbody => _Rigidbody;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Ensures that all fields have valid values and finds missing components nearby.</summary>
        protected virtual void OnValidate()
        {
            gameObject.GetComponentInParentOrChildren(ref _Collider);
            gameObject.GetComponentInParentOrChildren(ref _Rigidbody);

            // Ensure that the Rigidbody is configured correctly.
            // Only set the values if they are actually different. Otherwise this can cause issues on prefabs.
            if (_Rigidbody != null && enabled)
            {
                if (_Rigidbody.bodyType != RigidbodyType2D.Dynamic)
                    _Rigidbody.bodyType = RigidbodyType2D.Dynamic;

                if (!_Rigidbody.simulated)
                    _Rigidbody.simulated = true;
            }
        }
#endif

        /************************************************************************************************************************/

        /// <summary>The <see cref="Rigidbody2D.position"/>.</summary>
        public Vector2 Position
        {
            get => _Rigidbody.position;
            set => _Rigidbody.position = value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Rigidbody2D.velocity"/>.</summary>
        public Vector2 Velocity
        {
            get => _Rigidbody.velocity;
            set => _Rigidbody.velocity = value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Rigidbody2D.mass"/>.</summary>
        public float Mass
        {
            get => _Rigidbody.mass;
            set => _Rigidbody.mass = value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Rigidbody2D.rotation"/> (currently fixed at 0).</summary>
        /// <remarks>Supporting rotation is possible, but would increase the complexity of many scripts.</remarks>
        public float Rotation
        {
            get => 0;// _Rigidbody.rotation;
            set => throw new NotSupportedException("Rotation is not supported.");// _Rigidbody.rotation = value;
        }

        /// <summary>The acceleration that gravity is currently applying to this body.</summary>
        public virtual Vector2 Gravity
            => Physics2D.gravity * _Rigidbody.gravityScale;

        /************************************************************************************************************************/

        private bool _IsGrounded;

        /// <summary>Is this body currently on the ground?</summary>
        public bool IsGrounded
        {
            get => _IsGrounded;
            set
            {
                if (_IsGrounded == value)
                    return;
                //变化了才会赋值，然后通知外部
                _IsGrounded = value;
                OnGroundedChanged?.Invoke(value);
            }
        }

        /// <summary>Called when <see cref="IsGrounded"/> is changed.</summary>
        public event Action<bool> OnGroundedChanged;

        /************************************************************************************************************************/
        //用来筛选碰撞结果的， https://docs.unity3d.com/cn/2019.2/ScriptReference/ContactFilter2D.html
        /*
         *      isFiltering	鉴于接触筛选器的当前状态，确定其是否将筛选所有结果。
                layerMask	*** 设置接触筛选器，使其筛选的结果仅包含层遮罩定义的层上的 Collider2D。
                maxDepth	设置接触筛选器，使其筛选的结果仅包含 Z 坐标（深度）小于该值的 Collider2D。
                maxNormalAngle	*** 设置接触筛选器，使其筛选的结果仅包含碰撞法线角度小于该角度的接触。
                minDepth	设置接触筛选器，使其筛选的结果仅包含 Z 坐标（深度）大于该值的 Collider2D。
                minNormalAngle	*** 设置接触筛选器，使其筛选的结果仅包含碰撞法线角度大于该角度的接触。
                useDepth	将接触筛选器设置为使用 minDepth 和 maxDepth 按深度筛选结果。
                useLayerMask	*** 将接触筛选器设置为按层遮罩筛选结果。
                useNormalAngle	将接触筛选器设置为使用 minNormalAngle 和 maxNormalAngle 按碰撞的法线角度筛选结果。
                useOutsideDepth	将接触筛选器设置为在 minDepth 和 maxDepth 范围内或在此范围外进行筛选。
                useOutsideNormalAngle	将接触筛选器设置为在 minNormalAngle 和 maxNormalAngle 范围内或在此范围外进行筛选。
                useTriggers	进行设置，以便根据触发碰撞体的涉及情况筛选接触结果。
         * 
         */
        private ContactFilter2D _TerrainFilter;

        /// <summary>A <see cref="ContactFilter2D"/> using the layer mask of the layers that this object collides with.</summary>
        public ContactFilter2D TerrainFilter => _TerrainFilter;

        /************************************************************************************************************************/

        /// <summary>
        /// The largest angle that a contact point can have between the local up and its normal to be considered a
        /// <see cref="GroundContact"/> and set <see cref="IsGrounded"/> to true.
        /// </summary>
        public virtual float GripAngle
        {
            get => 0;
            set => throw new NotSupportedException($"Can't set {GetType().FullName}.{nameof(GripAngle)}.");
        }

        //坡度
        /// <summary>The maximum height that this body can snap up or down a step to remain on the ground.</summary>
        public virtual float StepHeight
        {
            get => 0;
            set => throw new NotSupportedException($"Can't set {GetType().FullName}.{nameof(StepHeight)}.");
        }

        /// <summary>Details of the current contact point with the ground.</summary>
        public virtual PlatformContact2D GroundContact => default;

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="TerrainFilter"/>.</summary>
        protected virtual void Awake()
        {
            _TerrainFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        }

        /************************************************************************************************************************/
        // protected  void OnEnable()
        // {
        //     Debug.Log($"{Time.frameCount} {GetType().Name} OnEnable=============");
        //     int a = 10;
        // }
        /// <summary>Sets <see cref="IsGrounded"/> to false.</summary>
        protected virtual void OnDisable()
        {
            // Debug.Log($"{Time.frameCount} {GetType().Name} OnDisable=============");
            IsGrounded = false;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Displays some non-serialized details at the bottom of the Inspector.</summary>
        /// <example>
        /// <see cref="https://kybernetik.com.au/inspector-gadgets/pro">Inspector Gadgets Pro</see> would allow this to
        /// be implemented much easier by simply placing
        /// <see cref="https://kybernetik.com.au/inspector-gadgets/docs/script-inspector/inspectable-attributes">
        /// Inspectable Attributes</see> on the properties we want to display like so:
        /// <para></para><code>
        /// [Inspectable]
        /// public bool IsGrounded ...
        /// </code>
        /// </example>
        [UnityEditor.CustomEditor(typeof(CharacterBody2D), true)]
        public class Editor : UnityEditor.Editor
        {
            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (!UnityEditor.EditorApplication.isPlaying)
                    return;

                using (new UnityEditor.EditorGUI.DisabledScope(true))
                {
                    var target = (CharacterBody2D)this.target;
                    UnityEditor.EditorGUILayout.Toggle("Is Grounded", target.IsGrounded);
                }
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
