using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OV.Core;
using OV.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static OV.Tools.Animations;
using static OV.Tools.Utilities;

namespace OV.Pyramid
{
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }
            return (value as byte[]).GetBitmapImageFromByteArray();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YGOSet Set = new YGOSet();
        private int currentIndex = -1;
        private bool inRefreshControl = false;

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

        private CustomFileExtensionControl.CustomFileExtension DefaultExtension
        {
            get
            {
                CustomFileExtensionControl.CustomFileExtension extenstion = new CustomFileExtensionControl.CustomFileExtension();
                extenstion.ApplicationName = "OV.Creation.exe";
                extenstion.Description = "OV.Creation Card";
                extenstion.EmbeddedIcon = false;
                extenstion.Extension = ".ocj";
                extenstion.Handler = "OV.Land.Pyramid";
                extenstion.IconName = "OV.Creation.Icon.ico";
                extenstion.IconPosition = 0;
                extenstion.OpenText = "CardIZE with OV.Creation";

                return extenstion;
            }
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

        public MainWindow()
        {
            InitializeComponent();            
            FirstLoad();           
        }

        private void FirstLoad()
        {
            //Set default global Style for Paragraph: Margin = 0
            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));
            Resources.Add(typeof(Paragraph), style);


            Set.Cards.Add(YGOCard.Default);
            Set.Cards.Add(YGOCard.Default);
            RenderCard.SetDefaultArtwork(GetLocationPath() + @"\Template\NoneImage.png");

            //RenderCard.Render(Current);

            LoadControl();
            LoadButton();

            this.Loaded += new RoutedEventHandler(MainContainer_Loaded);
            //DefaultExtension.RegisterFileType();
            //DefaultExtension.RemoveFileType();

            cardList.ItemsSource = Set.Cards;
            cardList.SelectedIndex = 0;

            

        }

        private void RefreshArtwork()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Set.Cards);
            view.Refresh();
            if (Current == null) { return; }
            RenderCard.Render(Current);
        }

        private void RefreshControl()
        {
            if (currentIndex == -1) { return;  }
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
            RefreshTextControl();

            RefreshCirculationControl();

            ArtworkPath.Text = "";     
            inRefreshControl = false;
        }

        private void RefreshCirculationControl()
        {            
            foreach(object item in EditionCombo.Items)
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

        private void RefreshTextControl()
        {
            DocumentBox.Document.Blocks.Clear();
            if (DescriptionButton.IsEnabled == false)
            {
                DocumentBox.AppendText(Current.Description);
            }
            else
            {
                DocumentBox.AppendText(Current.PendulumEffect);
            }            
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
            } else if (Current.ATK >= 0)
            {
                value = Current.ATK.ToString();
            } else
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
                    if (button.Tag.ToString() == Current.Frame.ToString())
                    {
                        button.IsEnabled = false;
                    }
                    
                    TextBlock content = button.FindChildrens<TextBlock>().ToList()[0] as TextBlock;
                    Image icon = button.FindChildrens<Image>().ToList()[0] as Image;
                    if (Current.Abilities.Contains(button.Tag.ToString().ToEnum<ABILITY>()))
                    {
                        content.Text = "Un-" + button.Tag;
                        icon.Source = Images.GetImage(GetLocationPath() + @"\Template\Ability\"+ button.Tag +".png")
                            .ToBitmap().Grayscale().ToBitmapSource();
                    }
                    else
                    {
                        content.Text = button.Tag.ToString();
                        icon.Source = Images.GetImage(GetLocationPath() + @"\Template\Ability\" + button.Tag + ".png");
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
                            icon.Source = Images.GetImage(GetLocationPath() + @"\Template\PendulumGrey.png");
                        }
                        else
                        {
                            content.Text = "Pendulum";
                            icon.Source = Images.GetImage(GetLocationPath() + @"\Template\Pendulum.png");
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
                if (button.Tag != null )
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
            foreach (Button button in NameCanvas.FindChildrens<Button>())
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
                Icon.Source = Images.GetImage(GetLocationPath() + @"Avatar\" + Stack[i].Name + ".png");
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
                choofdlog.Filter = "OV.Creation Set Card|*.ocs|OV.Creation Card|*.occ";
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
                var contractResolver = new DefaultContractResolver();
                contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                };
                Set = JsonConvert.DeserializeObject<YGOSet>(File.ReadAllText(path), settings);

            }
            else
            {
                Set.Cards.Clear();
                Set.Cards.Add(JsonConvert.DeserializeObject<YGOCard>(File.ReadAllText(path)));
            }
            cardList.SelectionChanged -= cardList_SelectionChanged;
            cardList.ItemsSource = Set.Cards;
            currentIndex = 0;
            cardList.SelectionChanged += cardList_SelectionChanged;

            RefreshArtwork();
            RenderCard.Load(Current);
            RefreshControl();
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
            dlg.Filter = "OV.Creation Set Card|*.ocs|OV.Creation Card|*.occ"; // Filter files by extension 

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
            LoadNameControl();
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            Caption.Name = "AbilityCaption";
            Caption.Text = "Ability";
            Caption.FontSize = 14;
            Canvas.SetLeft(Caption, 4);
            Panel.Children.Add(Caption);
            AbilityExpander.Header = Panel;

            string[] Ability_String = typeof(ABILITY).GetList().ToArray();

            for (int i = 1; i <= Ability_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 90;

                AbilityCanvas.Children.Add(Button);

                Canvas.SetTop(Button, 6 + ((i - 1) / 4) * 33);
                Canvas.SetLeft(Button, 22 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Images.GetImage(GetLocationPath() + "Template/Ability/" + Ability_String[i - 1] + ".png");
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.Stretch = Stretch.Uniform;
                Icon.Margin = new Thickness(0, 0, 4, 0);
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Ability_String[i - 1])));
                Zone.Margin = new Thickness(0, 4, 0, 0);

                //Button.Name = Ability_String[i - 1].Replace(' ', '_').Replace("-", "0X0");
                Button.Content = Zone;
                Button.Tag = Ability_String[i - 1];
                Button.Click += Button_Ability;
            }

            Button Reset = new Button();
            Reset.IsEnabled = false;
            Reset.Width = 90;
            Reset.Name = "Ability_Reset";
            Canvas.SetTop(Reset, 39 + 39);
            Canvas.SetLeft(Reset, 272);
            AbilityCanvas.Children.Add(Reset);
            //Reset.Click += Button_Reset;
            {
                StackPanel Zone = new StackPanel();
                Zone.Orientation = Orientation.Horizontal;
                Image Icon = new Image();
                Icon.Source = Images.GetImage(GetLocationPath() + "Other/Reset.png");
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            Caption.Name = "ScaleCaption";
            Caption.Text = "Pendulum Scale";
            Caption.FontSize = 14;
            Canvas.SetLeft(Caption, 4);
            Panel.Children.Add(Caption);
            ScaleExpander.Header = Panel;

            Image InLeft = new Image();
            InLeft.Source = Images.GetImage(GetLocationPath() + @"\Template\Middle\ScaleLeft.png");
            InLeft.Width = 32;
            InLeft.Height = 32;
            Canvas.SetLeft(InLeft, 178);
            Canvas.SetTop(InLeft, 54);

            Canvas.SetTop(ScaleLeftBox, Canvas.GetTop(InLeft) + 40);
            Canvas.SetLeft(ScaleLeftBox, Canvas.GetLeft(InLeft) + 20);
            ScaleCanvas.Children.Add(InLeft);
            {
                Image InRight = new Image();
                InRight.Source = Images.GetImage(GetLocationPath() + @"\Template\Middle\ScaleRight.png");
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
            Canvas.SetTop(Button, 38);
            Canvas.SetLeft(Button, 216);

            int left = 20, top = 0; //110-36
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
                Icon.Source = Images.GetImage(GetLocationPath() + @"Template\Pendulum.png");
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
            Tab.TabStripPlacement = Dock.Right;

            Tab.Width = 380;
            Tab.Height = 160;

            MiddleCanvas.Children.Add(Tab);
            Canvas.SetTop(Tab, 8);
            Canvas.SetLeft(Tab, -14);

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

            //Style style = new Style(typeof(TabItem));
            //ContentPresenter x = new ContentPresenter();
            //x.Content = Tab.ContentTemplate;
            //x.LayoutTransform = new RotateTransform(270);
            //style.Setters.Add(new Setter(TabItem.HeaderTemplateProperty, new DataTemplate(x)));
            //Tab.Resources.Add(typeof(TabItem), style);
            Tab.Resources.Add(typeof(TabItem), this.FindResource("RotatedTab") as Style);

        }

        private void MiddleReload(Canvas MiddleCanvas, string type)
        {
            MiddleCanvas.Children.Clear();
            if (type != "Property")
            {
                int left = 46, top = 30;
                for (int i = 1; i <= 12; i++)
                {
                    Button Button = new Button();
                    Button.Width = 50;
                    MiddleCanvas.Children.Add(Button);

                    {
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
                    }
                    {
                        StackPanel Zone = new StackPanel();
                        Zone.Orientation = Orientation.Horizontal;
                        Image Icon = new Image();
                        Icon.Source = Images.GetImage(GetLocationPath() + @"Template\Middle\" + type + ".png");
                        Icon.Width = 20;
                        Icon.Height = 20;
                        Icon.Stretch = Stretch.Uniform;
                        Icon.Margin = new Thickness(0, 0, 4, 0);
                        Zone.Children.Add(Icon);
                        Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                        Zone.Margin = new Thickness(0, 4, 0, 0);

                        Button.Tag = type.Replace(' ', '_') + "_" + i.ToString();
                        Button.Content = Zone;
                    }
                    /*
                    if (MiddleCanvas.Tag != null)
                        if (Current.Middle == (type + i).ToString())
                            Button.IsEnabled = false;
                            */
                    Button.Click += Button_LR;

                    //LoadLevel(Caption.Text);
                }

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
                    Icon.Source = Images.GetImage(GetLocationPath() + "Template/Other/Reset.png");
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("Reset")));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Reset.Content = Zone;
                }
                /*
                if (Current.Middle == null)
                    Reset.IsEnabled = false;
                else
                    Reset.IsEnabled = true;
                    */
            }
            else
            {
                string[] Magic = new string[] { "SPELL", "TRAP" };
                int count = 1;
                while (count <= Magic.Length)
                {
                    Image IconMonster = new Image();
                    IconMonster.Source = Images.GetImage(GetLocationPath() + "Template/Attribute/" + Magic[count - 1] + ".png");
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
                        Canvas.SetLeft(Button, 44 + ((i - 1) % 3) * (Button.Width + 4) - 32);
                        //    Uti.SetCanvas(Button, 40, 22 + ((i - 1) % 3) * (Button.Width + 4));

                        StackPanel Zone = new StackPanel();

                        Zone.Orientation = Orientation.Horizontal;
                        Button.Content = Zone;
                        Image Icon = new Image();
                        Icon.Source = Images.GetImage(GetLocationPath() + "Template/Middle/" + Property_String[i - 1] + ".png");
                        Icon.Width = 20;
                        Icon.Height = 20;
                        Icon.Stretch = Stretch.Uniform;
                        Icon.Margin = new Thickness(0, 0, 6, 0);
                        Zone.Children.Add(Icon);
                        Zone.Children.Add(new TextBlock(new Run(Property_String[i - 1])));

                        /*
                        if (Current.Middle!= null)
                        {
                            if (Property_String[i - 1] == Current.Middle)
                                Button.IsEnabled = false;

                            if (Current.IsFrame("Spell") && Magic[count - 1] != "SPELL")
                                Button.IsEnabled = true;
                            if (Current.IsFrame("Trap") && Magic[count - 1] != "TRAP")
                                Button.IsEnabled = true;
                        }*/
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
                Current.SetRank(number);
                Current.SetLevel(double.NaN, false);
            }

            RenderCard.Render(Current);
            //RefreshMiddleControl();
            RefreshControl();
        }

        private void LoadAttributeControl()
        {
            
            int left = 44;
            string[] Attribute_String = typeof(ATTRIBUTE).GetList().Where(o => o != ATTRIBUTE.UNKNOWN.ToString()).ToArray();
            for (int i = 1; i <= Attribute_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 70;
                AttributeCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 8);
                        Canvas.SetLeft(Button, left + ((i - 1) % 3) * (Button.Width + 4) - 14);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 8);
                        Canvas.SetLeft(Button, left + 3 * (Button.Width + 4) - 14);
                    }
                    else if (i >= 8)
                    {
                        Canvas.SetTop(Button, 90);
                        Canvas.SetLeft(Button, left + 68 + (i - 8) * (Button.Width + 16) - 14);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 44);
                        Canvas.SetLeft(Button, left + ((i - 3) % 4) * (Button.Width + 4) - 14);
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Images.GetImage(GetLocationPath() + "/Template/Attribute/" + Attribute_String[i - 1] + ".png");
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            Caption.Name = "FrameCaption";
            Caption.Text = "Card Type";
            Caption.FontSize = 14;
            Canvas.SetLeft(Caption, 4);
            Panel.Children.Add(Caption);
            FrameExpander.Header = Panel;

            List<string> frameList = typeof(FRAME).GetList().ToList();
            frameList.Insert(frameList.IndexOf("Spell"), "Pendulum");
            string[] Frame_String = frameList.ToArray();

            double sub = 18;

            for (int i = 1; i <= Frame_String.Length; i++)
            {
                Button Button = new Button();
                Button.Width = 78;
                FrameCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 8);
                        Canvas.SetLeft(Button, ((i - 1) % 3) * (Button.Width + 4) + sub);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 8);
                        Canvas.SetLeft(Button, 3 * (Button.Width + 4) + sub);
                    }
                    else if (i == 8)
                    {
                        Button.Width += 32;
                        Canvas.SetTop(Button, 90);
                        Canvas.SetLeft(Button, (i - 8) * (Button.Width + 16) + sub);
                    }
                    else if (i >= 9)
                    {
                        Canvas.SetTop(Button, 90);
                        Canvas.SetLeft(Button, 82 + (i - 8) * (Button.Width + 4) + sub);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 44);
                        Canvas.SetLeft(Button, ((i - 3) % 4) * (Button.Width + 4) + sub);
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                if (i == 8)
                    Icon.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Pendulum.png");
                else
                    Icon.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Frame/" + Frame_String[i - 1] + ".png");
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

                /*
                if (Current.IsFrame(Frame_String[i - 1]))
                    Button.IsEnabled = false;
                else
                    Button.IsEnabled = true;
                    */
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            Caption.Name = "TypeCaption";
            Caption.Text = "Monster Type";
            Caption.FontSize = 14;
            Canvas.SetLeft(Caption, 4);
            Panel.Children.Add(Caption);
            TypeExpander.Header = Panel;

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
                            Canvas.SetTop(Button, 222 + ((i - (Type_String.Length - 1)) / 2) * 33);
                            Canvas.SetLeft(Button, 22 + ((i - (Type_String.Length - 1)) % 2) * (Button.Width + 4) + 44);
                        }
                        else
                        {
                            Canvas.SetTop(Button, 186 + ((i - (Type_String.Length - 4)) / 3) * 33);
                            Canvas.SetLeft(Button, 22 + ((i - (Type_String.Length - 4)) % 3) * (Button.Width + 4) - 10);
                        }
                    }
                    else
                    {
                        Canvas.SetTop(Button, 4 + ((i - 1) / 4) * 33);
                        Canvas.SetLeft(Button, 22 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                        if (i >= Type_String.Length - 7)
                        {
                            Canvas.SetLeft(Button, Canvas.GetLeft(Button) + 44);
                            Canvas.SetTop(Button, Canvas.GetTop(Button) + 2);
                        }
                    }
                }
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Horizontal;

                Image Icon = new Image();
                Icon.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Type/" + Type_String[i - 1] + ".png");
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

                //Button.Name = Type_String[i - 1].Replace(' ', '_').Replace("-", "0X0");
                Button.Content = Zone;

                Button.Tag = Type_String[i - 1];

                Button.Click += Button_Type;
            }
        }

        private void LoadCirculationControl()
        {

            //
            EditionCombo.Items.Add("Unlimited Edition");
            EditionCombo.Items.Add("1st Edition");
            EditionCombo.Items.Add("Limited Edition");
            EditionCombo.Items.Add("Duel Terminal");
            //EditionCombo.SelectedIndex = 0;

            StickerCombo.Items.Add("None");
            StickerCombo.Items.Add("Gold");
            StickerCombo.Items.Add("Silver");
            StickerCombo.Items.Add("Promo Silver");
            StickerCombo.Items.Add("Promo Gold");

            /*
            Three = new TextBlock(new Run("Creator"));
            Canvas.SetTop(Three, 104);
            Canvas.SetLeft(Three, 0);
            CirculationCanvas.Children.Add(Three);

            TextBox Creator = new TextBox();
            Creator.Name = "CreatorEdit";
            Creator.Width = 220;
            Creator.TextChanged += Creator_TextChanged;
            CirculationCanvas.Children.Add(Creator);
            Canvas.SetLeft(Creator, 90);
            Canvas.SetTop(Creator, 104);
            */

        }

        private void LoadNameControl()
        {
            string[] Rarity = typeof(RARITY).GetList().ToArray();
            /*new string[] {"Common", "Rare", "SuperRare", "UltraRare",
                    "SecretRare", "ParallelRare", "StarfoilRare", "MosaicRare",
                    "GoldRare", "GhostRare", "UltimateRare"}; */

            for (int i = 0; i < Rarity.Length; i++)
            {
                Button Button = new Button();
                //Button.Name = "Rarity_" + Rarity[i];
                Button.Content = Rarity[i].AddSpaceBetweenCapital();
                if (i == 0)
                    Button.IsEnabled = false;
                Button.Width = 70;
                if (i >= 8)
                    Button.Width += 14;
                Button.Height = 30;
                Canvas.SetTop(Button, 36 + (i / 4) * (Button.Height + 10));

                int Left = -10;
                if (i >= 8)
                    Left = 42;
                else if (i >= 4)
                    Left = 56;
                Canvas.SetLeft(Button, Left + (i % 4) * (Button.Width + 10));
                Button.Click += Button_Rarity;
                Button.Tag = Rarity[i];
                NameCanvas.Children.Add(Button);
            }

            string[] Symbol = { "●", "α", "β", "Ω" };

            for (int i = 0; i < Symbol.Length; i++)
            {
                Button button = new Button();
                NameCanvas.Children.Add(button);
                button.Content = Symbol[i];
                button.Click += Symbol_Click;
                Canvas.SetTop(button, 2);
                Canvas.SetLeft(button, 300 + i * 18);
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
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.SetSticker(Text.Remove(" ").ToEnum<STICKER>());
            RenderCard.Render(Current);
        }

        private void SetNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            Current.SetSet((sender as TextBox).Text);
            RenderCard.Render(Current);
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            int.TryParse((sender as TextBox).Text, out value);
            Current.SetNumber(value);
            RenderCard.Render(Current);
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            TextBox CardNumber = CirculationCanvas.FindChildren<TextBox>("CardNumberEdit");
            Random t = new Random();
            CardNumber.Text = t.Next(0, 99999999 + 1).ToString().PadLeft(8, '0');
        }

        private void Edition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.SetEdition(Text.Remove(" ").Replace("1st", "First").ToEnum<EDITION>());
            RenderCard.Render(Current);
        }

        private void Name_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NameExpander.IsExpanded = true;
        }

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            Current.SetName((sender as TextBox).Text); 
            RenderCard.Render(Current);
        }

        private void Symbol_Click(object sender, RoutedEventArgs e)
        {
            TextBox Edit = (NameExpander.Header as Canvas).FindChildren<TextBox>("NameEdit");

            Edit.Text = Edit.Text.Insert(Edit.CaretIndex, (sender as Button).Content.ToString());
        }

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

        private void DocumentExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += DocumentCanvas.Height - 40;
            SynchroAnimation(DocumentBorder, DocumentCanvas.Height, Border.HeightProperty);

            TextBlock Caption = (DocumentExpander.Header as Canvas).FindChildren<TextBlock>("DocumentCaption");
            SynchroAnimation(Caption, (DocumentBorder.Width - Caption.ActualWidth) / 2 - 32, Canvas.LeftProperty);
        }

        private void DocumentExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= DocumentCanvas.Height - 40;
            SynchroAnimation(DocumentBorder, 40, Border.HeightProperty);

            TextBlock Caption = (DocumentExpander.Header as Canvas).FindChildren<TextBlock>("DocumentCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void CirculationExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += CirculationCanvas.Height - 40;
            SynchroAnimation(CirculationBorder, CirculationCanvas.Height, Border.HeightProperty);


            SynchroAnimation(CirculationCaption,
                (CirculationBorder.Width - CirculationCaption.ActualWidth) / 2 - 32, Canvas.LeftProperty);

            Scroll.ScrollToEnd();
        }

        private void CirculationExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= CirculationCanvas.Height - 40;
            SynchroAnimation(CirculationBorder, 40, Border.HeightProperty);
            
            SynchroAnimation(CirculationCaption, 2, Canvas.LeftProperty);
        }

        private void NameExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 190 - 40;
            SynchroAnimation(NameBorder, 190, Border.HeightProperty);

            TextBlock Caption = (NameExpander.Header as Canvas).FindChildren<TextBlock>("NameCaption");
            SynchroAnimation(Caption, 148, Canvas.LeftProperty);

            TextBox Edit = (NameExpander.Header as Canvas).FindChildren<TextBox>("NameEdit");
            Edit.Focus();
            Edit.SelectAll();
            SynchroAnimation(Edit, 20, Canvas.TopProperty);
            SynchroAnimation(Edit, 2, Canvas.LeftProperty);
        }

        private void NameExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 190 - 40;
            SynchroAnimation(NameBorder, 40, Border.HeightProperty);

            TextBlock Caption = (NameExpander.Header as Canvas).FindChildren<TextBlock>("NameCaption");

            SynchroAnimation(Caption, 2, Canvas.LeftProperty);

            TextBox Edit = (NameExpander.Header as Canvas).FindChildren<TextBox>("NameEdit");
            Keyboard.ClearFocus();
            SynchroAnimation(Edit, -2, Canvas.TopProperty);
            SynchroAnimation(Edit, 50, Canvas.LeftProperty);
        }

        private void AttributeExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 160 - 40;
            //double width = 80;
            {
                SynchroAnimation(AttributeBorder, 160, Border.HeightProperty);
                //SynchroAnimation(AttributeBorder, 400 - width - 10, Border.WidthProperty);

                TextBlock Caption = (AttributeExpander.Header as Canvas).FindChildren<TextBlock>("AttributeCaption");
                SynchroAnimation(Caption, (AttributeBorder.Width - Caption.ActualWidth) / 2 - 34, Canvas.LeftProperty);
                //Grid.SetColumnSpan(AttributeBorder, 4);
            }
        }

        private void AttributeExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 160 - 40;
            {
                SynchroAnimation(AttributeBorder, 40, Border.HeightProperty);
                //SynchroAnimation(AttributeBorder, 195, Border.WidthProperty);

                TextBlock Caption = (AttributeExpander.Header as Canvas).FindChildren<TextBlock>("AttributeCaption");
                SynchroAnimation(Caption, 2, Canvas.LeftProperty);
            }
            //SynchroAnimation(MiddleBorder, 195, Border.WidthProperty);
        }

        private void MiddleExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 210 - 40;
            {
                SynchroAnimation(MiddleBorder, 40, Border.HeightProperty);
                //SynchroAnimation(MiddleBorder, 195, Border.WidthProperty);

                TextBlock Caption = (MiddleExpander.Header as Canvas).FindChildren<TextBlock>("LRCaption");
                SynchroAnimation(Caption, 2, Canvas.LeftProperty);
            }
            // SynchroAnimation(AttributeBorder, 195, Border.WidthProperty);
        }

        private void MiddleExpander_Expanded(object sender, RoutedEventArgs e)
        {
            TextBlock Caption = MiddleCaption;
            //MiddleReload(Caption.Text);
            Grid.Height += 210 - 40;
            //double width = 160;
            {
                SynchroAnimation(MiddleBorder, 210, Border.HeightProperty);
                //SynchroAnimation(MiddleBorder, 400 - width - 10, Border.WidthProperty);

                //SynchroAnimation(Caption, 64, Canvas.LeftProperty);
                SynchroAnimation(Caption, (MiddleBorder.Width - Caption.ActualWidth) / 2 - 32, Canvas.LeftProperty);
            }

            //AttributeExpander.IsExpanded = false;

            //SynchroAnimation(AttributeBorder, width + 2, Border.WidthProperty);
        }

        private void FrameExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 162 - 40;

            SynchroAnimation(FrameBorder, 162, Border.HeightProperty);

            TextBlock Caption = (FrameExpander.Header as Canvas).FindChildren<TextBlock>("FrameCaption");
            SynchroAnimation(Caption, 134, Canvas.LeftProperty);

            /*
            Image Frame = Uti.FindChildren<Image>(Card, "Frame");
            for (int i = 0; i < Frame_String.Length; i++)
            {
                Button Z = Uti.FindChildren<Button>(FrameCanvas, Frame_String[i]);
                if (Z.Name == Current.Frame as string)
                    Z.IsEnabled = false;
                else
                    Z.IsEnabled = true;
            }*/

            //
            FrameCanvas.LayoutTransform = new ScaleTransform(1, 1);
            SynchroAnimation(FrameCanvas, 1, "(FrameworkElement.LayoutTransform).(ScaleTransform.ScaleY)");
            //ScaleTransform test = new ScaleTransform();
        }

        private void FrameExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 162 - 40;
            SynchroAnimation(FrameBorder, 40, Border.HeightProperty);

            TextBlock Caption = (FrameExpander.Header as Canvas).FindChildren<TextBlock>("FrameCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void TypeExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 294 - 40;
            //Scroll.ScrollToVerticalOffset(Grid.Height - Scroll.Height - (294 - 40));
            SynchroAnimation(TypeBorder, 294, Border.HeightProperty);

            TextBlock Caption = (TypeExpander.Header as Canvas).FindChildren<TextBlock>("TypeCaption");
            SynchroAnimation(Caption, 122, Canvas.LeftProperty);
        }

        private void TypeExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 294 - 40;
            SynchroAnimation(TypeBorder, 40, Border.HeightProperty);

            TextBlock Caption = (TypeExpander.Header as Canvas).FindChildren<TextBlock>("TypeCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void AbilityExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 152 - 40;
            SynchroAnimation(AbilityBorder, 40, Border.HeightProperty);

            TextBlock Caption = (AbilityExpander.Header as Canvas).FindChildren<TextBlock>("AbilityCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void AbilityExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 152 - 40;
            //Scroll.ScrollToVerticalOffset(Grid.Height - Scroll.Height);
            SynchroAnimation(AbilityBorder, 152, Border.HeightProperty);

            TextBlock Caption = (AbilityExpander.Header as Canvas).FindChildren<TextBlock>("AbilityCaption");
            SynchroAnimation(Caption, 142, Canvas.LeftProperty);
        }

        private void ScaleExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 190 - 40;
            SynchroAnimation(ScaleBorder, 190, Border.HeightProperty);

            TextBlock Caption = (ScaleExpander.Header as Canvas).FindChildren<TextBlock>("ScaleCaption");

            SynchroAnimation(Caption, (ScaleBorder.Width - Caption.ActualWidth) / 2 - 34, Canvas.LeftProperty);
            Scroll.ScrollToEnd();
        }

        private void ScaleExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 190 - 40;
            //ScaleBorder.MinHeight = 40;
            SynchroAnimation(ScaleBorder, 40, Border.HeightProperty);

            TextBlock Caption = (ScaleExpander.Header as Canvas).FindChildren<TextBlock>("ScaleCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
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
        
        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush Show = Brushes.LightBlue;

            foreach (Visual Visual in Grid.Children)
            {
                try
                {
                    Border Border = Visual as Border;
                    if (Border.Background == Show)
                        (Border.Child as Expander).IsExpanded = (sender as Button).Content.ToString().Contains("Expand");
                }
                catch
                {

                }
            }

            (sender as Button).Content = (sender as Button).Content.ToString().Contains("Expand") ? "Collapse - All" : "Expand - All";
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            DescriptionButton.IsEnabled = false;
            PendulumEffectButton.IsEnabled = true;
            /*
            DocumentBox.Document.Blocks.Clear();
            DocumentBox.AppendText(Current.Description);
            */
            RefreshControl();
        }

        private void ApplyDocument_Click(object sender, RoutedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextRange textRange = new TextRange(DocumentBox.Document.ContentStart, DocumentBox.Document.ContentEnd);

            string Text = @textRange.Text;
            Text = Text.ReplaceLastOccurrence(Environment.NewLine, "");

            if (DescriptionButton.IsEnabled == false) //Current is Description
            {
                Current.SetDescription(Text);
            }
            else
            {
                Current.SetPendulumEffect(Text);
                RefreshControl();
            }

            RenderCard.Render(Current);
        }

        private void cardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            currentIndex = cardList.SelectedIndex;
            if (currentIndex != -1)
            {
                ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
            }
            RefreshArtwork();
            RefreshControl();
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
                filePath = GetLocationPath() + @"\Template\NoneImage.png";                
            }
            if (File.Exists(filePath))
            {
                Current.ArtworkByte = Images.GetImageByte(filePath);
                ControlArtwork.Source = Images.GetImage(filePath);
                RefreshArtwork();
            }
        }

        private void PendulumEffectButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionButton.IsEnabled = true;
            PendulumEffectButton.IsEnabled = false;
            /*
            DocumentBox.Document.Blocks.Clear();
            DocumentBox.AppendText(Current.PendulumEffect);
            */
            RefreshControl();
        }

        private void CreatorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (inRefreshControl) { return; }
            bool isChecked = (sender as CheckBox).IsChecked ?? false;
            Current.SetCreator(isChecked ? CREATOR.KazukiTakahashi : CREATOR.NONE);            
            RenderCard.Render(Current);
            RefreshControl();
        }
    }

    public static class Masking
    {
        private static readonly DependencyPropertyKey _maskExpressionPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MaskExpression",
            typeof(Regex),
            typeof(Masking),
            new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the <see cref="Mask"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached("Mask",
            typeof(string),
            typeof(Masking),
            new FrameworkPropertyMetadata(OnMaskChanged));

        /// <summary>
        /// Identifies the <see cref="MaskExpression"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaskExpressionProperty = _maskExpressionPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the mask for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask is to be retrieved.
        /// </param>
        /// <returns>
        /// The mask, or <see langword="null"/> if no mask has been set.
        /// </returns>
        public static string GetMask(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MaskProperty) as string;
        }

        /// <summary>
        /// Sets the mask for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask is to be set.
        /// </param>
        /// <param name="mask">
        /// The mask to set, or <see langword="null"/> to remove any existing mask from <paramref name="textBox"/>.
        /// </param>
        public static void SetMask(TextBox textBox, string mask)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            textBox.SetValue(MaskProperty, mask);
        }

        /// <summary>
        /// Gets the mask expression for the <see cref="TextBox"/>.
        /// </summary>
        /// <remarks>
        /// This method can be used to retrieve the actual <see cref="Regex"/> instance created as a result of setting the mask on a <see cref="TextBox"/>.
        /// </remarks>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask expression is to be retrieved.
        /// </param>
        /// <returns>
        /// The mask expression as an instance of <see cref="Regex"/>, or <see langword="null"/> if no mask has been applied to <paramref name="textBox"/>.
        /// </returns>
        public static Regex GetMaskExpression(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MaskExpressionProperty) as Regex;
        }

        private static void SetMaskExpression(TextBox textBox, Regex regex)
        {
            textBox.SetValue(_maskExpressionPropertyKey, regex);
        }

        private static void OnMaskChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            var mask = e.NewValue as string;
            textBox.PreviewTextInput -= textBox_PreviewTextInput;
            textBox.PreviewKeyDown -= textBox_PreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, Pasting);

            if (mask == null)
            {
                textBox.ClearValue(MaskProperty);
                textBox.ClearValue(MaskExpressionProperty);
            }
            else
            {
                textBox.SetValue(MaskProperty, mask);
                SetMaskExpression(textBox, new Regex(mask, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace));
                textBox.PreviewTextInput += textBox_PreviewTextInput;
                textBox.PreviewKeyDown += textBox_PreviewKeyDown;
                DataObject.AddPastingHandler(textBox, Pasting);
            }
        }

        private static void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var maskExpression = GetMaskExpression(textBox);

            if (maskExpression == null)
            {
                return;
            }

            var proposedText = GetProposedText(textBox, e.Text);

            if (!maskExpression.IsMatch(proposedText))
            {
                e.Handled = true;
            }
        }

        private static void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var maskExpression = GetMaskExpression(textBox);

            if (maskExpression == null)
            {
                return;
            }

            //pressing space doesn't raise PreviewTextInput - no idea why, but we need to handle
            //explicitly here
            if (e.Key == Key.Space)
            {
                var proposedText = GetProposedText(textBox, " ");

                if (!maskExpression.IsMatch(proposedText))
                {
                    e.Handled = true;
                }
            }
        }

        private static void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            var maskExpression = GetMaskExpression(textBox);

            if (maskExpression == null)
            {
                return;
            }

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = e.DataObject.GetData(typeof(string)) as string;
                var proposedText = GetProposedText(textBox, pastedText);

                if (!maskExpression.IsMatch(proposedText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static string GetProposedText(TextBox textBox, string newText)
        {
            var text = textBox.Text;

            if (textBox.SelectionStart != -1)
            {
                text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }

            text = text.Insert(textBox.CaretIndex, newText);

            return text;
        }
    }
}