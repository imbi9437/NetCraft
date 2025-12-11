using UnityEngine;

namespace _Project.Script.StateMachine.Mono
{
    /// <summary>
    /// MonoBehaviour 기반 상태 최상위 객체 <br/>
    /// index의 경우에는 Property Setter로 설정 <code> public override int index => 1; </code> <br/>
    /// 혹은 상태 머신에서 선언한 enum을 캐스팅하여 사용 <code> public override int index => (enumType)enumValue</code> <br/>
    /// MonoBehaviour의 메세지 함수 활용해 상태에 대한 기능 구현 <br/>
    /// OnEnable : 해당 상태 시작 <br/>
    /// Update : 해당 상태 지속되는 경우 <br/>
    /// OnDisable : 해당 상태 종료
    /// </summary>
    public abstract class MonoState : MonoBehaviour
    {
        public abstract int index { get; }
        protected MonoStateMachine machine;

        public virtual void Initialize(MonoStateMachine machine)
        {
            this.machine = machine;
        }

        public virtual void OnEnable() { }
        public virtual void Update() { }
        public virtual void OnDisable() { }
    }
}
