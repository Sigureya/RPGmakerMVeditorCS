using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPGmakerMV
{

    using QuestMessage = Dictionary<string, List<string>>;


    [Serializable]
    class Quest : BaseItem
    {
        public static string TypeText { get => "quest"; }

        public override VariantItem VariantItem()
        {
            return new VariantItem(TypeText, Id);
        }

        private ConditionVariable mConditionVariable = new ConditionVariable();
        private List<VariantItem> mReward = new List<VariantItem>();
        private QuestMessage mMessage = new QuestMessage();
        private int enabledSwitch = 0;
        //        private Dictionary<String, int> parameter = new Dictionary<string, int>();

        public Quest() { }

        public Quest(int id )
        {
            this.Id = id;
            this.Name = "hage" + id;
        }
        public void Clear()
        {
            Name = "";
            mMessage.Clear();
            mReward.Clear();
            ConditionSwitch = 0;
        }

        public Quest DeepCopy()
        {
            return new Quest
            {
                Name = Name,
                ConditionSwitch = ConditionSwitch,
                ConditionVariable = ConditionVariable.DeepCopy(),
                Message = CloneMessage(),
                Reward = CloneReward(),
            };
        }

        public QuestMessage CloneMessage()
        {
            QuestMessage result = new QuestMessage();
            foreach (var pair in mMessage)
            {
                result.Add(
                    pair.Key,
                    new List<string>( pair.Value.Select((s)=>s))
                 );
            }
            return result;
        }

        public void Normalize()
        {
            mReward.RemoveAll((item) => item.IsEmpty);
        }

        public List<VariantItem> CloneReward()
        {
           return new List < VariantItem >( mReward.Select((item)=>item.DeepCopy()) );
        }

        [JsonProperty("id")]
        public new int Id {
            get => ((BaseItem)(this)).Id;
            set => ((BaseItem)(this)).Id = value;
        }
        [JsonProperty("name")]
        public new string Name
        {
            get => ((BaseItem)(this)).Name;
            set => ((BaseItem)(this)).Name = value;
        }

        [JsonProperty("cond_s")]
        public int ConditionSwitch { get => enabledSwitch; set => enabledSwitch = value; }

        [JsonProperty("cond_v")]
        internal ConditionVariable ConditionVariable { get => mConditionVariable; set => mConditionVariable = value; }


        [JsonProperty("message")]
        public Dictionary<string, List<string>> Message { get => mMessage; set => mMessage = value; }

        [JsonProperty("reward")]
        public List<VariantItem> Reward { get => mReward; set => mReward = value; }
    }
}
