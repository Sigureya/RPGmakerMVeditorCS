using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
//using System.ValueTuple;

namespace RPGmakerMV
{
//    using RewardItem = VariantItem;
    using RewardTuple = Tuple<VariantItemButton, NumericUpDown>;
    delegate List<Quest> GetQuestListDelegate();

    class TextTab : TabPage
    {
        private TextBox mText;

        public TextTab(String name ):base(name)
        {
            mText = new TextBox
            {
                Multiline = true
            };
            Controls.Add(mText);
        }

        public TextBox TextForm { get => mText; }
    }

    class MessageEditor : TabControl
    {
        private List<TextTab> textTabs = new List<TextTab>();
        private Button okButton = new Button();

        public MessageEditor()
        {
            this.Width = 240;
            this.Height = 240;
            this.CreateTabs(new String[] { "displayed","hidden",  "started","finished" });
        }

        void CreateTabs( IList<string> pageList)
        {

            var clientSize = this.ClientSize;
            clientSize.Width -= this.Padding.X * 2;
            clientSize.Height -= this.Padding.Y * 2+30;

            foreach (var name in pageList){
                var tabPage = new TextTab(name);

                tabPage.TextForm.ClientSize = clientSize;
                Controls.Add(tabPage);
                textTabs.Add(tabPage);
            }
        }

        public Dictionary<String,List<String>> GetData()
        {
            var result = new Dictionary<String, List<String>>();

            foreach(var tab in textTabs) {
                result.Add(tab.Text, new List<string> (tab.TextForm.Lines.Select((s)=>s)));
            }
            return result;
        }
        public void SetData(Dictionary<String, List<String>> valuePairs)
        {
            foreach (var tab in textTabs)
            {
                if (valuePairs.ContainsKey(tab.Text))
                {
                    tab.TextForm.Lines = valuePairs[tab.Text].ToArray<string>();
                }
                else
                {
                    tab.TextForm.Lines = new string[0];
                }
            }
            this.Update();
        }
    }

    class BaseItemListBox<T> :ComboBox where T : BaseItem 
    {
        public BaseItemListBox(IList<T> list):base()
        {
            DataSource = list;
            DisplayMember ="NameWithId";
            Width = 160;
        }
        public T Item()
        {
           return (T)this.SelectedItem;
        }
    }


    interface IItemSelectable {

       VariantItem Reward();
    }

    class SaveModeItems: ToolStripMenuItem
    {
        private ToolStripMenuItem mIndented;
        private ToolStripMenuItem mNone;

         SaveModeItems()
        {
            Text = "保存形式";
//            mIndented.ch
        }

        public string QuestListToString(List<Quest> list)
        {
            string jsonText = JsonConvert.SerializeObject(list, Formatting.Indented);
            return jsonText;
        }

        void hage(ToolStripMenuItem seletedItem)
        {
            foreach(ToolStripMenuItem item in this.DropDownItems)
            {
                if(seletedItem != item)
                {
                    item.Checked = false;
                }
            }
        }

    }

    class FileMenuItems  : ToolStripMenuItem
    {

        ToolStripMenuItem fileNew;
        ToolStripMenuItem fileOpen;
        ToolStripMenuItem fileSave ;
        ToolStripMenuItem fileOverwrite;
        ToolStripMenuItem fileClose;

        public ToolStripMenuItem Save { get => fileSave; }
        public ToolStripMenuItem Overwrite { get => fileOverwrite;  }
        public ToolStripMenuItem Open { get => fileOpen; }


        public FileMenuItems(  )
        {
           CreateFileMenu();
        }

        public ToolStripMenuItem[] items()
        {
            return new ToolStripMenuItem[]{
              fileNew, fileOpen, fileSave,fileOverwrite,fileClose
             };
        }

        void CreateFileMenu()
        {
            Text = "ファイル";

            fileNew = new ToolStripMenuItem
            {
                Text ="新規",
                ShortcutKeys = Keys.Control | Keys.N
            };

            fileOpen = new ToolStripMenuItem {
                Text ="開く",
                ShortcutKeys = Keys.Control | Keys.O
            };
            fileSave = new ToolStripMenuItem
            {
                Text = "名前を付けて保存",
            };

            fileOverwrite = new ToolStripMenuItem()
            {
                Text = "上書き保存",
                ShortcutKeys = Keys.Control | Keys.S
            };


            fileClose = new ToolStripMenuItem
            {
                Text ="閉じる"
            };
            this.DropDownItems.AddRange(new ToolStripMenuItem[]{
                fileOpen,fileNew , fileSave,fileOverwrite,fileClose
            });
        }




    }

    
    class SaveModeList : ComboBox
    {
        const string modeMV = "ツクールMV風";
        const string modeIndented = "整列";
        const string modeNone = "そのまま";

