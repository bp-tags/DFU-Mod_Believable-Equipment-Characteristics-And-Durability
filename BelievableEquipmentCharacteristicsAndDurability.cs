// Project:         BelievableEquipmentCharacteristicsAndDurability mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    3/8/2020, 5:15 PM
// Last Edit:		3/16/2020, 10:00 AM
// Modifier:		

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using System;

namespace BelievableEquipmentCharacteristicsAndDurability
{
    public class BelievableEquipmentCharacteristicsAndDurability : MonoBehaviour
    {
        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("BelievableEquipmentCharacteristicsAndDurability");
            go.AddComponent<BelievableEquipmentCharacteristicsAndDurability>();
        }

        void Awake()
        {
            InitMod();

            mod.IsReady = true;
        }

        private static void InitMod()
        {
            Debug.Log("Begin mod init: BelievableEquipmentCharacteristicsAndDurability");
			
			FormulaHelper.RegisterOverride(mod, "DamageEquipment", (Func<DaggerfallEntity, DaggerfallEntity, int, DaggerfallUnityItem, int, bool>)DamageEquipment);			
			
			Debug.Log("Finished mod init: BelievableEquipmentCharacteristicsAndDurability");
		}
		
		#region Equipment Durability Damage Formulas
		
        /// Allocate any equipment damage from a strike, and reduce item condition.
        private static bool DamageEquipment(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart)
        {
			int atkStrength = attacker.Stats.LiveStrength * 100;
			int tarMatMod = 0;
			int matDifference = 0;
			bool bluntWep = false;
			int wepWeight = 1;
			
            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            if (weapon != null && damage > 0)
			{				
				if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
				{
					bluntWep = true;
					wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());
				}
				
				int wepEqualize = EqualizeMaterialConditions(weapon, damage);
				int atkMatMod = weapon.GetWeaponMaterialModifier() + 2;
				int wepDam = wepEqualize + (atkStrength / 20);
				
				ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam); // Does condition damage to the attackers weapon.
				
				if (attacker == GameManager.Instance.PlayerEntity)
					WarningMessagePlayerEquipmentCondition(weapon);
				
				DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
				bool shieldTakesDamage = false;
				if (shield != null)
				{
					BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

					for (int i = 0; (i < protectedBodyParts.Length) && !shieldTakesDamage; i++)
					{
						if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
							shieldTakesDamage = true;
					}
				}

