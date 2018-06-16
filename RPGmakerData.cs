using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace RPGmakerMV
{
    class Trait
    {
        private Dictionary<String, String> keyValues;
        public Dictionary<string, string> KeyValues { get => keyValues; set => keyValues = value; }
    }

    class VariantItem :ICloneable {
        private String mType = "";
        private int mId = 0;
        private int mValue = 0;

        [JsonProperty("type")]
        public String Type { get => mType; set => mType = value; }

        [JsonProperty("id")]
        public int Id { get => mId; set => mId = value; }
        [JsonProperty("value")]
        public int Value { get => mValue; set => mValue = value; }
        [JsonIgnore]
        public bool IsEmpty { get => String.IsNullOrEmpty(mType) ;  }

        [JsonIgnore]
        public bool IsArmor { get => mType == Armor.TypeText; }
        [JsonIgnore]
        public bool IsItem { get => mType == Item.TypeText;  }
        [JsonIgnore]
        public bool IsWeapon { get => mType == Weapon.TypeText; }
        [JsonIgnore]
        public bool IsSwitch { get => mType == Switch.TypeText; }
        [JsonIgnore]
        public bool IsVariable { get => mType == Variable.TypeText; }
        [JsonIgnore]
        public bool IsEvent { get => mType == CommonEvent.TypeText; }

        public VariantItem() { }

        public VariantItem(String type,int id,int value=0)
        {
            mType = type;
            mId = id;
            mValue = value;
        }

        public BaseItem[] DataList()
        {
            if (IsItem)
            {
                return GameData.Items;
            }
            if (IsArmor)
            {
                return GameData.Armors;
            }
            if (IsWeapon)
            {
                return GameData.Weapons;
            }
            if (IsEvent)
            {
                return GameData.CommonEvents;
            }
            if (IsVariable)
            {
                return GameData.System.Variables;
            }
            if (IsSwitch)
            {
                return GameData.System.Switches;
            }
            return new BaseItem[0];
        }

        public BaseItem GetBaseItem()
        {
            var list = this.DataList();
            if (this.Id < list.Length)
            {
                return list[Id];
            }
            return null;
        }
        public void Clear()
        {
            Id = 0;
            Value = 0;
            Type = "";

        }
        public VariantItem DeepCopy()
        {
            return new VariantItem
            {
                Id = this.Id,
                Value = this.Value,
                Type =this.Type
            };
        }

        public object Clone()
        {
            return this.DeepCopy();
        }
    }

    abstract class BaseItem
    {
        private String mName = "";
        private int mId = 0;

        [JsonProperty("name")]
        public string Name { get => mName; set => mName = value; }
        [JsonProperty("id")]
        public int Id { get => mId; set => mId = value; }

        [JsonIgnore]
        public string NameWithId {
            get => String.Format("{0:0000}", Id) +" : " + Name;
        }

        public abstract VariantItem VariantItem();

        public static T[] LoadJSON<T> (string JSONtext) where T : new()
        {
            var v = JsonConvert.DeserializeObject<T[]>(JSONtext);
            if (v[0] == null)
            {
                v[0] = new T();
            }
            return v;
        }
        public static T[] LoadJsonFromFile<T>(string fiePath) where T : new()
        {
            if (!File.Exists(fiePath)) { return new T[0]; }

            using (var stream = new StreamReader(fiePath))
            {
                if (!stream.EndOfStream)
                {
                    var result = LoadJSON<T>(stream.ReadToEnd());
                    return result;
                }
            }
            return new T[0];
        }
    }
    class Armor : BaseItem {
        public static string TypeText { get => "armor"; }

        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }
    class Weapon: BaseItem{
        public static string TypeText { get => "weapon"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }

    class Item : BaseItem {
        public static string TypeText { get => "item"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }


    class ConditionSwitch 
    {
        private int mId =0;
        [JsonProperty("id")]
        public int Id { get => mId; set => mId = value; }
    }
    class ConditionVariable {
        private int mId = 0;
        private int mValue = 0;

        [JsonProperty("id")]
        public int Id { get => mId; set => mId = value; }
        [JsonProperty("value")]
        public int Value { get => mValue; set => this.mValue = value; }
        public void Clear()
        {
            mId = 0;
            mValue = 0;
        }

        public ConditionVariable DeepCopy()
        {
            return new ConditionVariable { Value = this.Value, Id = this.Id };
        }
    }



    class Variable : BaseItem{
        public static string TypeText { get => "variable"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }

    class Switch : BaseItem
    {
        public static string TypeText { get => "switch"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }

    class CommonEvent : BaseItem {
        public static string TypeText { get => "event"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }
    class Actor : BaseItem {
        public static string TypeText { get => "actor"; }
        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }
    }

    // Systemにすると重複して死ぬので、こうするよ
    class SystemData
    {
        private Switch[] mSwitches = new Switch[0];
        private Variable[] mVariables = new Variable[0];

        static Variable[] StringListToArrayV(IList<String> list)
        {
            return list.Select((value, index) => {
                var v = new Variable
                {
                    Id = index,
                    Name = value ?? ""
                };
                return v;
            }).ToArray();
        }
        static Switch[] StringListToArrayS(IList<String> list)
        {
            return list.Select((value, index) => {
                var v = new Switch
                {
                    Id = index,
                    Name = value ?? ""
                };
                return v;
            }).ToArray();
        }
        static string[] ListToString( BaseItem[] variables)
        {
            return variables.Select(v => v !=null ? v.Name :"").ToArray();
        }

        [JsonProperty("variables")]
        public string[] VariablesString {
            get => ListToString(mVariables);
            set => mVariables = StringListToArrayV(value);
        }

        [JsonProperty("switches")]
        public string[] SwitchesString {
            get => ListToString(mSwitches);
            set => mSwitches = StringListToArrayS(value);

        }
        internal Variable[] Variables { get => mVariables; set => mVariables = value; }
        internal Switch[] Switches { get => mSwitches; set => mSwitches = value; }

        public static SystemData LoadJSON(string jsonText)
        {
            var result = JsonConvert.DeserializeObject<SystemData>(jsonText);
            return result;
        }
    }

    class GameData
    {
        private static Armor[] sArmors;
        private static Item[] sItems;
        private static Weapon[] sWeapons;
        private static SystemData sSystem;
        private static Actor[] sActors;

        private static CommonEvent[] sCommonEvents;

        internal static SystemData System { get => sSystem; }
        internal static Armor[] Armors { get => sArmors;  }
        internal static Item[] Items { get => sItems;  }
        internal static Weapon[] Weapons { get => sWeapons;  }
        internal static CommonEvent[] CommonEvents { get => sCommonEvents;  }
        internal static Actor[] Actors { get => sActors;  }

        static void LoadSystem()
        {
            const string sys = "System.json";

            
            if (File.Exists(sys))
            {
                using (var stream = new StreamReader(sys))
                {
                    sSystem = SystemData.LoadJSON(stream.ReadToEnd());
                }
            }
            else
            {
                sSystem = new SystemData();
            }


        }

        public static void LoadFiles()
        {

            sArmors = BaseItem.LoadJsonFromFile<Armor>("Armors.json");
            sItems = BaseItem.LoadJsonFromFile<Item>("Items.json");
            sWeapons = BaseItem.LoadJsonFromFile<Weapon>("Weapons.json");
            sCommonEvents = BaseItem.LoadJsonFromFile<CommonEvent>("CommonEvents.json");
            sActors = BaseItem.LoadJsonFromFile<Actor>("Actors.json");

            LoadSystem();

        }

    }

    


}
