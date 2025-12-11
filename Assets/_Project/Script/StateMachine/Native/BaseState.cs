namespace _Project.Script.StateMachine.Native
{
    /// <summary>
    /// 상태의 최상위 객체
    /// </summary>
    /// <typeparam name="T">해당 상태를 사용할 객체</typeparam>
    public abstract class BaseState<T> : IState<T> where T : class
    {
        public abstract void Enter(T owner);
        public abstract void Update(T owner);
        public abstract void Exit(T owner);
    }
}
