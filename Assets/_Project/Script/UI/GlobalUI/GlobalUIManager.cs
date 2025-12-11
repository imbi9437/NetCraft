using System;
using System.Collections.Generic;
using _Project.Script.Generic;
using _Project.Script.Core;
using UnityEngine;

namespace _Project.Script.UI.GlobalUI
{
    public enum GlobalPanelType
    {
        ItemTooltip,
        DragItem,
        ConfirmPopup,
        TwoButtonPopup,
    }

    public class GlobalUIManager : MonoSingleton<GlobalUIManager>
    {
        private Dictionary<GlobalPanelType, GlobalPanel> panelDic;

        protected override void Awake()
        {
            base.Awake();

            panelDic = new Dictionary<GlobalPanelType, GlobalPanel>();

            var panels = GetComponentsInChildren<GlobalPanel>(true);

            foreach (var panel in panels)
            {
                panelDic.TryAdd(panel.UIType, panel);
                panel.Initialize();
                panel.gameObject.SetActive(false);
            }
        }

        public void ShowPanel(GlobalPanelType type, object param = null, Action<object> callback = null)
        {
            panelDic[type].Show(param, callback);
        }

        public void HidePanel(GlobalPanelType type)
        {
            panelDic[type].Hide();
        }

        public void HideAllPanel()
        {
            foreach (var panel in panelDic.Values)
            {
                panel.Hide();
            }
        }



		public bool TryGetPanel<T>(GlobalPanelType type, out T panel) where T : GlobalPanel
		{
			if (panelDic.TryGetValue(type, out GlobalPanel basePanel) && basePanel is T casted)
			{
				panel = casted;
				return true;
			}

			panel = null;
			return false;
		}
	}
}
