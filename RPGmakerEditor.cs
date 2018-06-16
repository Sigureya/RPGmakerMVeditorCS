using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace RPGmakerMV
{

    class ItemSelectComboBox : ComboBox, IItemSelectable
    {
        private IReadOnlyList<BaseItem> mList;
        public ItemSelectComboBox(IReadOnlyList<BaseItem> list)
        {
            mList = list;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DataSource = list;
            DisplayMember = "NameWithId";
            Width = 160;
        }

        public VariantItem Reward()
        {
            if (SelectedItem is BaseItem baseItem)
            {
                return baseItem.VariantItem();
            }
            return null;
        }
    }

    class ItemSelect
    {
        private ItemSelectComboBox mItemSelect;
        private RadioButton mRadioButton;

        void OnCheckedChanged(object o,EventArgs e)
        {            
            mItemSelect.Enabled = mRadioButton.Checked;
        }
        
        public ItemSelect(String text, ItemSelectComboBox itemSelect) : base()
        {
            mItemSelect = itemSelect;
            mItemSelect.Enabled = false;

            mRadioButton = new RadioButton
            {
                Text = text
            };


            mRadioButton.CheckedChanged += this.OnCheckedChanged;

            Location = new Point(0, 0);
        }
        void SetLocation(Point point)
        {
            mRadioButton.Location = point;
            mItemSelect.Location = new Point(mRadioButton.Right + 10, mRadioButton.Top);
        }

        public bool IsMatch(VariantItem item)
        {
            return item.Type ==mRadioButton.Text;
        }


        public VariantItem Reward()
        {
            return this.mItemSelect.Reward();
        }
        public ItemSelectComboBox ComboBox { get => mItemSelect; }
        public RadioButton RadioButton { get => mRadioButton; }

        public Point Location { set => SetLocation(value); }
        public int Left { get => mRadioButton.Left; }
        public int Bottom { get => mRadioButton.Bottom; }

        public static List<ItemSelect> AllInList()
        {
            return new List<ItemSelect>
            {
                Item(),
                Weapon(),
                Armor(),
                Switch(),
                Variable(),
                Event()
            };
        }

        public static ItemSelect Item()
        {
            return new ItemSelect(RPGmakerMV.Item.TypeText, new ItemSelectComboBox( GameData.Items));
        }
        public static ItemSelect Armor()
        {
            return new ItemSelect(RPGmakerMV.Armor.TypeText, new ItemSelectComboBox(GameData.Armors));
        }
        public static ItemSelect Weapon()
        {
            return new ItemSelect(RPGmakerMV.Weapon.TypeText, new ItemSelectComboBox(GameData.Weapons));
        }
        public static ItemSelect Actor()
        {
            return new ItemSelect(RPGmakerMV.Actor.TypeText, new ItemSelectComboBox(GameData.Actors));
        }
        public static ItemSelect Variable()
        {
            return new ItemSelect(RPGmakerMV.Variable.TypeText, new ItemSelectComboBox(GameData.System.Variables));
        }
        public static ItemSelect Switch()
        {
            return new ItemSelect(RPGmakerMV.Switch.TypeText, new ItemSelectComboBox(GameData.System.Switches));
        }
        public static ItemSelect Event()
        {
            return new ItemSelect(RPGmakerMV.CommonEvent.TypeText, new ItemSelectComboBox(GameData.CommonEvents));
        }


    }

    abstract class PopupForm : Form
    {

        private Button mOkButton = new Button { Text ="Ok"};
        private Button mCancelButton = new Button { Text = "Cancel" };

        public Button OkButton { get => mOkButton;  }
        public new Button CancelButton { get => mCancelButton;  }

        void SetCancel()
        {
            Form f = this;
            f.CancelButton = CancelButton;
        }

        public PopupForm()
        {
            this.FormClosing += ClosingXXX;
            SetCancel();
//            (Form)(this)CancelButton = mCancelButton;
            mOkButton.Click += (o, e) => OnOk();
            mCancelButton.Click += (o,e) => this.Hide();
            Controls.Add(mOkButton);
            Controls.Add(mCancelButton);
        }

        void ClosingXXX(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public abstract void OnOk();
    }

    class ItemMaxButton : Button
    {

    }
    class ItemMaxForm : Form
    {
        private NumericUpDown mValue;// = new NumericUpDown();
        private Button mOkButton;
        private Button mCancelButton;

        public Button OkButton { get => mOkButton; }
        public Decimal Value  { get => mValue.Value; set => mValue.Value = value; }

        public ItemMaxForm()
        {
            mValue = new NumericUpDown() { Location = new Point(10, 10),Minimum=1 };


            mOkButton = new Button
            {
                Text = "Ok",
                Location = new Point(mValue.Left, mValue.Bottom + 10)
            };
            mCancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(mOkButton.Right + 10, mOkButton.Top)
            };
            mCancelButton.Click += (o, e) => this.Close();
            mOkButton.Click += (o, e) =>{ this.Close(); };

            Width = mOkButton.Width + mCancelButton.Width+40;
            Height = 120;
            StartPosition = FormStartPosition.Manual;
            MaximizeBox = false;
            MinimizeBox = false;
            Controls.Add(mValue);
            Controls.Add(mOkButton);
            Controls.Add(mCancelButton);
        }
    }



    class VariantItemButton : Button
    {
        private VariantItem variantItem;

        internal VariantItem VariantItem
        {
            get => variantItem;
            set => SetItem(value);
        }

        public VariantItemButton() : base()
        {
            SetItem(new VariantItem());
        }

        public void SetItem(VariantItem item)
        {
            variantItem = item ?? new VariantItem();
            Text = variantItem.IsEmpty ? "なし" : variantItem.GetBaseItem().Name;
        }

        public void Clear()
        {
            variantItem.Clear();
        }
    }

    class ItemSelectForm : Form
    {
        private Button mOkButton = new Button { Text = "Ok" };
        private Button mCancelButton = new Button { Text = "Cancel" };

        public Button OkButton { get => mOkButton; }

        private ItemSelect mSeletedItem;
        private List<ItemSelect> mItemSelects;
        private GroupBox itemGroup;
        private VariantItemButton mBindedButton;

        public ItemSelectForm(List<ItemSelect> list)
        {
            Width = 360;
            mItemSelects = list;
            CreateSelector();
            CreateButtons();
            ResetLocation();
            StartPosition = FormStartPosition.CenterParent;
        }
        void CreateButtons()
        {
            mOkButton.Location = new Point(10, itemGroup.Bottom + 10);
            mCancelButton.Location = new Point(mOkButton.Right+10, mOkButton.Top);

            Controls.Add(mOkButton);
            Controls.Add(mCancelButton);
            CancelButton = mCancelButton;
            mOkButton.Click += (o, e) => OnOk();
        }

        public void OnOk()
        {
            CancelButton.PerformClick();
            if (mSeletedItem != null)
            {
                VariantItem item = mSeletedItem.Reward();
                mBindedButton.SetItem(item);
            }
        }

        void SetSelectedItem( ItemSelect itemSelect)
        {
            mSeletedItem = itemSelect;
        }

        void CreateSelector()
        {
            const int lineHeigth = 24;
            int length = mItemSelects.Count;
            itemGroup = new GroupBox
            {
                Location = new Point(10, 10),
                Text = "項目",
                Width = Width - 20
            };
            ;
            itemGroup.Height = lineHeigth * length +40;

            Controls.Add(itemGroup);
            const int x = 10;
            for(int i=0; i < length; ++i)
            {
                var item = mItemSelects[i];
                item.Location = new Point(x, i * lineHeigth+20);
                item.RadioButton.CheckedChanged += (o, e) =>
                {
                    if (item.RadioButton.Checked)
                    {
                        this.SetSelectedItem(item);
                    }
                };
                itemGroup.Controls.Add(item.RadioButton);
                itemGroup.Controls.Add(item.ComboBox);
            }


        }

        void ResetLocation()
        {
            itemGroup.Location = new Point(0, 0);

            mOkButton.Location = new Point(itemGroup.Left, itemGroup.Bottom);
            mCancelButton.Location = new Point(OkButton.Right + 10, OkButton.Top);
        }


        public void Add(ItemSelect item)
        {
            var last = mItemSelects.Last();
            if (last == null)
            {
                item.Location = new Point(0, 0);
            }
            else
            {
                item.Location = new Point(last.Left, last.Bottom + 10);
            }
            mItemSelects.Add(item);
        }

        void SelectFirst()
        {
            var firts = mItemSelects.First();
            if (firts != null)
            {
                firts.RadioButton.Checked = true;
                SetSelectedItem(firts);
            }
        }

        void SelectByItem(VariantItem item)
        {
            if(item ==null ||  item.IsEmpty)
            {
                SelectFirst();
                return;
            }

            ItemSelect element= mItemSelects.Find((s) => s.IsMatch(item));
            
            if(element == null) {
                SelectFirst();
            }
            else
            {
                SetSelectedItem(element);
            }

        }

        void ButtonCliked(VariantItemButton button)
        {
            if( !this.Visible)
            {
                mBindedButton = button;
                SelectByItem(button.VariantItem);

                ShowDialog(this.Owner);
            }
        }

        public VariantItemButton NewButton()
        {
            var button = new VariantItemButton();
            button.Click += (o, s) => { this.ButtonCliked(button); };
            return button;
        }
    }

}
