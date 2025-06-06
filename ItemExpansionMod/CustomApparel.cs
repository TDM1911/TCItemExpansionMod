using ANToolkit.ResourceManagement;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Items;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ItemExpansionMod
{
    public class CustomApparel : Apparel
    {
        public bool IsLocked;
        public List<StatModifierInfo> StatModifierInfos;

        public override void UnEquipped(Character User)
        {
            User = User.Get();
            User.EquippedItems.OnSlotChanged.RemoveListener(OnSlotChanged);
            base.UnEquipped(User);
            foreach (Item item in User.EquippedItems.GetAll())
            {
                Apparel apparel = ((item is Apparel) ? ((Apparel)item) : null);
                if (!apparel)
                {
                    continue;
                }

                foreach (EquipmentSlot slot in Slots)
                {
                    if (apparel.Requires.Contains(slot))
                    {
                        User.UnequipItem(apparel);
                        break;
                    }
                }
            }

            UseText = "Wear";
            if (IsLocked)
            {
                User.EquipItem(this);
                UseText = "Remove";
            }
        }
        public static CustomApparel CreateWithStatModifiers(string name)
        {
            Item item = Create(name);
            if (item is CustomApparel)
            {
                typeof(Equipment)
                   .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                   .SetValue(item, (item as CustomApparel).StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(item, (item as CustomApparel).StatModifierInfos);
                return (item as CustomApparel);
            }

            return null;
        }
    }
}