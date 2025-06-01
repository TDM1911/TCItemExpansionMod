using Asuna.CharManagement;
using Asuna.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ItemExpansionMod
{
    public class CustomApparel : Apparel
    {
        public bool IsLocked;
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

    }
}