using _Project.Script.Interface;

namespace _Project.Script.EventStruct
{
    /// <summary>
    /// 플레이어 입력 활성화/비활성화 이벤트
    /// PlayerSpawner에서 PlayerInput으로 전달
    /// </summary>
    public struct OnPlayerInputEnabledEvent : IEvent
    {
        public bool enabled;

        public OnPlayerInputEnabledEvent(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}
