using UnityEngine;
using FSM;

namespace Player.States.Ground {
    public class PlayerCrouchState : StateBase<PlayerStates> {
        private PlayerController _controller;

        public PlayerCrouchState(PlayerController controller) : base(needsExitTime: false) {
            _controller = controller;
        }

        public override void OnEnter() {
            //_controller.transform.localScale = new Vector3(_controller.transform.localScale.x, _controller.CrouchYScale, _controller.transform.localScale.z);
            _controller.RigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            _controller.Crouching = true;

            _controller.DesiredMoveSpeed = _controller.CrouchSpeed;
            
            _controller._animator.SetBool("Crouch", true);
        }

        public override void OnExit() {
            //_controller.transform.localScale = new Vector3(_controller.transform.localScale.x, _controller.StartYScale, _controller.transform.localScale.z);

            _controller.Crouching = false;
            _controller._animator.SetBool("Crouch", false);
        }
    }
}