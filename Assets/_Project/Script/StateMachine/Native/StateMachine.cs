namespace _Project.Script.StateMachine.Native
{
    /// <summary>
    /// 기본적인 상태 패턴 머신
    /// </summary>
    /// <typeparam name="T">해당 머신을 사용할 객체</typeparam>
    public class StateMachine<T> where T : class
    {
        private T _owner;

        private IState<T> _prevState;
        private IState<T> _curState;

        public StateMachine(T owner) => _owner = owner;

        public void Update() => _curState?.Update(_owner);

        public void ChangeState(IState<T> state)
        {
            _curState?.Exit(_owner);
            _prevState = _curState;
            _curState = state;
            _curState.Enter(_owner);
        }

        public void RollBackState() => ChangeState(_prevState);
    }
}
