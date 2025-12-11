namespace _Project.Script.Interface
{
    public interface IInteractable
    {
        public void HoveredEnter(IInteractor interactor);
        public void Interact(IInteractor interactor);
        public void HoveredExit(IInteractor interactor);
    }
}