        public SaveModeList()
        {
            this.Items.AddRange(new string[] {modeMV,modeIndented,modeNone});

            DropDownStyle = ComboBoxStyle.DropDownList;
        }
        public string QuestListToString(List<Quest> list)
        {
            string mode = SelectedItem.ToString();

            string jsonText = JsonConvert.SerializeObject(list, Formatting.Indented);
            return jsonText;
        }


    }

    class QuestEditor : Form
    {
        private OpenFileDialog mFileDialog;


        private string mFileName = "Quest.json";
        private Quest[] mDeepCopyed; 
        private MenuStrip mMenuStrip;
        private FileMenuItems mFileMenu;

        private SaveModeList mSaveMode;

        private BindingSource bindingSource;

        private List<Quest> mQuestList = new List<Quest>();
        private ListBox mQuestListBox;
        private TextBox questName;
        private MessageEditor messge;
        private Quest mQuest = null;
        private BaseItemListBox<Switch> mSwitch;
        private BaseItemListBox<Variable> mVariable;
        private NumericUpDown mVariabeValue;
        private ItemSelectForm mItemSelect;
        private ItemMaxForm mItemMaxForm;
        private Button mItemMaxButton;

        private GroupBox mQuestListGrop;
        private GroupBox mQuestNameGrop;
        private GroupBox mConditionGrop;
        private GroupBox mRewardGrop;
        private GroupBox mMessageGrop;

        private List<RewardTuple> mRewardButtons;

        private static int GropBoxWidth { get => 280; } 

        void ReflectOnQuest()
        {
            if(mQuest == null) { return; }
            mQuest.Message = messge.GetData();
            mQuest.Name = questName.Text;
            mQuest.Reward = this.GetReward();
            mQuest.ConditionSwitch = mSwitch.SelectedIndex;
        }

        void ClearEditingData()
        {
            messge.SetData(new Dictionary<string, List<string>>());
            questName.Text = "";
            foreach(var item in mRewardButtons)
            {
                item.Item1.Clear();
            }

        }

        void SetQuest(Quest quest)
        {
            
            ReflectOnQuest();
            mQuest = quest;
            if(mQuest == null) { return; }

            messge.SetData(quest.Message);
            questName.Text = quest.Name;
            SetReward(quest.Reward);
            mSwitch.SelectedIndex = quest.ConditionSwitch;
        }

        int QuestNameWidth { get => 160; }

        public QuestEditor()
        {
            this.SuspendLayout();
            this.Text = "RPGmakerMV QuestEditor";
            this.ClientSize = new System.Drawing.Size(800, 400);
            CreateItemMaxForm();
            CreateMenuStrip();
            CreateQuestList();
            CreateItemSelect();

            CreaetQuestName();
            CreateCondition();
            CreateRewardButton();
            CreateMessageEditor();

            mQuestListBox.Select();
            this.ResumeLayout(false);

        }

        void CreateItemSelect()
        {
            mItemSelect = new ItemSelectForm(ItemSelect.AllInList());
        }

        void SetListLength(int n)
        {
            bool questListEmpty = mQuestList.Count == 0;
            int questCount = mQuestList.Count;
            for(int i = questCount; i < n; ++i)
            {
                mQuestList.Add(new Quest(i));
            }
            bindingSource.ResetBindings(false);
            //if ( mQuestListBox.SelectedItem  is Quest quest )
            //{
            //    this.SetQuest(quest);
            //}
            
        }
        void CreateItemMaxForm()
        {
            mItemMaxForm = new ItemMaxForm {Text ="クエストの最大数"};
            mItemMaxForm.OkButton.Click += (o, e) =>
            {
                this.SetListLength((int)mItemMaxForm.Value);
            };
        }

        string QuestListToString()
        {
            string jsonText = JsonConvert.SerializeObject(mQuestList, Formatting.Indented);
            return jsonText;
        }

        void LoadJsonText(string jsonText)
        {
            List<Quest> tmp =  JsonConvert.DeserializeObject<List<Quest>>(jsonText);
            mQuestList = tmp;
            bindingSource.DataSource = tmp;
            
        }

        void ExecuteLoad()
        {
            if (File.Exists(mFileName))
            {
                using (var stream = new StreamReader(mFileName))
                {
                    if (!stream.EndOfStream)
                    {
                        string text = stream.ReadToEnd();
                        LoadJsonText(text);
                    }
                }
            }
        }

