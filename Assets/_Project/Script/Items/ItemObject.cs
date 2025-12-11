using _Project.Script.Interface;
using _Project.Script.Manager;
using _Project.Script.UI.GlobalUI;
using EPOOutline;
using UnityEngine;

namespace _Project.Script.Items
{
    [RequireComponent(typeof(Outlinable))]
    public class ItemObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private Outlinable outlinable;

        private GameObject _visualObject;
        private ItemInstance _itemInstance;

		private void Update()
		{
            //아이템 회수테스트용
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Interact(null);
            }
		}

		public void Initialize(ItemInstance instance)
        {
            _itemInstance = instance;
            //TODO : 위치는 플레이어 위치를 따라가야할텐데 transform말고 플레이어 위치를 받아올수 있으면 좋을듯
            _visualObject = Instantiate(_itemInstance.itemData.visualPrefab, transform);
            _visualObject.transform.localPosition = Vector3.zero;
            _visualObject.transform.localRotation = Quaternion.identity;
            
            //이건 어떻게 써야할지 모르겠음...
            //outlinable.AddAllChildRenderersToRenderingList();
        }

        public void HoveredEnter(IInteractor interactor)
        {
            outlinable.enabled = true;
            GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ItemTooltip, _itemInstance);
        }
        public void Interact(IInteractor interactor)
        {
            bool result = DataManager.Instance.TryAddItem(_itemInstance, out bool isDestroyObject);
            
            if (result)
            {
                if (isDestroyObject) Destroy(gameObject);
            }
            else
            {
                Debug.Log("아이템 획득 실패");
            }
        }
        public void HoveredExit(IInteractor interactor)
        {
            outlinable.enabled = false;
            GlobalUIManager.Instance.HidePanel(GlobalPanelType.ItemTooltip);
        }

        private void Reset()
        {
            outlinable = GetComponent<Outlinable>();
        }
    }
}
