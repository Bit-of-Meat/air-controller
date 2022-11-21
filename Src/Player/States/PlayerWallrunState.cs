using UnityEngine;
using FSM;

namespace Player.States {
    public class PlayerWallrunState : StateBase<PlayerStates> {
        private PlayerController _controller;
        private float _upforce;
        private Vector3 _forward;
        
        public PlayerWallrunState(PlayerController controller) : base(needsExitTime: false) {
            _controller = controller;
        }

        public override void OnEnter() {
            _controller.DesiredMoveSpeed = _controller.WallrunSpeed;
            _controller.RigidBody.drag = _controller.WallrunDrag;
            _controller.RigidBody.velocity = Vector3.zero;
            _upforce = _controller.StartWallrunForce;

            Vector3 _orientation = _controller.Orientation.forward;
            Vector3 _normal = _controller.IsLeftWall ? _controller._leftHit.normal : _controller._rightHit.normal;

            _forward = _orientation - Vector3.Dot(_orientation, _normal) * _normal;
            Debug.DrawLine(_controller.transform.position, _controller.transform.position + _forward, Color.red, 20f);
        }

        public override void OnLogic() {
            _controller.RigidBody.velocity = _forward * 2000f * Time.deltaTime;
            _controller.RigidBody.velocity = new Vector3(_controller.RigidBody.velocity.x, _upforce, _controller.RigidBody.velocity.z);
            _upforce -= _controller.WallrunTick * Time.deltaTime;
        }
    }
}