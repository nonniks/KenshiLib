using System.Collections.Generic;

#nullable enable
namespace KenshiLib.Core;

public class ModRecord
{
  private static readonly Dictionary<int, string> ChangeTypeCodes;
  private static readonly Dictionary<int, string> ModTypeCodes;

  public int InstanceCount { get; set; }

  public int TypeCode { get; set; }

  public int Id { get; set; }

  public string Name { get; set; } = "";

  public string StringId { get; set; } = "";

  public int ModDataType { get; set; }

  public Dictionary<string, bool> BoolFields { get; set; } = new Dictionary<string, bool>();

  public Dictionary<string, float> FloatFields { get; set; } = new Dictionary<string, float>();

  public Dictionary<string, int> LongFields { get; set; } = new Dictionary<string, int>();

  public Dictionary<string, float[]> Vec3Fields { get; set; } = new Dictionary<string, float[]>();

  public Dictionary<string, float[]> Vec4Fields { get; set; } = new Dictionary<string, float[]>();

  public Dictionary<string, string> StringFields { get; set; } = new Dictionary<string, string>();

  public Dictionary<string, string> FilenameFields { get; set; } = new Dictionary<string, string>();

  public Dictionary<string, Dictionary<string, int[]>>? ExtraDataFields { get; set; }

  public List<ModInstance>? InstanceFields { get; set; }

  public string getModType()
  {
    return CollectionExtensions.GetValueOrDefault<int, string>((IReadOnlyDictionary<int, string>) ModRecord.ModTypeCodes, this.TypeCode, "UNKNOWN:" + this.TypeCode.ToString());
  }

  public string getChangeType()
  {
    bool flag;
    return this.BoolFields.TryGetValue("REMOVED", out flag) & flag ? "REMOVED" : CollectionExtensions.GetValueOrDefault<int, string>((IReadOnlyDictionary<int, string>) ModRecord.ChangeTypeCodes, this.ModDataType, "UNKNOWN:" + this.ModDataType.ToString());
  }

