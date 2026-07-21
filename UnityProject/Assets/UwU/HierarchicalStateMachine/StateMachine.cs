namespace UwU.HierarchicalStateMachine
{
    public abstract class StateMachine : State
    {
        private State current;

        public State Current => this.current;

        public void ChangeState(State next)
        {
            if (this.current == next)
                return;

            this.current?.Exit();

            this.current = next;

            this.current.SetMachine(this);

            this.current.Enter();
        }

        internal void NotifyStateFinished(State state, StateResult result)
        {
            if (state != this.current)
                return;

            OnStateFinished(state, result);
        }

        protected virtual void OnStateFinished(
            State state,
            StateResult result)
        {
        }

        public override void Update(float dt)
        {
            this.current?.Update(dt);
        }

        public override void FixedUpdate(float dt)
        {
            this.current?.FixedUpdate(dt);
        }

        public override void Exit()
        {
            this.current?.Exit();
        }
    }
}