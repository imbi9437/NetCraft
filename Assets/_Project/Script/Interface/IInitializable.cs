namespace _Project.Script.Interface
{
    public interface IInitializable
    {
        public bool IsInitialized { get; }
        public void Initialize();
    }
}