        void ExecuteSave()
        {
            foreach(Quest quest in mQuestList)
            {
                quest.Normalize();
            }
            

            using (StreamWriter streamWriter = new StreamWriter(this.mFileName))
            {
                string jsonText = this.QuestListToString();
                streamWriter.Write(jsonText);
            }

        }
        void CreateMenuStrip()
        {
            mFileDialog = new OpenFileDialog();
            mFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
//            mFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            mFileDialog.FileOk += MFileDialog_FileOk;

            mMenuStrip = new MenuStrip();
            mFileMenu = new FileMenuItems();
            mFileMenu.Overwrite.Click += (o, e) => ExecuteSave();

            mFileMenu.Open.Click += (o, e) => {
                mFileDialog.ShowDialog(this);
            };
//            mFileMenu.op

            Controls.Add(mMenuStrip);
            mMenuStrip.Items.Add(mFileMenu);
        }

        private void MFileDialog_FileOk(object sender, CancelEventArgs e)
        {
           this.mFileName =  mFileDialog.FileName;
            ExecuteLoad();
        }

        void CreateRewardButton()
        {
            const int buttonWidth = 160;
            const int numericWidth = 80;

            mRewardGrop = new GroupBox
            {
                Width = GropBoxWidth,
                Text = "クエスト報酬",
                Location = new Point(mConditionGrop.Left, mConditionGrop.Bottom + 10)
            };
            mRewardButtons = new List<RewardTuple>();


            for(int i = 0; i < 3; ++i)
            {
                var button = mItemSelect.NewButton();
                NumericUpDown num = new NumericUpDown();
                RewardTuple tuple = new RewardTuple(button, num);

                button.Location = new Point(0, i * (button.Height + 5)+20  );
                button.Width = buttonWidth;

                num.Width = numericWidth;
                num.Maximum = 999999;
                num.Location = new Point(button.Right+10, button.Top);

                num.ValueChanged += (o,e)=>{
                    button.VariantItem.Value =(int)num.Value;
                };

                mRewardGrop.Controls.Add(num);
                mRewardGrop.Controls.Add(button);
                mRewardButtons.Add(tuple);
            }
            mRewardGrop.Height = mRewardButtons.Count * 40;
            mItemSelect.Owner = this;
            Controls.Add(mRewardGrop);
        }
        List<VariantItem> GetReward()
        {
            List<VariantItem> result = new List<VariantItem>();

            foreach(var tuple in mRewardButtons)
            {
                result.Add(tuple.Item1.VariantItem);
            }
            return result;
        }

        void SetReward(IList<VariantItem> rewardItems)
        {
            int length =mRewardButtons.Count;
            for(int i=0;i < length; ++i)
            {

                if (i < rewardItems.Count)
                {
                    var hage = rewardItems[i];
                    mRewardButtons[i].Item1.VariantItem = hage;
                    mRewardButtons[i].Item2.Value = hage == null ? 0 : hage.Value;
                }
                else
                {
                    mRewardButtons[i].Item1.VariantItem = null;
                    mRewardButtons[i].Item2.Value = 0;
                }
            }
        }
        void CreateCondition()
        {
            mConditionGrop = new GroupBox
            {
                Location = new Point(mQuestNameGrop.Left, mQuestNameGrop.Bottom+ 10),
                Text = "クエスト出現条件",
                Width = GropBoxWidth,
                Height = 80
            };

            mSwitch = new BaseItemListBox<Switch>(GameData.System.Switches)
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 20)
            };

