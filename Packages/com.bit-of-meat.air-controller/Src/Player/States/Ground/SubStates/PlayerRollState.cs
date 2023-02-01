using FSM;

namespace Player.States.Ground {
    public class PlayerRollState : StateBase<PlayerStates> {
        private PlayerController _controller;

        public PlayerRollState(PlayerController controller) : base(needsExitTime: false) {
            _controller = controller;
        }
    }
}