// Project:         BelievableEquipmentCharacteristicsAndDurability mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.10
// Created On: 	    3/8/2020, 5:15 PM
// Last Edit:		3/26/2020, 1:15 AM
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
			int atkStrength = attacker.Stats.LiveStrength;
			int tarMatMod = 0;
			int matDifference = 0;
			bool bluntWep = false;
			bool shtbladeWep = false;
			bool missileWep = false;
			int wepEqualize = 1;
			int wepWeight = 1;
			float wepDamResist = 1f;
			float armorDamResist = 1f;
			
            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            if (weapon != null && damage > 0)
			{
				int atkMatMod = weapon.GetWeaponMaterialModifier() + 2;
				int wepDam = damage;
				wepEqualize = EqualizeMaterialConditions(weapon);
				wepDam *= wepEqualize;
		
				if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
				{
					wepDam += (atkStrength / 10);
					wepDamResist = (wepEqualize*.20f) + 1;
					wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
					bluntWep = true;
					wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else if (weapon.GetWeaponSkillIDAsShort() == 28) // Checks if the weapon being used is in the Short Blade category, then sets a bool value to true.
				{
					if (weapon.TemplateIndex == (int)Weapons.Dagger || weapon.TemplateIndex == (int)Weapons.Tanto)
					{
						wepDam += (atkStrength / 30);
						wepDamResist = (wepEqualize*.90f) + 1;
						wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
						shtbladeWep = true;
					}
					else
					{
						wepDam += (atkStrength / 30);
						wepDamResist = (wepEqualize*.30f) + 1;
						wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
						shtbladeWep = true;
					}
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else if (weapon.GetWeaponSkillIDAsShort() == 33) // Checks if the weapon being used is in the Missile Weapon category, then sets a bool value to true.
				{
					missileWep = true;
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else // If all other weapons categories have not been found, it defaults to this, which currently includes long blades and axes.
				{
					wepDam += (atkStrength / 10);
					wepDamResist = (wepEqualize*.20f) + 1;
					wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}

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
					int shieldEqualize = EqualizeMaterialConditions(shield);
					damage *= shieldEqualize;
					tarMatMod = ArmorMaterialModifierFinder(shield);
					matDifference = tarMatMod - atkMatMod;
					damage = MaterialDifferenceDamageCalculation(shield, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldTakesDamage);
					
					ApplyConditionDamageThroughWeaponDamage(shield, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);
					
					if (target == GameManager.Instance.PlayerEntity)
						WarningMessagePlayerEquipmentCondition(shield);
				}
				else
				{
					EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
					DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
					if (armor != null)
					{
						int armorEqualize = EqualizeMaterialConditions(armor);
						damage *= armorEqualize;
						tarMatMod = ArmorMaterialModifierFinder(armor);
						matDifference = tarMatMod - atkMatMod;
						damage = MaterialDifferenceDamageCalculation(armor, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldTakesDamage);
						
						ApplyConditionDamageThroughWeaponDamage(armor, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);
						
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
					int shieldEqualize = EqualizeMaterialConditions(shield);
					damage *= shieldEqualize;
					tarMatMod = ArmorMaterialModifierFinder(shield);
					atkStrength /= 5;
					armorDamResist = (tarMatMod*.35f) + 1;
					damage = (int)Mathf.Ceil((damage + atkStrength)/armorDamResist);
					
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
						int armorEqualize = EqualizeMaterialConditions(armor);
						damage *= armorEqualize;
						tarMatMod = ArmorMaterialModifierFinder(armor);
						atkStrength /= 5;
						armorDamResist = (tarMatMod*.20f) + 1;
						damage = (int)Mathf.Ceil((damage + atkStrength)/armorDamResist);
						
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
		}
		
        /// Applies condition damage to an item based on physical hit damage.
        private static void ApplyConditionDamageThroughWeaponDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage, bool bluntWep, bool shtbladeWep, bool missileWep, int wepEqualize) // Possibly add on so that magic damage also damages worn equipment.
        {
			//Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
			//Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);
			
			if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
                int amount = item.IsShield ? damage * 2: damage * 4;
                item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
			else // Attacker gets their weapon damaged, if they are using one, otherwise this method is not called.
			{
				int amount = (10 * damage) / 50;
				if ((amount == 0) && Dice100.SuccessRoll(40))
					amount = 1;
					
				if (missileWep)
					amount = SpecificWeaponConditionDamage(item, amount, wepEqualize);

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
				
				if (item.ConditionPercentage <= 49 && item.ConditionPercentage >= 47) // This will work for now, until I find a more elegant solution.
					DaggerfallUI.AddHUDText(roughItemMessage, 2.00f); // Possibly make a random between a few of these lines to mix it up or something.				
				else if (item.ConditionPercentage <= 16 && item.ConditionPercentage >= 14)
					DaggerfallUI.AddHUDText(damagedItemMessage, 2.00f);
			}
		}
		
		// Retrieves the multiplier based on the condition modifier of a material, the idea being that items will take around the same amount of damage as other items in that category. The thing that will increase the durability in the end is having the number reduced later on by the material modifier, if applicable.
		private static int EqualizeMaterialConditions (DaggerfallUnityItem item)
		{
			int itemMat = item.NativeMaterialValue;
			
			if (itemMat <= 9 && itemMat >= 0) // Checks if the item material is for weapons, and leather armor.
			{
				if (itemMat == (int)ArmorMaterialTypes.Leather)
					return 1;
				else if (itemMat == (int)WeaponMaterialTypes.Iron)
					return 1;
				else if (itemMat == (int)WeaponMaterialTypes.Steel)
					return 1;
				else if (itemMat == (int)WeaponMaterialTypes.Silver)
					return 1;
				else if (itemMat == (int)WeaponMaterialTypes.Elven)
					return 2;
				else if (itemMat == (int)WeaponMaterialTypes.Dwarven)
					return 3;
				else if (itemMat == (int)WeaponMaterialTypes.Mithril)
					return 4;
				else if (itemMat == (int)WeaponMaterialTypes.Adamantium)
					return 5;
				else if (itemMat == (int)WeaponMaterialTypes.Ebony)
					return 6;
				else if (itemMat == (int)WeaponMaterialTypes.Orcish)
					return 7;
				else if (itemMat == (int)WeaponMaterialTypes.Daedric)
					return 8;
				else
					return 1;
			}
			else if (itemMat <= 521 && itemMat >= 256) // Checks if the item material is for armors.
			{
				if (itemMat == (int)ArmorMaterialTypes.Chain)
					return 1;
				else if (itemMat == (int)ArmorMaterialTypes.Chain2)
					return 1;
				else if (itemMat == (int)ArmorMaterialTypes.Iron)
					return 1;
				else if (itemMat == (int)ArmorMaterialTypes.Steel)
					return 1;
				else if (itemMat == (int)ArmorMaterialTypes.Silver)
					return 1;
				else if (itemMat == (int)ArmorMaterialTypes.Elven)
					return 2;
				else if (itemMat == (int)ArmorMaterialTypes.Dwarven)
					return 3;
				else if (itemMat == (int)ArmorMaterialTypes.Mithril)
					return 4;
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
			else
				return 1;
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
		
		//For dealing with special cases of specific weapons in terms of condition damage amount.
		private static int SpecificWeaponConditionDamage(DaggerfallUnityItem weapon, int damageWep, int materialValue)
		{
			if (weapon.TemplateIndex == (int)Weapons.Long_Bow)
			{
				if (materialValue == 1) // iron, steel, silver
					damageWep = 1;
				else if (materialValue == 2) // elven
					damageWep = 2;
				else // dwarven, mithril, adamantium, ebony, orcish, daedric 
					damageWep = 3; 
			}
			else if (weapon.TemplateIndex == (int)Weapons.Short_Bow)
			{
				if (materialValue == 1) // iron, steel, silver
					damageWep = 1;
				else // elven, dwarven, mithril, adamantium, ebony, orcish, daedric
					damageWep = 2;
			}
			return damageWep;
		}
		
		#endregion
    }
}