            mVariable = new BaseItemListBox<Variable>(GameData.System.Variables)
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(mSwitch.Left, mSwitch.Bottom + 5)
            };


            mVariabeValue = new NumericUpDown
            {
                Location = new Point(mVariable.Right + 4, mVariable.Top),
                Width = 68,
                Maximum = 999999
            };

            mConditionGrop.Controls.Add(mSwitch);
            mConditionGrop.Controls.Add(mVariable);
            mConditionGrop.Controls.Add(mVariabeValue);
            Controls.Add(mConditionGrop);
        }


        void CreateMessageEditor()
        {
            messge = new MessageEditor {
                Location =new Point(10,10)
            };
            mMessageGrop = new GroupBox
            {
                Location = new Point( mQuestNameGrop.Right+10,mQuestNameGrop.Top),
                Width = messge.Width +20,
                Height = messge.Height +20
            };

            mMessageGrop.Controls.Add(messge);
            Controls.Add(mMessageGrop);
        }

        void OnQuestNameChange(object sender, EventArgs e)
        {
           if (questName.Focused)
            {
                if( mQuestListBox.SelectedItem is Quest quest)
                {
                    quest.Name = questName.Text;
                    bindingSource.ResetBindings(false);
                }
            }
        }


        void CreaetQuestName()
        {
            questName = new TextBox
            {
                Width = QuestNameWidth,
                Location = new Point(0 ,14)
            };
            questName.TextChanged += this.OnQuestNameChange;
            mQuestNameGrop = new GroupBox {
                Location = new Point(mQuestListGrop.Right + 10, mQuestListGrop.Top),
                Text = "クエスト名",
                Width = GropBoxWidth,
                Height=40
            };

            mQuestNameGrop.Controls.Add(questName);
            Controls.Add(mQuestNameGrop);
        }
        void OnQuestListIndexChange(object sender, EventArgs e)
        {
            this.SetQuest((Quest)mQuestListBox.SelectedItem);
        }

        void SetQuestList()
        {
            var list = new List<Quest>();
            bindingSource.DataSource = list;
        }

        void CreateQuestList()
        {
            mQuestListGrop = new GroupBox()
            {
                Location = new Point(10, mMenuStrip.Bottom),
                Height = 320
            };
            Controls.Add(mQuestListGrop);

            mQuestListBox = new ListBox
            {
                DisplayMember = "NameWithId",
                Location = new System.Drawing.Point(10, 10),
                Name = "quest",
                SelectionMode =SelectionMode.MultiExtended,
//                MultiColumn = true,
                ColumnWidth = QuestNameWidth,
                Width = QuestNameWidth,
                Height = mQuestListGrop.Height -50
            };
            bindingSource = new BindingSource
            {
                DataSource = mQuestList,
            };
            mQuestListBox.DataSource = bindingSource;
            mQuestListBox.SelectedIndexChanged += new System.EventHandler(this.OnQuestListIndexChange);
            mQuestListBox.KeyDown += MQuestListBox_KeyDown;

            this.Controls.Add(mQuestListBox);
            Load += (o, e) => {
                this.SetListLength(1);
                if (File.Exists(mFileName))
                {
                    ExecuteLoad();
                }
            };
            mQuestListGrop.Controls.Add(mQuestListBox);
            mItemMaxButton = new Button
            {
                Location = new Point(mQuestListBox.Left + 6, mQuestListBox.Bottom + 6),
                Width = mQuestListBox.Width - 10,
                Text = "最大数の変更"
            };
            mItemMaxButton.Click += (o, e) => 
            {
                mItemMaxForm.Value = mQuestList.Count;
                mItemMaxForm.Location =  new Point(mQuestListGrop.Right+10,400);
                mItemMaxForm.ShowDialog(this);
            };
            mQuestListGrop.Controls.Add(mItemMaxButton);
        }

        void ExecuteDelete()
        {
            var list = mQuestListBox.SelectedItems;
            foreach (Quest quest in list)
            {
                quest.Clear();
                if(quest == mQuest)
                {
                    ClearEditingData();
                }
            }
            //            mQuestListBox.
            bindingSource.ResetBindings(false);
        }

        static IEnumerable<Quest> QusetArrayDeepCopy(IEnumerable<Quest> list)
        {
            return list.Select((quest) => quest.DeepCopy());
        }

        void PasteItems()
        {
            int length = Math.Min(mQuestList.Count, mDeepCopyed.Length);
            int index = mQuestListBox.SelectedIndex;

            for(int i = 0; i < length; ++i) {

                if(i + index < mQuestList.Count)
                {
                    mQuestList[i+index] = mDeepCopyed[i].DeepCopy();
                    mQuestListBox.SetSelected(i + index, true);
                }
            }
            bindingSource.ResetBindings(false);

        }

        void CopyItems()
        {
            mDeepCopyed = QusetArrayDeepCopy(  mQuestListBox.SelectedItems.Cast<Quest>() ).ToArray();
        }

        private void MQuestListBox_KeyDown(object sender, KeyEventArgs e)
        {
            //ショートカットメモ
            //CTRL+C　コピー
            //CTRL+SHIFT+C テキストでコピー
            //CTRL+V 貼り付け　テキストなら、頑張って変換する 

            if (e.Control)
            {
                if(e.KeyCode == Keys.C)
                {
                    CopyItems();
                    return;
                }
                if(e.KeyCode == Keys.V)
                {
                    PasteItems();
                    return;
                }
            }

            if(e.KeyData == Keys.Delete)
            {
                ExecuteDelete();
                return;
            }
        }
    }
}