				if (shieldTakesDamage)
				{
					damage = EqualizeMaterialConditions(shield, damage);
					tarMatMod = ArmorMaterialModifierFinder(shield);
					matDifference = tarMatMod - atkMatMod;
					damage = MaterialDifferenceDamageCalculation(shield, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldTakesDamage);
					
					ApplyConditionDamageThroughWeaponDamage(shield, target, damage);
					
					if (target == GameManager.Instance.PlayerEntity)
						WarningMessagePlayerEquipmentCondition(shield);
				}
				else
				{
					EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
					DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
					if (armor != null)
					{
						damage = EqualizeMaterialConditions(armor, damage);
						tarMatMod = ArmorMaterialModifierFinder(armor);
						matDifference = tarMatMod - atkMatMod;
						damage = MaterialDifferenceDamageCalculation(armor, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldTakesDamage);
						
						ApplyConditionDamageThroughWeaponDamage(armor, target, damage);
						
						if (target == GameManager.Instance.PlayerEntity)
							WarningMessagePlayerEquipmentCondition(armor);
					}
				}
				return false;
			}
			else if (weapon == null && damage > 0) // Handles Unarmed attacks.
			{	
				DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
				bool shieldTakesDamage = false;
				if (shield != null)
				{
					BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

					for (int i = 0; (i < protectedBodyParts.Length) && !shieldTakesDamage; i++)
					{
						if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
							shieldTakesDamage = true;
					}
				}

				if (shieldTakesDamage)
				{
					damage = EqualizeMaterialConditions(shield, damage);
					tarMatMod = ArmorMaterialModifierFinder(shield);
					atkStrength /= 5;
					damage = (damage / tarMatMod) + atkStrength;
					
					ApplyConditionDamageThroughUnarmedDamage(shield, target, damage);
					
					if (target == GameManager.Instance.PlayerEntity)
						WarningMessagePlayerEquipmentCondition(shield);
				}
				else
				{
					EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
					DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
					if (armor != null)
					{
						damage = EqualizeMaterialConditions(armor, damage);
						tarMatMod = ArmorMaterialModifierFinder(armor);
						atkStrength /= 5;
						damage = (damage / tarMatMod) + atkStrength;
						
						ApplyConditionDamageThroughUnarmedDamage(armor, target, damage);
						
						if (target == GameManager.Instance.PlayerEntity)				
							WarningMessagePlayerEquipmentCondition(armor);
					}
				}
				return false;
			}
			return false;
        }
		
		/// Does most of the calculations determining how much a material/piece of equipment should be taking damage from something hitting it.
		private static int MaterialDifferenceDamageCalculation(DaggerfallUnityItem item, int matDifference, int atkStrength, int damage, bool bluntWep, int wepWeight, bool shieldCheck)
		{
			int itemMat = item.NativeMaterialValue;

			if (bluntWep) // Personally, I think the higher tier materials should have higher weight modifiers than most of the lower tier stuff, that's another idea for another mod though.
			{
				if (shieldCheck)
					damage *= 2;
				
				if (itemMat == (int)ArmorMaterialTypes.Leather)
					damage /= 2;
				else if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
					damage *= 2;
				
				if (wepWeight >= 7 && wepWeight <= 9) // Later on, possibly add some settings into this specific mod, just for more easy modification for users, as well as practice for myself.
				{
					atkStrength /= 5; // 1-3 Kg Staves, 3-6 Kg Maces, 4-9 Kg Flails, 4-9 Kg Warhammers (I think Warhammers are bugged here, should be higher, not equal to flails)
					damage = (damage * 3) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 4 && wepWeight <= 6) // The Extra Weight negative enchantment just multiplies the current weight by 4, feather weight sets everything to 0.25 Kg.
				{
					atkStrength /= 5;
					damage = (damage * 2) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 10 && wepWeight <= 12) // This will matter if the Warhammer weight "bug" is ever fixed.
				{
					atkStrength /= 5;
					damage = (damage * 4) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 1 && wepWeight <= 3) // Put these weights lower down, since it's less likely, so very slight better performance with less unneeded checks.
				{
					atkStrength /= 5;
					damage += atkStrength;
					damage /= 4;
					return damage;
				}
				else if (wepWeight >= 13 && wepWeight <= 35) // 35 would be highest weight with a 8.75 base item and the extra weight enchant.
				{
					atkStrength /= 5;
					damage = (damage * 5) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 36) // 36 and higher weight would be a "bug fixed" steel or daedric warhammer with extra weight, 48 Kg.
				{
					atkStrength /= 5;
					damage = (damage * 7) + atkStrength;
					damage /= 3;
					return damage;
				}
				else // Basically any value that would be 0, which I don't think is even possible since this number is rounded up, so even feather weight would be 1 I believe.
				{
					atkStrength /= 20;
					damage = (damage / 2) + atkStrength;
					damage /= 5;
					return damage;
				}
			}
			else
			{
				if (shieldCheck)
					damage /= 2;
				
				if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
					damage /= 2;
				
				if (matDifference < 0)
				{
					matDifference *= -1;
					matDifference += 1;
					atkStrength /= 10;
					damage = (damage * matDifference) + atkStrength;
					damage /= 2;
					return damage;
				}
				else if (matDifference == 0)
				{
					atkStrength /= 10;
					damage += atkStrength;
					damage /= 2;
					return damage;
				}
				else
				{
					atkStrength /= 10;
					damage = (damage / matDifference) + atkStrength;
					damage /= 3;
					return damage;
				}
			}
			return damage;
		}
		
        /// Applies condition damage to an item based on physical hit damage.
        private static void ApplyConditionDamageThroughWeaponDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage) // Possibly add on so that magic damage also damages worn equipment.
        {
			//Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
			//Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);
			
			if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
				damage /= 100;
                int amount = item.IsShield ? damage * 2: damage * 4;
                item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
			else // Attacker gets their weapon damaged, if they are using one, otherwise this method is not called.
			{
				damage /= 100;
				int amount = (10 * damage) / 50;
				if ((amount == 0) && Dice100.SuccessRoll(40)) // Likely will want to increase this somehow without completely gimping lower durability weapons.
					amount = 1;

				item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
				if (owner == GameManager.Instance.PlayerEntity){
					Debug.LogFormat("Attacker Damaged {0} by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
			}
        }
		
		/// Applies condition damage to an item based on physical hit damage. Specifically for unarmed attacks.
        private static void ApplyConditionDamageThroughUnarmedDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage)
        {
			//Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
			//Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);
			
			if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
				damage /= 100;
                int amount = item.IsShield ? damage: damage * 2;
                item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
			}
		}
		
		#endregion
		
		#region Helper Methods
		
		// If the player has equipment that is below a certain percentage of condition, this will check if they should be warned with a pop-up message about said piece of equipment.
		private static void WarningMessagePlayerEquipmentCondition(DaggerfallUnityItem item)
		{
			string roughItemMessage = "";
			string damagedItemMessage = "";
			
			if (item.ConditionPercentage <= 49)
			{
				if (item.TemplateIndex == (int)Armor.Boots || item.TemplateIndex == (int)Armor.Gauntlets || item.TemplateIndex == (int)Armor.Greaves) // Armor With Plural Names Text
				{
					roughItemMessage = String.Format("My {0} Are In Rough Shape", item.shortName);
					damagedItemMessage = String.Format("My {0} Are Falling Apart", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 29 || item.GetWeaponSkillIDAsShort() == 28 || item.GetWeaponSkillIDAsShort() == 31) // Bladed Weapons Text
				{
					roughItemMessage = String.Format("My {0} Could Use A Sharpening", item.shortName);
					damagedItemMessage = String.Format("My {0} Looks As Dull As A Butter Knife", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 32) // Blunt Weapoons Text
				{
					roughItemMessage = String.Format("My {0} Is Full Of Dings And Dents", item.shortName);
					damagedItemMessage = String.Format("My {0} Looks About Ready To Crumble To Dust", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 33) // Archery Weapons Text
				{
					roughItemMessage = String.Format("The Bowstring On My {0} Is Losing Its Twang", item.shortName);
					damagedItemMessage = String.Format("The Bowstring On My {0} Looks Ready To Snap", item.shortName);
				}
				else // Text for any other Valid Items
				{
					roughItemMessage = String.Format("My {0} Is In Rough Shape", item.shortName);
					damagedItemMessage = String.Format("My {0} Is Falling Apart", item.shortName);
				}
				
				if (item.ConditionPercentage <= 49 && item.ConditionPercentage >= 45) // 49 & 45 // This will work for now, until I find a more elegant solution.
					DaggerfallUI.AddHUDText(roughItemMessage, 2.00f); // Possibly make a random between a few of these lines to mix it up or something.				
				else if (item.ConditionPercentage <= 16 && item.ConditionPercentage >= 12) // 16 & 12
					DaggerfallUI.AddHUDText(damagedItemMessage, 2.00f);
			}
		}
		
		// Multiplies damage amount based on the condition modifier of a material, the idea being that items will take around the same amount of damage as other items in that category. The thing that will increase the durability in the end is having the number reduced later on by the material modifier, if applicable.
		private static int EqualizeMaterialConditions (DaggerfallUnityItem item, int damage)
		{
			int itemMat = item.NativeMaterialValue;
			damage *= 100;
			
			if (itemMat <= 9 && itemMat >= 0) // Checks if the item material is for weapons, and leather armor.
			{
				if (itemMat == (int)ArmorMaterialTypes.Leather)
					return damage;
				else if (itemMat == (int)WeaponMaterialTypes.Iron)
					return damage;
				else if (itemMat == (int)WeaponMaterialTypes.Steel)
					return damage;
				else if (itemMat == (int)WeaponMaterialTypes.Silver)
					return damage;
				else if (itemMat == (int)WeaponMaterialTypes.Elven)
					return damage * 2;
				else if (itemMat == (int)WeaponMaterialTypes.Dwarven)
					return damage * 3;
				else if (itemMat == (int)WeaponMaterialTypes.Mithril)
					return damage * 4;
				else if (itemMat == (int)WeaponMaterialTypes.Adamantium)
					return damage * 5;
				else if (itemMat == (int)WeaponMaterialTypes.Ebony)
					return damage * 6;
				else if (itemMat == (int)WeaponMaterialTypes.Orcish)
					return damage * 7;
				else if (itemMat == (int)WeaponMaterialTypes.Daedric)
					return damage * 8;
				else
					return damage;
			}
			else if (itemMat <= 521 && itemMat >= 256) // Checks if the item material is for armors.
			{
				if (itemMat == (int)ArmorMaterialTypes.Chain)
					return damage;
				else if (itemMat == (int)ArmorMaterialTypes.Chain2)
					return damage;
				else if (itemMat == (int)ArmorMaterialTypes.Iron)
					return damage;
				else if (itemMat == (int)ArmorMaterialTypes.Steel)
					return damage;
				else if (itemMat == (int)ArmorMaterialTypes.Silver)
					return damage;
				else if (itemMat == (int)ArmorMaterialTypes.Elven)
					return damage * 2;
				else if (itemMat == (int)ArmorMaterialTypes.Dwarven)
					return damage * 3;
				else if (itemMat == (int)ArmorMaterialTypes.Mithril)
					return damage * 4;
				else if (itemMat == (int)ArmorMaterialTypes.Adamantium)
					return damage * 5;
				else if (itemMat == (int)ArmorMaterialTypes.Ebony)
					return damage * 6;
				else if (itemMat == (int)ArmorMaterialTypes.Orcish)
					return damage * 7;
				else if (itemMat == (int)ArmorMaterialTypes.Daedric)
					return damage * 8;
				else
					return damage;
			}
			else
				return damage;
		}
		
		// Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.
		private static int ArmorMaterialModifierFinder (DaggerfallUnityItem armor)
		{
			int itemMat = armor.NativeMaterialValue;
			
			if (itemMat == (int)ArmorMaterialTypes.Leather)
				return 1;
			else if (itemMat == (int)ArmorMaterialTypes.Chain)
				return 2;
			else if (itemMat == (int)ArmorMaterialTypes.Chain2)
				return 2;
			else if (itemMat == (int)ArmorMaterialTypes.Iron)
				return 2;
			else if (itemMat == (int)ArmorMaterialTypes.Steel)
				return 3;
			else if (itemMat == (int)ArmorMaterialTypes.Silver)
				return 3;
			else if (itemMat == (int)ArmorMaterialTypes.Elven)
				return 4;
			else if (itemMat == (int)ArmorMaterialTypes.Dwarven)
				return 5;
			else if (itemMat == (int)ArmorMaterialTypes.Mithril)
				return 5;
			else if (itemMat == (int)ArmorMaterialTypes.Adamantium)
				return 5;
			else if (itemMat == (int)ArmorMaterialTypes.Ebony)
				return 6;
			else if (itemMat == (int)ArmorMaterialTypes.Orcish)
				return 7;
			else if (itemMat == (int)ArmorMaterialTypes.Daedric)
				return 8;
			else
				return 1;
		}
		
		#endregion
    }
}