  static ModRecord()
  {
    Dictionary<int, string> dictionary1 = new Dictionary<int, string>();
    dictionary1.Add(-2147483646 /*0x80000002*/, "NEW");
    dictionary1.Add(-2147483647 /*0x80000001*/, "CHANGED_A");
    dictionary1.Add(-2147483645 /*0x80000003*/, "CHANGED_B");
    ModRecord.ChangeTypeCodes = dictionary1;
    Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
    dictionary2.Add(0, "BUILDING");
    dictionary2.Add(1, "CHARACTER");
    dictionary2.Add(2, "WEAPON");
    dictionary2.Add(3, "ARMOUR");
    dictionary2.Add(4, "ITEM");
    dictionary2.Add(5, "ANIMAL_ANIMATION");
    dictionary2.Add(6, "ATTACHMENT");
    dictionary2.Add(7, "RACE");
    dictionary2.Add(9, "NATURE");
    dictionary2.Add(10, "FACTION");
    dictionary2.Add(13, "TOWN");
    dictionary2.Add(16 /*0x10*/, "LOCATIONAL_DAMAGE");
    dictionary2.Add(17, "COMBAT_TECHNIQUE");
    dictionary2.Add(18, "DIALOGUE");
    dictionary2.Add(19, "DIALOGUE_LINE");
    dictionary2.Add(21, "RESEARCH");
    dictionary2.Add(22, "AI_TASK");
    dictionary2.Add(24, "ANIMATION");
    dictionary2.Add(25, "STATS");
    dictionary2.Add(26, "PERSONALITY");
    dictionary2.Add(27, "CONSTANTS");
    dictionary2.Add(28, "BIOMES");
    dictionary2.Add(29, "BUILDING_PART");
    dictionary2.Add(30, "INSTANCE_COLLECTION");
    dictionary2.Add(31 /*0x1F*/, "DIALOG_ACTION");
    dictionary2.Add(34, "PLATOON");
    dictionary2.Add(36, "GAMESTATE_CHARACTER");
    dictionary2.Add(37, "GAMESTATE_FACTION");
    dictionary2.Add(38, "GAMESTATE_TOWN_INSTANCE_LIST");
    dictionary2.Add(41, "INVENTORY_STATE");
    dictionary2.Add(42, "INVENTORY_ITEM_STATE");
    dictionary2.Add(43, "REPEATABLE_BUILDING_PART_SLOT");
    dictionary2.Add(44, "MATERIAL_SPEC");
    dictionary2.Add(45, "MATERIAL_SPECS_COLLECTION");
    dictionary2.Add(46, "CONTAINER");
    dictionary2.Add(47, "MATERIAL_SPECS_CLOTHING");
    dictionary2.Add(49, "VENDOR_LIST");
    dictionary2.Add(50, "MATERIAL_SPECS_WEAPON");
    dictionary2.Add(51, "WEAPON_MANUFACTURER");
    dictionary2.Add(52, "SQUAD_TEMPLATE");
    dictionary2.Add(53, "ROAD");
    dictionary2.Add(55, "COLOR_DATA");
    dictionary2.Add(56, "CAMERA");
    dictionary2.Add(57, "MEDICAL_STATE");
    dictionary2.Add(59, "FOLIAGE_LAYER");
    dictionary2.Add(60, "FOLIAGE_MESH");
    dictionary2.Add(61, "GRASS");
    dictionary2.Add(62, "BUILDING_FUNCTIONALITY");
    dictionary2.Add(63 /*0x3F*/, "DAY_SCHEDULE");
    dictionary2.Add(64 /*0x40*/, "NEW_GAME_STARTOFF");
    dictionary2.Add(66, "CHARACTER_APPEARANCE");
    dictionary2.Add(67, "GAMESTATE_AI");
    dictionary2.Add(68, "WILDLIFE_BIRDS");
    dictionary2.Add(69, "MAP_FEATURES");
    dictionary2.Add(70, "DIPLOMATIC_ASSAULTS");
    dictionary2.Add(71, "SINGLE_DIPLOMATIC_ASSAULT");
    dictionary2.Add(72, "AI_PACKAGE");
    dictionary2.Add(73, "DIALOGUE_PACKAGE");
    dictionary2.Add(74, "GUN_DATA");
    dictionary2.Add(76, "ANIMAL_CHARACTER");
    dictionary2.Add(77, "UNIQUE_SQUAD_TEMPLATE");
    dictionary2.Add(78, "FACTION_TEMPLATE");
    dictionary2.Add(80 /*0x50*/, "WEATHER");
    dictionary2.Add(81, "SEASON");
    dictionary2.Add(82, "EFFECT");
    dictionary2.Add(83, "ITEM_PLACEMENT_GROUP");
    dictionary2.Add(84, "WORD_SWAPS");
    dictionary2.Add(86, "NEST_ITEM");
    dictionary2.Add(87, "CHARACTER_PHYSICS_ATTACHMENT");
    dictionary2.Add(88, "LIGHT");
    dictionary2.Add(89, "HEAD");
    dictionary2.Add(92, "FOLIAGE_BUILDING");
    dictionary2.Add(93, "FACTION_CAMPAIGN");
    dictionary2.Add(94, "GAMESTATE_TOWN");
    dictionary2.Add(95, "BIOME_GROUP");
    dictionary2.Add(96 /*0x60*/, "EFFECT_FOG_VOLUME");
    dictionary2.Add(97, "FARM_DATA");
    dictionary2.Add(98, "FARM_PART");
    dictionary2.Add(99, "ENVIRONMENT_RESOURCES");
    dictionary2.Add(100, "RACE_GROUP");
    dictionary2.Add(101, "ARTIFACTS");
    dictionary2.Add(102, "MAP_ITEM");
    dictionary2.Add(103, "BUILDINGS_SWAP");
    dictionary2.Add(104, "ITEMS_CULTURE");
    dictionary2.Add(105, "ANIMATION_EVENT");
    dictionary2.Add(107, "CROSSBOW");
    ModRecord.ModTypeCodes = dictionary2;
  }
}
