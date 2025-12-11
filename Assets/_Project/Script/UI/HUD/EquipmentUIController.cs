using System;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Script.UI.HUD
{
    public class EquipmentUIController : HUDController
    {
        private List<EquipmentSlot> _slots;
        
        private void Awake()
        {
            _slots = GetComponentsInChildren<EquipmentSlot>(true).OrderBy(s => s.transform.GetSiblingIndex()).ToList();

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Initialize(this,i);
            }
        }
    }
}
