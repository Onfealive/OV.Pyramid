using CustomFileExtensionControl;
using JsonNet.PrivateSettersContractResolvers;
using Microsoft.Win32;
using Newtonsoft.Json;
using OV.Core;
using OV.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static OV.Tools.Utilities;

namespace OV.Pyramid
{
    
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private YGOSet Set = new YGOSet();
        private int currentIndex = -1;

        private bool inRefreshControl = false;
        private bool inChangeSetCard;
        
        private static string DatabasePath = GetLocationPath() + @"\Resources\Datas.ld";

        private ByteDatabase Database = new ByteDatabase(DatabasePath);

        private YGOCard Current
        {
            get
            {
                if (currentIndex < 0) return null;
                return Set.Cards[currentIndex];
            }
            set
            {
                if (currentIndex < 0) return;
                Set.Cards[currentIndex] = value;
            }
        }
        public MainPage()
        {
            
            InitializeComponent();
            FirstLoad();
            
            //this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            //this.Dispatcher.ShutdownFinished += Dispatcher_ShutdownFinished;
            /*
            Loaded += (s, e) => { // only at this point the control is ready
                Window.GetWindow(this) // get the parent window
                      .Closed += (s1, e1) => Directory.Delete(TempFolder, true);  //disposing logic here
            }; */
        }

