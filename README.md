# DFU-Mod_Believable-Equipment-Characteristics-And-Durability
First Mod I Made for the Daggerfall Unity Project.
Name: Believable Equipment Characteristics And Durability
Author: Kirk.O
Released On: Version 1.00, Released on 3/16/2020, 5:30 PM
Version: 1.10

DESCRIPTION
Changes how equipment durability is damaged, and how much damage equipment takes based on what material and weapon type is being used to deal said damage.

UPDATES:

Version 1.10, 3/26/2020: Fixed some bugs, redid parts of the code for easier modification when needed. Also made it so bows and short-blade weapons last much longer than before.


WHAT DOES THIS MOD CHANGE AND WHY?:
-Overall, increases how much condition damage equipment takes from both dealing damage and taking damage. With the purpose being that more mind will have 
to be kept on maintaining ones equipment, instead of being able to cut down 1000s of foes before your elven longsword needs to be sharpened, or that you 
leather shield can take 3000 blows from a giant before it a useless pile of nothing. 

-This equipment damage will also be increased or decreased depending on what is being used to deal said damage, as well as the attributes of the attacker. 
This can be seen in that all attacks will have the attackers strength attribute add onto this condition damage.

-Unarmed attacks will deal condition damage to equipment now, unlike in the base game where only attacks with a weapon would do damage to the equipment that 
was hit. Unarmed attacks make up most of the attacks the player will ever take, so this adds quite a bit more overall wear to armor.

-Blunt type weapons (Staves, Maces, Flails, and Warhammers) will deal their increased condition damage based on the weapons current weight value. So that 
super light ebony staff will deal nearly no damage to the equipment of a target, but that Steel flail will deal much more condition damage compared to the 
staff. On the topic of blunt weapons, the well padded leather armor will take much less damage from blunt force, and will deal more damage to shields of 
all types, as well as dealing more damage to chain armor.

-All other weapon types (essentially all bladed) will deal increased condition damage based on what material the target is wearing as compared to the weapon 
the attacker is using. Simply put, that ebony katana is going to rip those leather pants into useless scraps in a few blows. That Dwarven War-axe will be 
able to cut large gashes into a Dwarven Cuirass, but will mostly stay structurally sound. That Iron Dagger will barely put a scuff on that Orcish helmet. This 
difference in damage is reduced or increased depending on how far each material is away from each other on the "Tier list" of materials. A Daedric Long-bow 
will deal additional damage to all materials, besides itself, but will deal much more to iron armor compared to Orcish or Ebony, etc.

-Additionally with all weapons that are not blunt, they will deal less damage to shields and less to chain armors.

-The effective "maximum condition" of all materials have been equalized in a sense, in an attempt to balance how long one would expect their equipment to 
last before it needed to be repaired/maintained. In the classic game, a leather cuirass has 4096 maximum condition damage, whereas this value is multiplied 
based on the material modifier of the item. So a daedric cuirass has 32768 max cond. damage, 8 times more, in practice this is way too much, especially when 
in practice the player is being hit less as well with that better armor, so that daedric cuirass will NEVER practically need to be repaired. That is why I 
have attempted to equalize them, there is still an advantage in terms of longevity for wearing better materials of course, that being the higher the tier of 
said piece of equipment, the final condition damage value will be reduced based on this. So yes, that daedric cuirass will last much longer than the leather 
one, but not to the insane extent that classic has it.

-The player will have a simple pop-up message appear when pieces of their equipment get below a certain percentage threshold. The message is also dependent on 
the piece of equipment that is in disrepair. Don't expect this to work perfectly, it's currently just in a state of "You should usually get warned, and sometimes 
you will be warned a few times more than you should be." This is mostly from lack of finding a solution on how to make it more elegant on my part, so hopefully 
I can make this warning system better in the future.

-When I say equipment, I don't just mean protective armor either, expect to need to sharpen your blades and restring your bows more often than before 
(that being pretty much never). I hope to be able to better balance these values out in the future, but from some testing they seem fair, and not overly intrusive 
to general game-play, but it makes you have to possibly bring spare equipment with you, or possibly save some gold to maintain your work-gear instead of instantly 
picking up that fancy pair of mithril gauntlets.


OPTIONS:

	Possible work in progress, if there is demand for more easily values and options.

VERSION HISTORY:

1.00 - Initial Release

1.10 - Bug Fixes, Minor Code Restructuring, and Longer Lasting Bows and Short-blades


INSTALLATION:
Unzip and open the folder that matches your operating system (Windows/OSX/Linux)
Copy the "Believable Equipment Characteristics And Durability.dfmod" into your DaggerfallUnity_Data\StreamingAssets\Mods folder
Make sure the mod is enabled and "Mod system" is enabled in the starting menu under "Advanced -> Enhancements"

COMPATIBILITY:
There should be no issues between this mod and other mods. The main issues would arise if another mod also alters the "DamageEquipment" method in the 
"FormulaHelper.cs" script. As of now, mods such as "RoleplayRealism" should be compatible, as a separate method is overridden. The best bet would be 
to have this lower in the load-order if you happen to be experiencing some issues. I.E. Make my mod load 5th, and the possibly likely incompatible 
mod load as 4th, etc.

UNINSTALL:
Remove "Believable Equipment Characteristics And Durability.dfmod" from the "StreamingAssets/Mods" folder.
