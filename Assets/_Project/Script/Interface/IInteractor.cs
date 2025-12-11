namespace _Project.Script.Interface
{
    public interface IInteractor
    {
        public IInteractable HoveredInteractable { get; set; }
        
        public void FindInteractable();
        public void Interaction();
    }
}
