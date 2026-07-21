namespace UwU.HierarchicalStateMachine
{
    public abstract class State
    {
        private StateMachine machine;

        internal void SetMachine(StateMachine machine)
        {
            this.machine = machine;
        }

        protected StateMachine Machine => this.machine;

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Update(float dt) { }

        public virtual void FixedUpdate(float dt) { }

        protected void Finish(StateResult result = StateResult.Success)
        {
            this.machine.NotifyStateFinished(this, result);
        }
    }
}