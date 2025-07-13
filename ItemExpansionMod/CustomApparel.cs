using ANToolkit.ResourceManagement;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Dialogues;
using Asuna.Items;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ItemExpansionMod
{
    public class CustomApparel : Apparel
    {
        public bool IsLocked;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;

        public override void Equipped(Character User)
        {
            if (IsLocked)
            {
                Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                dialogue.Lines.Add(new DialogueLine()
                {
                    LineID = "body_mod_alert",
                    Text = "I can't equip this without A's help!",
                    Speaker = "Jenna",
                    BrowExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    EyeExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    MouthExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    Character = Character.Player,
                    TextColor = Color.white,
                });
                DialogueManager.StartDialogue(dialogue);
                UseText = "Wear";
                return;
            }
            base.Equipped(User);
        }

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
                Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                dialogue.Lines.Add(new DialogueLine()
                {
                    LineID = "body_mod_alert",
                    Text = "I can't remove this without A's help!",
                    Speaker = "Jenna",
                    BrowExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    EyeExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    MouthExpression = Character.Player.GetPresetExpressionID(PresetExpression.Shocked),
                    Character = Character.Player,
                    TextColor = Color.white,
                });
                DialogueManager.StartDialogue(dialogue);
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