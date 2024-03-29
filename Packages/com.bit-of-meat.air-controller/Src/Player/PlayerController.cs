using UnityEngine;
using FSM;

using Player.States;
using Player.States.Ground;

namespace Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private float _wallrunJumpForce;
        public float WallrunJumpForce { get => _wallrunJumpForce; }
        //Base
        public PlayerInput Input { get => _input; }
        public Rigidbody RigidBody { get => _rigidbody; }
        //Movement
        public float DesiredMoveSpeed { get; set; }
        public float WalkSpeed { get => _walkSpeed; }
        public float SprintSpeed { get => _sprintSpeed; }
        public float SlideSpeed { get => _slideSpeed; }
        // Ground
        public bool IsGrounded { get => _isGrounded; }
        public LayerMask GroundLayerMask { get => _groundLayerMask; }
        public float GroundDrag { get => _groundDrag; }
        public float SlideDrag { get => _slideDrag; }
        public bool IsAbove { get => _isAbove; }
        public float PlayerHeight { get => _playerHeight; }
        public bool IsLeftWall { get => _isLeftWall; }
        public bool IsRightWall { get => _isRightWall; }
        // Jumping
        public float JumpHeight { get => _jumpHeight; }
        public float JumpCooldown { get => _jumpCooldown; }
        public bool ReadyToJump { get; set; }
        // Crouching
        public bool Crouching { get; set; }
        public float CrouchSpeed { get => _crouchSpeed; }
        public float StartYScale { get => _startYScale; }
        public float CrouchYScale { get => _crouchYScale; }
        public bool ExitingSlope { get; set; }
        // Wallrun
        public float WallrunDrag { get => _wallrunDrag; }
        public float WallrunSpeed { get => _wallrunSpeed; }

        public float AirMultiplier { get => _airMultiplier; }

        public Transform Orientation { get => _orientation; }

        public float Speed { get => new Vector3(RigidBody.velocity.x, 0f, RigidBody.velocity.z).magnitude; }
        
        public RaycastHit _leftHit, _rightHit;

        public Animator _animator;





        private StateMachine<PlayerStates> _stateMachine;

        private bool _isGrounded;
        private bool _isAbove;
        private bool _isLeftWall;
        private bool _isRightWall;

        // Movement
        public float MoveSpeed { get; set; }
        public float LastDesiredMoveSpeed { get; set; }
        private Vector3 _moveDirection;
        // Crouching
        private float _startYScale;

        [SerializeField] private PlayerInput _input;
        [SerializeField] private Rigidbody _rigidbody;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _slideSpeed = 7f;
        [SerializeField] private float _slideDrag = 0f;
        [SerializeField] private float _groundDrag = 8f;

        [Header("Jumping")]
        [SerializeField] private float _jumpHeight = 3f;
        [SerializeField] private float _jumpCooldown = 0.25f;
        [SerializeField] private float _airMultiplier = 0.4f;

        [Header("Crouching")]
        [SerializeField] private float _crouchSpeed = 3.5f;
        [SerializeField] private float _crouchYScale = 0.5f;
        
        [Header("Ground Check")]
        [SerializeField] private float _playerHeight = 2f;
        [SerializeField] private LayerMask _groundLayerMask;

        [Header("Easy Wallrun v0.1")]
        [SerializeField] private float _wallrunDrag;
        [SerializeField] private float _wallrunSpeed;
        [SerializeField] private float _startWallrunForce;
        public float StartWallrunForce { get => _startWallrunForce; }
        [SerializeField] private float _wallrunTick;
        public float WallrunTick { get => _wallrunTick; }
        [SerializeField] private Transform _orientation;

        // private Coroutine smooth = null;
        // private float _test;

        /// <summary>
        /// Adds player states
        /// </summary>
        private void AddStates() {
            _stateMachine.AddState(PlayerStates.Ground, new PlayerGroundState(this));
            _stateMachine.AddState(PlayerStates.Jump, new PlayerJumpState(this));
            _stateMachine.AddState(PlayerStates.Fall, new PlayerFallState(this));
            _stateMachine.AddState(PlayerStates.Wallrun, new PlayerWallrunState(this));
            _stateMachine.AddState(PlayerStates.Walljump, new PlayerWalljumpState(this));
        }

        /// <summary>
        /// Adds player transitions
        /// </summary>
        private void AddTransitions() {
            _stateMachine.AddTransition(PlayerStates.Ground, PlayerStates.Fall, (_) => !IsGrounded);
            _stateMachine.AddTransition(PlayerStates.Ground, PlayerStates.Jump, (_) => Input.IsJump && ReadyToJump);

            _stateMachine.AddTransition(PlayerStates.Jump, PlayerStates.Fall, (_) => ReadyToJump);
            
            _stateMachine.AddTransition(PlayerStates.Fall, PlayerStates.Ground, (_) => IsGrounded);
            _stateMachine.AddTransition(PlayerStates.Fall, PlayerStates.Wallrun, (_) => Input.MovementDirection.y > 0 && Input.IsJump && (IsLeftWall || IsRightWall) && Speed >= WalkSpeed);

            _stateMachine.AddTransition(PlayerStates.Wallrun, PlayerStates.Walljump, (_) => !Input.IsJump && ReadyToJump);
            _stateMachine.AddTransition(PlayerStates.Walljump, PlayerStates.Fall, (_) => ReadyToJump);
            _stateMachine.AddTransition(PlayerStates.Wallrun, PlayerStates.Ground, (_) => IsGrounded);
        }

        /// <summary>
        /// Initialize all variables
        /// </summary>
        private void Init() {
            RigidBody.freezeRotation = true;
            ReadyToJump = true;
            _startYScale = transform.localScale.y;

            _stateMachine = new StateMachine<PlayerStates>();
        }

        private void Start() {
            Init();
            AddStates();
            AddTransitions();

            _stateMachine.SetStartState(PlayerStates.Ground);
            _stateMachine.Init();
        }

        private void Update() {
            _isGrounded = Physics.BoxCast(transform.position, new Vector3(0.5f, 0.05f, 0.5f), Vector3.down, Quaternion.identity, PlayerHeight * 0.5f, GroundLayerMask);
            _isAbove = Physics.Raycast(transform.position, Vector3.up, PlayerHeight * 0.5f + 0.05f, GroundLayerMask);

            _isLeftWall = Physics.Raycast(transform.position, -_orientation.right, out _leftHit, PlayerHeight * 0.5f + 0.05f, GroundLayerMask);
            _isRightWall = Physics.Raycast(transform.position, _orientation.right, out _rightHit, PlayerHeight * 0.5f + 0.05f, GroundLayerMask);
        }

        /// <summary>
        /// Calculate all physics here
        /// </summary>
        private void FixedUpdate() {
            _stateMachine.OnLogic();
            SpeedLimiter();
        }

        /// <summary>
        /// Globally limits the speed of the player  
        /// </summary>
        private void SpeedLimiter() {
            Vector3 flatVel = new Vector3(RigidBody.velocity.x, 0f, RigidBody.velocity.z);

            if (flatVel.magnitude > DesiredMoveSpeed) {
                Vector3 limitedVel = flatVel.normalized * DesiredMoveSpeed;
                RigidBody.velocity = new Vector3(limitedVel.x, RigidBody.velocity.y, limitedVel.z);
            }
        }

        // void OnDrawGizmos() {
        //     Gizmos.color = Color.black;
        //     Gizmos.DrawCube(transform.position + Vector3.down * (PlayerHeight * 0.5f), new Vector3(0.5f, 0.05f, 0.5f));
        // }
    }
}