        private void FirstLoad()
        {
            //SaveDatabase();

            //Set default global Style for Paragraph: Margin = 0
            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));
            Resources.Add(typeof(Paragraph), style);


            Set.Cards.Add(YGOCard.Default);
            RenderCard.SetDefaultArtwork(Database.GetData(@"Template\NoneImage.png").Bytes);

            LoadControl();
            LoadButton();
            RefreshSetting();
            this.Loaded += new RoutedEventHandler(MainContainer_Loaded);

            cardList.ItemsSource = Set.Cards;
            cardList.SelectedIndex = 0;
        }

        private void SaveDatabase()
        {
            Database.Generate(GetLocationPath() + @"\Resources2\");
        }
        
        private void RefreshArtwork()
        {
            inChangeSetCard = true;
            ICollectionView view = CollectionViewSource.GetDefaultView(Set.Cards);
            view.Refresh();
            inChangeSetCard = false;
            if (Current == null) { return; }

            RenderCard.Render(Current);

        }

        private void RefreshControl()
        {
            if (currentIndex == -1) { return; }
            inRefreshControl = true;

            NameEdit.Text = Current.Name;



            RefreshRarityControl();
            RefreshAttributeControl();
            RefreshMiddleControl();
            RefreshFrameControl();
            RefreshTypeControl();
            RefreshAbilityControl();



            RefreshATKControl();
            RefreshDEFControl();
            RefreshScaleControl();


            RefreshPendulumControl();
            RefreshDescriptionControl();

            RefreshCirculationControl();
            ArtworkPath.Text = "";
            artworkPaddingHeight.Text = RenderCard.RecommendedHeight.ToString();

            inRefreshControl = false;

        }

        private void RefreshSetting()
        {
            if (Properties.Settings.Default.Associated)
            {
                Associate.IsEnabled = false;
                Unassociate.IsEnabled = true;
            }
            else
            {
                Associate.IsEnabled = true;
                Unassociate.IsEnabled = false;
            }
            Properties.Settings.Default.Save();
        }

        private void RefreshCirculationControl()
        {
            foreach (object item in EditionCombo.Items)
            {
                if (item.ToString().Remove(" ") == Current.Edition.ToString())
                {
                    EditionCombo.SelectedItem = item;
                    break;
                }
            }

            foreach (object item in StickerCombo.Items)
            {
                if (item.ToString().Remove(" ") == Current.Sticker.ToString())
                {
                    StickerCombo.SelectedItem = item;
                    break;
                }
            }

            SetNumberEdit.Text = Current.Set.ToString();
            CardNumberEdit.Text = Current.Number > 0 ? Current.Number.ToString() : "";

            CreatorCheckBox.IsChecked = Current.Creator == CREATOR.NONE ? false : true;
        }

        private void RefreshPendulumControl()
        {
            PendulumBox.Document.Blocks.Clear();
            PendulumBox.AppendText(Current.PendulumEffect);
        }

        private void RefreshDescriptionControl()
        {
            DescriptionBox.Document.Blocks.Clear();
            DescriptionBox.AppendText(Current.Description);
        }

        private void RefreshScaleControl()
        {
            foreach (Button button in ScaleCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null)
                {
                    if (double.Equals(Current.ScaleLeft, Current.ScaleRight))
                    {
                        if (double.IsNaN(Current.ScaleLeft) && button.Tag.ToString() == "?")
                        {
                            button.IsEnabled = false;
                        }
                        else if (button.Tag.ToString() == Current.ScaleLeft.ToString())
                        {
                            button.IsEnabled = false;
                        }
                    }

                    if (Current.ScaleLeft > 0 || double.IsNaN(Current.ScaleLeft))
                    {
                        ScaleLeftBox.Text = double.IsNaN(Current.ScaleLeft) ? "?" : Current.ScaleLeft.ToString();
                    }
                    if (Current.ScaleRight > 0 || double.IsNaN(Current.ScaleRight))
                    {
                        ScaleRightBox.Text = double.IsNaN(Current.ScaleRight) ? "?" : Current.ScaleRight.ToString();
                    }
                }
            }
        }

        private void RefreshATKControl()
        {
            string value;
            if (double.IsNaN(Current.ATK))
            {
                value = "?";
            }
            else if (Current.ATK >= 0)
            {
                value = Current.ATK.ToString();
            }
            else
            {
                value = "";
            }
            ATKBox.Text = value;
        }

        private void RefreshDEFControl()
        {
            string value;
            if (double.IsNaN(Current.DEF))
            {
                value = "?";
            }
            else if (Current.DEF >= 0)
            {
                value = Current.DEF.ToString();
            }
            else
            {
                value = "";
            }
            DEFBox.Text = value;
        }

        private void RefreshAbilityControl()
        {
            foreach (Button button in AbilityCanvas.FindChildrens<Button>())
            {                
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;                    
                }
                if (button.Tag != null)
                {                    
                    if (button.Tag.ToString() == Current.Frame.ToString()) //Effect Ability
                    {
                        button.IsEnabled = false;
                    }

                    TextBlock content = button.FindChildrens<TextBlock>().ToList()[0] as TextBlock;
                    Image icon = button.FindChildrens<Image>().ToList()[0] as Image;
                    if (Current.Abilities.Contains(button.Tag.ToString().ToEnum<ABILITY>()))
                    {
                        if (content.Text != "Un-" + button.Tag)
                        {
                            content.Text = "Un-" + button.Tag;
                            icon.Source = Database.GetImage(@"Template\Ability\" + button.Tag + ".png")
                                .ToBitmap().Grayscale().ToBitmapSource();
                        }
                    }
                    else
                    {
                        if (content.Text != button.Tag.ToString())
                        {
                            content.Text = button.Tag.ToString();
                            icon.Source = Database.GetImage(@"Template\Ability\" + button.Tag + ".png");
                        }
                    }
                }
            }
        }

        private void RefreshTypeControl()
        {
            foreach (Button button in TypeCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null && button.Tag.ToString() == Current.Type.ToString())
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void RefreshFrameControl()
        {
            foreach (Button button in FrameCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null)
                {
                    if (button.Tag.ToString() == Current.Frame.ToString())
                    {
                        button.IsEnabled = false;
                    }

                    if (button.Tag.ToString() == "Pendulum")
                    {
                        
                        TextBlock content = button.FindChildrens<TextBlock>().ToList()[0] as TextBlock;
                        Image icon = button.FindChildrens<Image>().ToList()[0] as Image;
                        if (Current.IsPendulum)
                        {
                            content.Text = "Un-Pendulum";
                            icon.Source = Database.GetImage(@"Template\PendulumGrey.png");
                        }
                        else
                        {
                            content.Text = "Pendulum";
                            icon.Source = Database.GetImage(@"Template\Pendulum.png");
                        }
                    }
                }

            }
        }

        private void RefreshMiddleControl()
        {
            foreach (Button button in MiddleCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }

                if (button.Tag != null)
                {
                    if (button.Tag.ToString() == "Level_" + Current.Level.ToString()
                        || button.Tag.ToString() == "Rank_" + Current.Rank.ToString()
                        || button.Tag.ToString() == Current.Frame.ToString() + "_" + Current.Property.ToString())
                    {

                        button.IsEnabled = false;
                    }
                }
            }
        }

        private void RefreshAttributeControl()
        {
            foreach (Button button in AttributeCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null && button.Tag.ToString() == Current.Attribute.ToString())
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void RefreshRarityControl()
        {
            foreach (Button button in RarityCanvas.FindChildrens<Button>())
            {
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null && button.Tag.ToString() == Current.Rarity.ToString())
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void LoadButton()
        {
            Button[] Stack = new Button[] { Home, Talk, About, Export, Save, Load, Exit };
            for (int i = 0; i < Stack.Length; i++)
            {
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Vertical;

                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Avatar\" + Stack[i].Name + ".png");
                Icon.Width = 24;
                Icon.Height = 24;
                Icon.Stretch = Stretch.Uniform;
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Stack[i].Name)));

                Stack[i].Content = Zone;
            }
            //Home.Click += Home_Click;
            //Talk.Click += Talk_Click;
            //About.Click += About_Click;
            Export.Click += Export_Click;
            Save.Click += Save_Click;
            Load.Click += Load_Click;
            //Update.Click += Update_Click;
            //Exit.Click += Exit_Click;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadSet();

        }

        private void LoadSet(string filePath = "")
        {
            string path = null;
            if (string.IsNullOrEmpty(filePath))
            {
                OpenFileDialog choofdlog = new OpenFileDialog();
                choofdlog.Filter = "OV.Creation Card|*.occ|OV.Creation Set Card|*.ocs";
                choofdlog.FilterIndex = 1;
                choofdlog.Multiselect = false;

                if (choofdlog.ShowDialog() == true)
                {
                    path = choofdlog.FileName;
                }
            }
            else
            {
                path = filePath;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            //var settings = new ObjectCustomerSettings();
            //Current = JsonConvert.DeserializeObject<YGOSet>(File.ReadAllText(path)).Cards[0];

            if (path.IsEndWith(".ocs"))
            {

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new PrivateSetterContractResolver(),
                };
                Set = JsonConvert.DeserializeObject<YGOSet>(File.ReadAllText(path), settings);
            }
            else
            {
                Set.Cards.Clear();
                Set.Cards.Add(JsonConvert.DeserializeObject<YGOCard>(File.ReadAllText(path)));
            }

            inChangeSetCard = true;
            SetEdit.Text = Set.Name;
            cardList.ItemsSource = Set.Cards;
            currentIndex = 0;
            inChangeSetCard = false;

            RefreshArtwork();
            RenderCard.Load(Current);
            RefreshControl();
        }

        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            var currentError = e.ErrorContext.Error.Message;
            MessageBox.Show(currentError);
            e.ErrorContext.Handled = true;
        }

        private void MainContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string fname = Application.Current.Properties["ArbitraryArgName"].ToString();
                //RenderCard.Load(fname);
                LoadSet(fname);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSet();
        }

        private void SaveSet()
        {
            string text;
            //MessageBox.Show(xml);


            SaveFileDialog dlg = new SaveFileDialog();
            if (string.IsNullOrWhiteSpace(Set.Name))
                dlg.FileName = "Set Name";
            else
                dlg.FileName = Set.Name.Replace(":", " -"); // Default file name
            //dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "OV.Creation Card|*.occ|OV.Creation Set Card|*.ocs"; // Filter files by extension 

            // Show save file dialog box

            // Process save file dialog box results 
            if (dlg.ShowDialog() == true)
            {
                // Save document 
                string filename = dlg.FileName;
                if (filename.IsEndWith(".ocs"))
                {
                    text = JsonConvert.SerializeObject(Set, Formatting.Indented);
                }
                else
                {
                    text = JsonConvert.SerializeObject(Current, Formatting.Indented);

                }
                File.WriteAllText(filename, text);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            RenderCard.Export();
        }

        private void LoadControl()
        {
            LoadRarityControl();
            LoadCirculationControl();
            LoadTypeControl();
            LoadFrameControl();
            LoadAttributeControl();
            LoadMiddleControl();
            LoadScaleControl();
            LoadAbilityControl();
            //SetColorControl();
        }

        private void LoadAbilityControl()
        {
            string[] Ability_String = typeof(ABILITY).GetList().ToArray();

            for (int i = 1; i <= Ability_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 90;

                AbilityCanvas.Children.Add(Button);

                Canvas.SetTop(Button, 36 + ((i - 1) / 4) * 33);
                Canvas.SetLeft(Button, 22 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Template\Ability\" + Ability_String[i - 1] + ".png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Ability_String[i - 1])));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                Button.Content = Zone;
                Button.Tag = Ability_String[i - 1];
                Button.Click += Button_Ability;
            }

            Button Reset = new Button();
            Reset.IsEnabled = false;
            Reset.Width = 90;
            Reset.Name = "Ability_Reset";
            Canvas.SetTop(Reset, 108);
            Canvas.SetLeft(Reset, 272);
            AbilityCanvas.Children.Add(Reset);
            //Reset.Click += Button_Reset;
            {
                StackPanel Zone = new StackPanel();
                Zone.Orientation = Orientation.Horizontal;
                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Other\Reset.png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run("Reset")));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                Reset.Content = Zone;
            }
        }

        private void Button_Ability(object sender, RoutedEventArgs e)
        {
            ABILITY ability = (sender as Button).Tag.ToString().ToEnum<ABILITY>();
            if (Current.Abilities.Contains(ability))
            {
                Current.SetAbility(ability, false);
            }
            else
            {
                Current.SetAbility(ability, true);
            }

            RenderCard.Render(Current);
            //RefreshAbilityControl();
            RefreshControl();
        }

        private void DEF_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetDEF(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);

            RefreshControl();
        }

        private void ATK_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetATK(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);

            RefreshControl();
        }

        private void LoadScaleControl()
        {
            Image InLeft = new Image();
            InLeft.Source = Database.GetImage(@"Template\Middle\ScaleLeft.png");
            InLeft.Width = 32;
            InLeft.Height = 32;
            Canvas.SetLeft(InLeft, 178);
            Canvas.SetTop(InLeft, 84);

            Canvas.SetTop(ScaleLeftBox, Canvas.GetTop(InLeft) + 40);
            Canvas.SetLeft(ScaleLeftBox, Canvas.GetLeft(InLeft) + 20);
            ScaleCanvas.Children.Add(InLeft);
            {
                Image InRight = new Image();
                InRight.Source = Database.GetImage(@"Template\Middle\ScaleRight.png");
                InRight.Width = 32;
                InRight.Height = 32;
                Canvas.SetLeft(InRight, 322);
                Canvas.SetTop(InRight, Canvas.GetTop(InLeft));
                ScaleCanvas.Children.Add(InRight);

                Canvas.SetTop(ScaleRightBox, Canvas.GetTop(InRight) + 40);
                Canvas.SetLeft(ScaleRightBox, Canvas.GetLeft(InRight) - 30);
            }

            Button Button = new Button();

            TextBlock Text = new TextBlock();
            Text.Text = "? - Change - ?";
            Text.TextAlignment = TextAlignment.Center;
            Text.FontSize = 14;
            //Button.Name = "Pendulum_0";
            Button.Click += Button_Scale;
            Button.Width = 100;
            Button.Content = Text;
            Button.Tag = "?";
            //Button.Style = (Style)FindResource("Windows8Button");
            ScaleCanvas.Children.Add(Button);
            Canvas.SetTop(Button, 78);
            Canvas.SetLeft(Button, 216);

            int left = 20, top = 30; //110-36
            for (int i = 1; i <= 12; i++)
            {
                Button ButtonX = new Button();
                ButtonX.Width = 50;
                ScaleCanvas.Children.Add(ButtonX);

                Canvas.SetTop(ButtonX, top + 8 + 36 * ((i - 1) / 3));
                Canvas.SetLeft(ButtonX, left + ((i - 1) % 3) * (ButtonX.Width + 4) - 14);

                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Template\Pendulum.png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                //ButtonX.Name = "Pendulum_" + i.ToString();
                ButtonX.Content = Zone;
                ButtonX.Tag = i;
                ButtonX.Click += Button_Scale;
            }
        }

        private void Button_Scale(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int value;
            Current.SetScaleLeft(int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            Current.SetScaleRight(int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            RenderCard.Render(Current);
            RefreshControl();
        }

        private void LoadMiddleControl()
        {

            TabControl Tab = new TabControl();
            Tab.Name = "Middle_Tab";
            //Tab.TabStripPlacement = Dock.Right;

            Tab.Width = 380;
            Tab.Height = 195;

            MiddleCanvas.Children.Add(Tab);
            Canvas.SetTop(Tab, 8);

            TabItem Level = new TabItem();
            Level.Header = "Level";
            Level.Content = new Canvas();
            Tab.Items.Add(Level);

            TabItem Rank = new TabItem();
            Rank.Header = "Rank";
            Rank.Content = new Canvas();
            Tab.Items.Add(Rank);

            TabItem Property = new TabItem();
            Property.Header = "Property";
            Property.Content = new Canvas();
            Tab.Items.Add(Property);

            MiddleReload(Level.Content as Canvas, "Level");
            MiddleReload(Rank.Content as Canvas, "Rank");
            MiddleReload(Property.Content as Canvas, "Property");
            //Tab.Resources.Add(typeof(TabItem), this.FindResource("RotatedTab") as Style);
        }

        private void MiddleReload(Canvas MiddleCanvas, string type)
        {
            MiddleCanvas.Children.Clear();
            if (type != "Property")
            {
                int left = 90, top = 30;
                for (int i = 1; i <= 12; i++)
                {
                    Button Button = new Button();
                    Button.Width = 50;
                    MiddleCanvas.Children.Add(Button);


                    if (i <= 4)
                    {
                        Canvas.SetTop(Button, top);
                        Canvas.SetLeft(Button, left + ((i - 1) % 4) * (Button.Width + 4) - 14);
                    }
                    else if (i <= 8)
                    {
                        Canvas.SetTop(Button, top + 36);
                        Canvas.SetLeft(Button, left + ((i - 1) % 4) * (Button.Width + 4) - 14);
                    }
                    else
                    {
                        Canvas.SetTop(Button, top + 72);
                        Canvas.SetLeft(Button, left + ((i - 1) % 4) * (Button.Width + 4) - 14);
                    }


                    StackPanel Zone = new StackPanel();
                    Zone.Orientation = Orientation.Horizontal;
                    Image Icon = new Image();
                    Icon.Source = Database.GetImage(@"Template\Middle\" + type + ".png");
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Button.Tag = type.Replace(' ', '_') + "_" + i.ToString();
                    Button.Content = Zone;


                    Button.Click += Button_LR;
                }
                /*
                Button Reset = new Button();
                Reset.Width = 80;
                Reset.Name = "Middle_" + type + "_Reset";
                Canvas.SetTop(Reset, 6);
                Canvas.SetLeft(Reset, 264);
                MiddleCanvas.Children.Add(Reset);
                //Reset.Click += Button_Reset;
                {
                    StackPanel Zone = new StackPanel();
                    Zone.Orientation = Orientation.Horizontal;
                    Image Icon = new Image();
                    Icon.Source = Database.GetImage(@"Other\Reset.png");
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("Reset")));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Reset.Content = Zone;
                }  */
            }
            else
            {
                string[] Magic = new string[] { "SPELL", "TRAP" };
                int count = 1;
                while (count <= Magic.Length)
                {
                    Image IconMonster = new Image();
                    IconMonster.Source = Database.GetImage(@"Template\Attribute\" + Magic[count - 1] + ".png");
                    IconMonster.Height = 20;
                    Canvas.SetTop(IconMonster, 8 + (count - 1) * 90);
                    Canvas.SetLeft(IconMonster, 10);
                    MiddleCanvas.Children.Add(IconMonster);
                    TextBlock TextMonster = new TextBlock(new Run(Magic[count - 1]));
                    Canvas.SetTop(TextMonster, 8 + (count - 1) * 90);
                    Canvas.SetLeft(TextMonster, 37);
                    TextMonster.FontSize = 13;
                    MiddleCanvas.Children.Add(TextMonster);

                    string[] Property_String;
                    if (Magic[count - 1] == "SPELL")
                    {
                        Property_String = new string[]
                            {
                                "Normal", "Quick-Play",
                                "Equip", "Continuous", "Field",
                                "Ritual",
                            };
                    }
                    else
                    {
                        Property_String = new string[]
                            {
                                "Normal",
                                "Continuous", "Counter",
                            };
                    }

                    for (int i = 1; i <= Property_String.Length; i++)
                    {
                        Button Button = new Button();
                        MiddleCanvas.Children.Add(Button);

                        if (Magic[count - 1] == "SPELL")
                            Button.Tag = "Spell_" + Property_String[i - 1].Remove("-");
                        else if (Magic[count - 1] == "TRAP")
                            Button.Tag = "Trap_" + Property_String[i - 1].Remove("-");

                        Button.Width = 106;
                        Button.Click += Button_Property;

                        Canvas.SetTop(Button, 34 + ((i - 1) / 3) * 33 + (count - 1) * 90);
                        Canvas.SetLeft(Button, 60 + ((i - 1) % 3) * (Button.Width + 4) - 32);

                        StackPanel Zone = new StackPanel();

                        Zone.Orientation = Orientation.Horizontal;
                        Button.Content = Zone;
                        Image Icon = new Image();
                        Icon.Source = Database.GetImage(@"Template\Middle\" + Property_String[i - 1].Remove("-") + ".png");
                        Icon.Width = 20;
                        Icon.Height = 20;
                        Icon.Stretch = Stretch.Uniform;
                        Icon.Margin = new Thickness(0, 0, 6, 0);
                        Zone.Children.Add(Icon);
                        Zone.Children.Add(new TextBlock(new Run(Property_String[i - 1])));
                    }
                    count++;
                }
            }
        }

        private void Button_Property(object sender, RoutedEventArgs e)
        {
            string Frame = (sender as Button).Tag.ToString().Split('_')[0];
            string Property = (sender as Button).Tag.ToString().Split('_')[1];

            Current.SetProperty(Frame == "Spell" ? FRAME.Spell : FRAME.Trap, Property.ToEnum<PROPERTY>());
            RenderCard.Render(Current);
            //RefreshMiddleControl();
            RefreshControl();
        }

        private void Button_LR(object sender, RoutedEventArgs e)
        {
            string type = (sender as Button).Tag.ToString().Split('_')[0];
            int number = Int32.Parse((sender as Button).Tag.ToString().Split('_')[1]);

            if (type == "Level")
            {
                Current.SetLevel(number);
                Current.SetRank(double.NaN, false);
            }
            else
            {

                Current.SetLevel(double.NaN, false);
                Current.SetRank(number);
            }

            RenderCard.Render(Current);
            //RefreshMiddleControl();
            RefreshControl();
        }

        private void LoadAttributeControl()
        {
            int left = 54;
            string[] Attribute_String = typeof(ATTRIBUTE).GetList().Where(o => o != ATTRIBUTE.UNKNOWN.ToString()).ToArray();
            for (int i = 1; i <= Attribute_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 70;
                AttributeCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 38);
                        Canvas.SetLeft(Button, left + ((i - 1) % 3) * (Button.Width + 4) - 14);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 38);
                        Canvas.SetLeft(Button, left + 3 * (Button.Width + 4) - 14);
                    }
                    else if (i >= 8)
                    {
                        Canvas.SetTop(Button, 120);
                        Canvas.SetLeft(Button, left + 68 + (i - 8) * (Button.Width + 16) - 14);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 74);
                        Canvas.SetLeft(Button, left + ((i - 3) % 4) * (Button.Width + 4) - 14);
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Template\Attribute\" + Attribute_String[i - 1] + ".png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Attribute_String[i - 1])));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                Button.Name = Attribute_String[i - 1];
                Button.Content = Zone;
                Button.Tag = Attribute_String[i - 1].ToEnum<ATTRIBUTE>();

                Button.Click += Button_Attribute;
            }
        }

        private void Button_Attribute(object sender, RoutedEventArgs e)
        {
            Current.SetAttribute((sender as Button).Tag.ToString().ToEnum<ATTRIBUTE>());
            RenderCard.Render(Current);
            //RefreshAttributeControl();
            RefreshControl();
        }

        private void LoadFrameControl()
        {
            List<string> frameList = typeof(FRAME).GetList().ToList();
            frameList.Insert(frameList.IndexOf("Spell"), "Pendulum");
            string[] Frame_String = frameList.ToArray();

            double sub = 25;

            for (int i = 1; i <= Frame_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 78;
                FrameCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 38);
                        Canvas.SetLeft(Button, ((i - 1) % 3) * (Button.Width + 4) + sub);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 38);
                        Canvas.SetLeft(Button, 3 * (Button.Width + 4) + sub);
                    }
                    else if (i == 8) //Pendulum
                    {
                        Button.Width += 32;
                        Canvas.SetTop(Button, 120);
                        Canvas.SetLeft(Button, (i - 8) * (Button.Width + 16) + sub);
                    }
                    else if (i >= 9)
                    {
                        Canvas.SetTop(Button, 120);
                        Canvas.SetLeft(Button, 82 + (i - 8) * (Button.Width + 4) + sub);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 74);
                        Canvas.SetLeft(Button, ((i - 3) % 4) * (Button.Width + 4) + sub);
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                if (i == 8)
                    Icon.Source = Database.GetImage(@"Template\Pendulum.png");
                else
                    Icon.Source = Database.GetImage(@"Template\Frame\" + Frame_String[i - 1] + ".png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Frame_String[i - 1])));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                Button.Tag = Frame_String[i - 1];
                Button.Content = Zone;

                Button.Click += Button_Frame;
            }
        }

        private void Button_Frame(object sender, RoutedEventArgs e)
        {
            string frameName = (sender as Button).Tag.ToString();
            if (frameName == "Pendulum")
            {
                Current.SetPendulum(!Current.IsPendulum);
            }
            else
            {
                Current.SetFrame(frameName.ToEnum<FRAME>());
            }
            RenderCard.Render(Current);
            //RefreshFrameControl();
            RefreshControl();
        }

        private void LoadTypeControl()
        {


            string[] Type_String = typeof(TYPE).GetList().Where(o => o != TYPE.NONE.ToString()).ToArray();

            for (int i = 1; i <= Type_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 90;

                TypeCanvas.Children.Add(Button);
                {
                    if (i >= Type_String.Length - 4)
                    {
                        Button.Width += 18;
                        if (i >= Type_String.Length - 1)
                        {
                            Canvas.SetTop(Button, 252 + ((i - (Type_String.Length - 1)) / 2) * 33);
                            Canvas.SetLeft(Button, 35 + ((i - (Type_String.Length - 1)) % 2) * (Button.Width + 4) + 44);
                        }
                        else
                        {
                            Canvas.SetTop(Button, 216 + ((i - (Type_String.Length - 4)) / 3) * 33);
                            Canvas.SetLeft(Button, 35 + ((i - (Type_String.Length - 4)) % 3) * (Button.Width + 4) - 10);
                        }
                    }
                    else
                    {
                        Canvas.SetTop(Button, 34 + ((i - 1) / 4) * 33);
                        Canvas.SetLeft(Button, 35 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                        if (i >= Type_String.Length - 7)
                        {

                            Canvas.SetTop(Button, Canvas.GetTop(Button) + 2);
                            Canvas.SetLeft(Button, Canvas.GetLeft(Button) + 44);
                        }
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Database.GetImage(@"Template\Type\" + Type_String[i - 1] + ".png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                string type = Type_String[i - 1];
                type = type.Replace("WingedBeast", "Winged Beast");
                type = type.Replace("BeastWarrior", "Beast-Warrior");
                type = type.Replace("SeaSerpent", "Sea Serpent");
                type = type.Replace("DivineBeast", "Divine-Beast");
                type = type.Replace("CreatorGod", "Creator God");
                Zone.Children.Add(new TextBlock(new Run(type)));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                Button.Content = Zone;
                Button.Tag = Type_String[i - 1];
                Button.Click += Button_Type;
            }
        }

        private void LoadCirculationControl()
        {
            EditionCombo.Items.Add("Unlimited Edition");
            EditionCombo.Items.Add("1st Edition");
            EditionCombo.Items.Add("Limited Edition");
            EditionCombo.Items.Add("Duel Terminal");

            StickerCombo.Items.Add("None");
            StickerCombo.Items.Add("Gold");
            StickerCombo.Items.Add("Silver");
            StickerCombo.Items.Add("Promo Silver");
            StickerCombo.Items.Add("Promo Gold");

        }

        private void LoadRarityControl()
        {
            string[] Rarity = typeof(RARITY).GetList().ToArray();

            for (int i = 0; i < Rarity.Length; i++)
            {
                Button Button = new Button();
                Button.Content = Rarity[i].AddSpaceBetweenCapital();
                Button.Width = 85;
                if (i >= 8)
                    Button.Width += 14;
                Button.Height = 30;
                Canvas.SetTop(Button, 36 + (i / 4) * (Button.Height + 10));

                int Left = 0;
                if (i >= 8)
                    Left = 32;
                else if (i >= 4)
                    Left = 0;
                Canvas.SetLeft(Button, Left + (i % 4) * (Button.Width + 10));
                Button.Click += Button_Rarity;
                Button.Tag = Rarity[i];
                RarityCanvas.Children.Add(Button);
            }
        }

        private void Button_Type(object sender, RoutedEventArgs e)
        {
            Current.SetType((sender as Button).Tag.ToString().ToEnum<TYPE>());
            RenderCard.Render(Current);
            //RefreshTypeControl();
            RefreshControl();
        }

        private void Sticker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.SetSticker(Text.Remove(" ").ToEnum<STICKER>());
            RenderCard.Render(Current);
        }

        private void SetNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            Current.SetSet((sender as TextBox).Text);
            RenderCard.Render(Current);
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            int value;
            int.TryParse((sender as TextBox).Text, out value);
            Current.SetNumber(value);


            RenderCard.Render(Current);
            //RefreshAttributeControl();
            RefreshControl();
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            TextBox CardNumber = CirculationCanvas.FindChildren<TextBox>("CardNumberEdit");
            Random t = new Random();
            CardNumber.Text = t.Next(0, 99999999 + 1).ToString().PadLeft(8, '0');
        }

        private void Edition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.SetEdition(Text.Remove(" ").Replace("1st", "First").ToEnum<EDITION>());
            RenderCard.Render(Current);
        }

        private string RenderText(string text)
        {
            string futureText = text ?? "";
            futureText = futureText.Replace("{.}", "●");
            futureText = futureText.Replace("{a}", "α");
            futureText = futureText.Replace("{b}", "β");
            futureText = futureText.Replace("{o}", "Ω");

            return futureText;
        }

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            string futureText = (sender as TextBox).Text;
            Current.SetName(RenderText(futureText));

            RefreshArtwork();
            RefreshControl();


        }
        /*
        private void Symbol_Click(object sender, RoutedEventArgs e)
        {
            TextBox Edit = (NameExpander.Header as Canvas).FindChildren<TextBox>("NameEdit");

            Edit.Text = Edit.Text.Insert(Edit.CaretIndex, (sender as Button).Content.ToString());
        }
        */

        private void Button_Rarity(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //ChangeRarity(Button.Content.ToString().Replace('_', ' '));
            Current.SetRarity(button.Tag.ToString().ToEnum<RARITY>());
            RenderCard.Render(Current);
            RefreshRarityControl();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Expander Ex = (sender as Border).Child as Expander;
            if (Ex == null)
                return;
            if (Ex.IsExpanded)
                Ex.IsExpanded = false;
            else
                Ex.IsExpanded = true;
        }


        private void ScaleLeft_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetScaleLeft(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);
            RefreshControl();
        }

        private void ScaleRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetScaleRight(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);
            RefreshControl();
        }


        private void ApplyDocument_Click(object sender, RoutedEventArgs e)
        {
            if (inRefreshControl) { return; }
            Current.SetDescription(RenderText(DescriptionBox.GetText()));
            Current.SetPendulumEffect(RenderText(PendulumBox.GetText()));
            RefreshControl();
            RenderCard.Render(Current);
        }

        private void cardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inChangeSetCard) { return; }
            currentIndex = cardList.SelectedIndex;
            //MessageBox.Show(currentIndex.ToString());
            if (currentIndex != -1)
            {
                ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
            }
            RefreshArtwork();
            RefreshControl();


        }

        private void cardList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (inChangeSetCard) { return; }
            if (e.Key == Key.Delete)
            {
                if (Set.Cards.Count() <= 1)
                {
                    return;
                }
                int previewIndex = currentIndex;
                Set.Cards.Remove(cardList.SelectedItem as YGOCard);
                cardList.ItemsSource = Set.Cards;

                currentIndex = previewIndex - 1;

                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }


                ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();

                RefreshArtwork();
                RefreshControl();
            }
        }

        private void InsertNewCard_Click(object sender, RoutedEventArgs e)
        {
            if (inChangeSetCard) { return; }

            //MessageBox.Show(Set.Cards[0].GetHashCode().ToString());
            if (Set.Cards.Where(o => o.Equals(YGOCard.Default)).Count() >= 3)
            {

            }
            else
            {

                Set.Cards.Add(YGOCard.Default);
                cardList.ItemsSource = Set.Cards;

                //currentIndex = cardList;

                if (currentIndex != -1)
                {
                    ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
                }
                RefreshArtwork();
                RefreshControl();
            }
        }



        private void ArtworkBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image File (*.png, *.jpg, *.bmp)|*.png;*.jpg;*.bmp";
            if (openDialog.ShowDialog() == true)
            {
                ArtworkPath.Text = openDialog.FileName;
            }
        }

        private void ArtworkPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            string filePath = (sender as TextBox).Text;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                //filePath = Database.GetImage(@"Template\NoneImage.png");
                Current.ArtworkByte = Database.GetImage(@"Template\NoneImage.png").GetImageArray();
                ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
                RefreshArtwork();
            }
            else if (File.Exists(filePath))
            {
                Current.ArtworkByte = Images.GetImageByte(filePath);
                ControlArtwork.Source = Images.GetImage(filePath);
                RefreshArtwork();
            }
            else
            {
                MessageBox.Show("Error #1");
            }
        }

        private void CreatorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (inRefreshControl) { return; }
            bool isChecked = (sender as CheckBox).IsChecked ?? false;
            Current.SetCreator(isChecked ? CREATOR.KazukiTakahashi : CREATOR.NONE);
            RenderCard.Render(Current);
            RefreshControl();
        }

        private void resizeArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(artworkPaddingHeight.Text, out value))
            {
                RenderCard.ResizeArtwork(value, true);
            }
        }

        private void previewResizeArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(artworkPaddingHeight.Text, out value))
            {
                ControlArtwork.Source = RenderCard.ResizeArtwork(value, false);
            }

        }


        

        private void NameEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            NameEdit.CaretIndex = NameEdit.Text.Length;
        }

        private void SetEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (inChangeSetCard) { return; }
            Set.SetName(SetEdit.Text);
        }

        private void AssociateExtension(bool isAssociate)
        {
            CustomFileExtension cardExtension = new CustomFileExtension();
            cardExtension.ApplicationName = "OV.Creation.exe";
            cardExtension.Description = "OV.Creation Card";
            cardExtension.EmbeddedIcon = false;
            cardExtension.Extension = ".occ";
            cardExtension.Handler = "OV.Creation.Card";
            cardExtension.IconName = @"Resources\OV.Card.ico";
            cardExtension.IconPosition = 0;
            cardExtension.OpenText = "Open with OV.Creation";

            CustomFileExtension setExtension = new CustomFileExtension();
            setExtension.ApplicationName = "OV.Creation.exe";
            setExtension.EmbeddedIcon = false;
            setExtension.IconPosition = 0;
            setExtension.OpenText = "Open with OV.Creation";
            setExtension.Description = "OV.Creation Set Card";
            setExtension.Extension = ".ocs";
            setExtension.Handler = "OV.Creation.Set";
            setExtension.IconName = @"Resources\OV.Set.ico";

            if (isAssociate)
            {
                cardExtension.RegisterFileType();
                setExtension.RegisterFileType();
            }
            else
            {

                cardExtension.RemoveFileType();
                setExtension.RemoveFileType();
            }

        }

        private void Associate_Click(object sender, RoutedEventArgs e)
        {
            AssociateExtension(true);

            Properties.Settings.Default.Associated = true;
            RefreshSetting();
        }

        private void Unassociate_Click(object sender, RoutedEventArgs e)
        {
            AssociateExtension(false);


            Properties.Settings.Default.Associated = false;
            RefreshSetting();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new About());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mediafire.com/folder/vamylw814g9iq/OV_Pyramid");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
        
    }
}
