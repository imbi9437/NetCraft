using System;
using UnityEngine;
using _Project.Script.Character.Player;

namespace _Project.Script.Core
{
    // StatType은 PlayerStatData.cs에서 정의됨

    public enum StatModifierType
    {
        Flat,
        Percent,
        Override,
    }


    [Serializable]
    public struct StatModifier
    {
        public float value;
        public StatType statType;
        public StatModifierType modifierType;
    }
}
