using CustomFileExtensionControl;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using static OV.Tools.Utilities;
using Forms = System.Windows.Forms;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal YgoSet CurrentSet { get; private set; }        

        private bool inRefreshControl;
        private bool inChangeSetCard;

        private static string DatabasePath;
        private ByteDatabase Database;
        private string currentFilePath;

        int currentNewIndex = 1;
        const string CARD_EXTENSION = ".occ";
        const string SET_EXTENSION = ".ocs";

        private string LastDataFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(currentFilePath) == false)
                { return Path.GetDirectoryName(currentFilePath); }
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        public static class Command
        {
            public static RoutedCommand RemoveCurrentCard;
        }
        

        private bool IsSaved
        {
            get
            {                
                if (PreviousSet != null)
                {
                    return PreviousSet == CurrentSet;
                }
                return false;
            }
        }

        internal YgoCard CurrentCard
        {
            get
            {
                if (CardList.SelectedIndex < 0) { return null; }
                return CurrentSet.Cards[CardList.SelectedIndex];
            }
            private set
            {
                if (CardList.SelectedIndex < 0) { return; }
                CurrentSet.Cards[CardList.SelectedIndex] = value;
            }
        }
        
        private YgoSet PreviousSet;

        public MainWindow()
        {
            InitializeComponent();
            FirstLoad();
        }        

        private void FirstLoad()
        {
            DatabasePath = GetLocationPath() + @"\Resources\Data.ld";
            Database = new ByteDatabase(DatabasePath);

            //SaveDatabase();

            CurrentSet = new YgoSet();
            CurrentSet.Cards.Add(YgoCard.Default);
            PreviousSet = CurrentSet.Clone();

            inRefreshControl = false;
            
            if (File.Exists("settings.json"))
            {
                LoadSettings();
            }
            else
            {
                MessageBox.Show("The file settings.json cannot be found!");
                GenerateSettings();
            }            
            
            YgoView.SetDefaultArtwork(Database.GetData(@"Template\NoneImage.png").Bytes);  

            LoadControl();
            LoadButton();
            RefreshSetting();
            this.Loaded += new RoutedEventHandler(MainContainer_Loaded);

            CardList.ItemsSource = CurrentSet.Cards;
            CardList.SelectedIndex = 0;

            DataObject.AddPastingHandler(PendulumBox, Document_OnPasting);
            DataObject.AddPastingHandler(DescriptionBox, Document_OnPasting);
        }

        private void Document_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            ApplyButton.PerformClick();
        }

        private void LoadSettings()
        {
            string data = File.ReadAllText("settings.json");
            JObject settings = JObject.Parse(JsonConvert.DeserializeObject(data).ToString());

            string exportPath = "";
            if (settings["Export"]["Path"] != null)
            {
                exportPath = settings["Export"]["Path"].ToString(0);
            }
            if (Directory.Exists(exportPath) == false)
            {
                exportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            exportPathTextBox.Text = exportPath;
        }

        private void GenerateSettings()
        {
            dynamic settings = new JObject();
            settings.Export = new JObject();
            settings["Export"]["Path"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText("settings.json",
                JsonConvert.SerializeObject(settings.ToString(), Formatting.Indented));
        }


        private void SaveDatabase()
        {
            Database.Generate(GetLocationPath() + @"\Resources2\");
        }

        private void RefreshArtwork()
        {
            inChangeSetCard = true;
            ICollectionView view = CollectionViewSource.GetDefaultView(CurrentSet.Cards);
            view.Refresh();
            inChangeSetCard = false;
            if (CurrentCard == null) { return; }

            YgoView.Render(CurrentCard);
        }

        private void RefreshControl()
        {
            if (CurrentCard == null) { return; }
            inRefreshControl = true;

            if (NameEdit.Text != CurrentCard.Name)
            {
                NameEdit.Text = CurrentCard.Name;
            }

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
            RefreshArtworkControl();
            RefreshOtherControls();
            

            inRefreshControl = false;
        }

        private void RefreshOtherControls()
        {
            DataText.Text = CurrentCard.GetData();

            Message.Text = string.IsNullOrEmpty(currentFilePath)
                ? ""
                : Path.GetFileName(currentFilePath);
            if (IsSaved == false)
            {
                Message.Text += " *";
            }
            RefreshExpanders();
        }

        private void RefreshExpanders()
        {
            List<Expander> expanders = new List<Expander>();
            expanders.Add(RarityBorder.Child as Expander);
            expanders.Add(AttributeBorder.Child as Expander);
            expanders.Add(MiddleBorder.Child as Expander);
            expanders.Add(FrameBorder.Child as Expander);
            expanders.Add(TypeBorder.Child as Expander);
            expanders.Add(AbilityBorder.Child as Expander);
            expanders.Add(ValueBorder.Child as Expander);
            expanders.Add(ScaleBorder.Child as Expander);
            expanders.Add(CirculationBorder.Child as Expander);

            ExpandAllButton.IsEnabled = expanders.Any(o => o.IsExpanded == false);
            CollapseAllButton.IsEnabled = expanders.Any(o => o.IsExpanded == true);
        }

        private void RefreshArtworkControl()
        {
            ArtworkPath.Text = "";
            ArtworkPaddingHeight.Text = YgoView.RecommendedHeight.ToString();
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
                if (item.ToString().Remove(" ") == CurrentCard.Edition.ToString())
                {
                    EditionCombo.SelectedItem = item;
                    break;
                }
            }

            foreach (object item in StickerCombo.Items)
            {
                if (item.ToString().Remove(" ") == CurrentCard.Sticker.ToString())
                {
                    StickerCombo.SelectedItem = item;
                    break;
                }
            }

            SetNumberEdit.Text = CurrentCard.Set.ToString();
            CardNumberEdit.Text = CurrentCard.Number > 0 ? CurrentCard.Number.ToString() : "";

            CreatorCheckBox.IsChecked = CurrentCard.Creator == CREATOR.NONE ? false : true;
        }

        private void RefreshPendulumControl()
        {
            PendulumBox.SetText(CurrentCard.PendulumEffect);
        }

        private void RefreshDescriptionControl()
        {
            DescriptionBox.SetText(CurrentCard.Description);
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
                    if (double.Equals(CurrentCard.ScaleLeft, CurrentCard.ScaleRight))
                    {
                        if (double.IsNaN(CurrentCard.ScaleLeft) && button.Tag.ToString() == "?")
                        {
                            button.IsEnabled = false;
                        }
                        else if (button.Tag.ToString() == CurrentCard.ScaleLeft.ToString())
                        {
                            button.IsEnabled = false;
                        }
                    }
                    if (CurrentCard.ScaleLeft >= 0 || double.IsNaN(CurrentCard.ScaleLeft))
                    {
                        ScaleLeftBox.Text = double.IsNaN(CurrentCard.ScaleLeft) ? "?" : CurrentCard.ScaleLeft.ToString();
                    }
                    if (CurrentCard.ScaleRight >= 0 || double.IsNaN(CurrentCard.ScaleRight))
                    {
                        ScaleRightBox.Text = double.IsNaN(CurrentCard.ScaleRight) ? "?" : CurrentCard.ScaleRight.ToString();
                    }
                }
            }
        }

        private void RefreshATKControl()
        {
            string value;
            if (double.IsNaN(CurrentCard.ATK))
            {
                value = "?";
            }
            else if (CurrentCard.ATK >= 0)
            {
                value = CurrentCard.ATK.ToString();
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
            if (double.IsNaN(CurrentCard.DEF))
            {
                value = "?";
            }
            else if (CurrentCard.DEF >= 0)
            {
                value = CurrentCard.DEF.ToString();
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
                if (button.Name.ToString() == "Ability_Reset")
                {
                    button.IsEnabled = CurrentCard.Abilities.Count() != 0;
                    continue;
                }
                if (button.IsEnabled == false)
                {
                    button.IsEnabled = true;
                }
                if (button.Tag != null)
                {
                    if (button.Tag.ToString() == "Effect")  //Effect Ability
                    {
                        if ((CurrentCard.Frame == FRAME.Effect)
                            || CurrentCard.Abilities.Count(o => o.IsEffectAbility()) > 0)
                        {
                            button.IsEnabled = false;
                        }
                    }
                    TextBlock content = button.FindChildrens<TextBlock>().ToList()[0] as TextBlock;
                    Image icon = button.FindChildrens<Image>().ToList()[0] as Image;
                    if (CurrentCard.Abilities.Contains(button.Tag.ToString().ToEnum<ABILITY>()))
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
                if (button.Tag != null && button.Tag.ToString() == CurrentCard.Type.ToString())
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
                    if (button.Tag.ToString() == CurrentCard.Frame.ToString())
                    {
                        button.IsEnabled = false;
                    }
                    if (button.Tag.ToString() == "Pendulum")
                    {
                        TextBlock content = button.FindChildrens<TextBlock>().ToList()[0] as TextBlock;
                        Image icon = button.FindChildrens<Image>().ToList()[0] as Image;
                        if (CurrentCard.IsPendulum)
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
            MiddleCanvas.FindChildrens<Button>().Where(o => o.IsEnabled == false)
                .ToList().ForEach(s => s.IsEnabled = true);

            MiddleCanvas.FindChildrens<Button>().Where(s => s.Tag != null && (s.Tag.ToString() == "Level_" + CurrentCard.Level.ToString()
                        || s.Tag.ToString() == "Rank_" + CurrentCard.Rank.ToString()
                        || s.Tag.ToString() == CurrentCard.Frame.ToString() + "_" + CurrentCard.Property.ToString()))
                .ToList().ForEach(s => s.IsEnabled = false);
            
            if (CurrentCard.IsFrame(FRAME.Xyz))
            {
                MiddleTab.SelectedIndex = 1; //RankTab
            }
            else if (CurrentCard.IsMagic())
            {
                MiddleTab.SelectedIndex = 2; //PropertyTab
            }
            else
            {
                MiddleTab.SelectedIndex = 0; //LevelTab
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
                if (button.Tag != null && button.Tag.ToString() == CurrentCard.Attribute.ToString())
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
                if (button.Tag != null && button.Tag.ToString() == CurrentCard.Rarity.ToString())
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void LoadButton()
        {
            Button[] Stack = new Button[] { Facebook, Mediafire, About, Save, Feedback, Open, Exit };
            for (int i = 0; i < Stack.Length; i++)
            {
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Avatar\" + Stack[i].Name + ".png"),
                    Width = 24,
                    Height = 24,
                    Stretch = Stretch.Uniform
                };
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Stack[i].Name)));

                Stack[i].Content = Zone;
            }            
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
                OpenFileDialog choofdlog = new OpenFileDialog()
                {
                    Filter = string.Format("OV.Creation Card/Set Card|*{0};*{1}",
                        CARD_EXTENSION, SET_EXTENSION),
                    Multiselect = false,
                    InitialDirectory = LastDataFolder,
                };
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
            
            if (path.IsExtension(".ocs"))
            {
                CurrentSet = YgoSet.LoadFrom(path);
                
            }
            else
            {
                CurrentSet.Cards.Clear();
                CurrentSet.Cards.Add(YgoCard.LoadFrom(path));
            }

            PreviousSet = CurrentSet.Clone();
            inChangeSetCard = true;

            SetEdit.Text = CurrentSet.Name;
            CardList.ItemsSource = CurrentSet.Cards;
            
            CardList.SelectedIndex = 0;
            currentFilePath = path;
            currentNewIndex = 0;

            inChangeSetCard = false;

            RefreshArtwork();
            YgoView.Load(CurrentCard);
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
                try
                {
                    currentFilePath = fname;
                    LoadSet(fname);
                }
                catch {
                    MessageBox.Show("Error #2");
                }
            }
        }

        private void QuickImageRender()
        {
            if (CurrentCard == null)
            {
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            //dlg.FileName = NameEdit.Text.Replace(":", " -"); // Default file name
            if (String.IsNullOrEmpty(dlg.FileName))
                dlg.FileName = CurrentCard.Name != null ? CurrentCard.Name.ToNonVietnamese() : "Card Name";
            //dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "JPEG Images|*.jpg|PNG Images|*.png|BMP Images|*.bmp"; // Filter files by extension 

            // Show save file dialog box

            // Process save file dialog box results 
            if (dlg.ShowDialog() == true)
            {                
                YgoView.SaveImageTo(dlg.FileName);
            }

        }

        private void SaveSet(string fileName = "")
        {
            string text;

            if (string.IsNullOrEmpty(fileName))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                if (string.IsNullOrWhiteSpace(CurrentSet.Name))
                    dlg.FileName = "Set Name";
                else
                    dlg.FileName = CurrentSet.Name.Replace(":", " -"); // Default file name
                dlg.Filter = string.Format("OV.Creation Card|*{0}|OV.Creation Set Card|*{1}",
                    CARD_EXTENSION, SET_EXTENSION);
                if (dlg.ShowDialog() == true)
                {
                    fileName = dlg.FileName;
                }
            }
            if (string.IsNullOrEmpty(fileName) == false)
            {
                if (fileName.IsExtension(".ocs"))
                {
                    text = JsonConvert.SerializeObject(CurrentSet, Formatting.Indented);
                }
                else
                {
                    text = JsonConvert.SerializeObject(CurrentCard, Formatting.Indented);

                }
                File.WriteAllText(fileName, text);
                PreviousSet = CurrentSet.Clone();
            }
            else
            {
                tabControl.SelectedIndex = 4;//ExportTab
            }
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
        }

        private void LoadAbilityControl()
        {
            string[] Ability_String = typeof(ABILITY).GetList().ToArray();

            for (int i = 1; i <= Ability_String.Length; i++)
            {
                Button Button = new Button()
                {
                    Width = 90
                };
                AbilityCanvas.Children.Add(Button);
                Canvas.SetTop(Button, 8 + ((i - 1) / 4) * 33);
                Canvas.SetLeft(Button, 36 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Template\Ability\" + Ability_String[i - 1] + ".png"),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run(Ability_String[i - 1])));
                Zone.Margin = new Thickness(0, 4, 0, 0);
                Button.Content = Zone;
                Button.Tag = Ability_String[i - 1];
                Button.Click += Button_Ability;
            }
            Button Reset = new Button()
            {
                IsEnabled = false,
                Width = 90,
                Name = "Ability_Reset"
            };
            Canvas.SetTop(Reset, 80);
            Canvas.SetLeft(Reset, 286);
            AbilityCanvas.Children.Add(Reset);
            Reset.Click += Button_ResetAbility;
            {
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Other\Reset.png"),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run("Reset")));
                Zone.Margin = new Thickness(0, 4, 0, 0);
                Reset.Content = Zone;
            }
        }

        private void Button_ResetAbility(object sender, RoutedEventArgs e)
        {
            CurrentCard.ResetAbility();
            YgoView.Render(CurrentCard);
            RefreshControl();
        }
        
        private void Button_Ability(object sender, RoutedEventArgs e)
        {
            ABILITY ability = (sender as Button).Tag.ToString().ToEnum<ABILITY>();
            if (CurrentCard.Abilities.Contains(ability))
            {
                CurrentCard.SetAbility(ability, false);
            }
            else
            {
                CurrentCard.SetAbility(ability, true);

            }
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void DEF_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;
            CurrentCard.SetDEF(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void ATK_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;
            CurrentCard.SetATK(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void LoadScaleControl()
        {
            Image InLeft = new Image();
            InLeft.Source = Database.GetImage(@"Template\Middle\ScaleLeft.png");
            InLeft.Width = 32;
            InLeft.Height = 32;
            Canvas.SetLeft(InLeft, 188);
            Canvas.SetTop(InLeft, 64);
            Canvas.SetTop(ScaleLeftBox, Canvas.GetTop(InLeft) + 40);
            Canvas.SetLeft(ScaleLeftBox, Canvas.GetLeft(InLeft) + 20);
            ScaleCanvas.Children.Add(InLeft);
            
                Image InRight = new Image()
                {
                    Source = Database.GetImage(@"Template\Middle\ScaleRight.png"),
                    Width = 32,
                    Height = 32
                };
                Canvas.SetLeft(InRight, 322);
                Canvas.SetTop(InRight, Canvas.GetTop(InLeft));
                ScaleCanvas.Children.Add(InRight);

                Canvas.SetTop(ScaleRightBox, Canvas.GetTop(InRight) + 40);
                Canvas.SetLeft(ScaleRightBox, Canvas.GetLeft(InRight) - 30);
            
    
            Button button = new Button();
            TextBlock Text = new TextBlock()
            {
                Text = "? - Change - ?",
                TextAlignment = TextAlignment.Center,
                FontSize = 14
            };
            button.Click += Button_Scale;
            button.Width = 100;
            button.Content = Text;
            button.Tag = "?";
            ScaleCanvas.Children.Add(button);
            Canvas.SetTop(button, 58);
            Canvas.SetLeft(button, 221);

            int left = 16, top = 14; //110-36
            for (int i = 0; i <= 13; i++)
            {
                Button scaleButton = new Button()
                {
                    Width = 50
                };
                ScaleCanvas.Children.Add(scaleButton);
                Canvas.SetTop(scaleButton, top + 36 * ((i - 1) / 3));
                Canvas.SetLeft(scaleButton, left + ((i - 1) % 3) * (scaleButton.Width + 4));
                if (i == 0 || i == 13)
                {
                    Canvas.SetTop(scaleButton, top);
                    Canvas.SetLeft(scaleButton, i == 0 ? 210 : 280);
                }
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Template\Pendulum.png"),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                Zone.Children.Add(Icon);
                Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                Zone.Margin = new Thickness(0, 4, 0, 0);
                scaleButton.Content = Zone;
                scaleButton.Tag = i;
                scaleButton.Click += Button_Scale;
            }
        }

        private void Button_Scale(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int value;
            CurrentCard.SetScaleLeft(int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            CurrentCard.SetScaleRight(int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void LoadMiddleControl()
        {            
            TabItem Level = new TabItem()
            {
                Header = "Level",
                Content = new Canvas()
            };
            MiddleTab.Items.Add(Level);
            TabItem Rank = new TabItem()
            {
                Header = "Rank",
                Content = new Canvas()
            };
            MiddleTab.Items.Add(Rank);
            TabItem Property = new TabItem()
            {
                Header = "Property",
                Content = new Canvas()
            };
            MiddleTab.Items.Add(Property);
            MiddleReload(Level.Content as Canvas, "Level");
            MiddleReload(Rank.Content as Canvas, "Rank");
            MiddleReload(Property.Content as Canvas, "Property");
        }

        private void MiddleReload(Canvas MiddleCanvas, string type)
        {
            MiddleCanvas.Children.Clear();
            if (type != "Property")
            {
                int left = 64, top = 30;
                for (int i = 0; i <= 12; i++)
                {
                    Button Button = new Button()
                    {
                        Width = 50
                    };
                    MiddleCanvas.Children.Add(Button);
                    if (i == 0)
                    {
                        Canvas.SetTop(Button, top);
                        Canvas.SetLeft(Button, 278);
                    }
                    else if (i <= 4)
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
                    StackPanel Zone = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    Image Icon = new Image()
                    {
                        Source = Database.GetImage(@"Template\Middle\" + type + ".png"),
                        Width = 20,
                        Height = 20,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 0, 4, 0)
                    };
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Button.Tag = type.Replace(' ', '_') + "_" + i.ToString();
                    Button.Content = Zone;
                    Button.Click += Button_LR;
                }                
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

                        StackPanel Zone = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };
                        Button.Content = Zone;
                        Image Icon = new Image()
                        {
                            Source = Database.GetImage(@"Template\Middle\" + Property_String[i - 1].Remove("-") + ".png"),
                            Width = 20,
                            Height = 20,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 0, 6, 0)
                        };
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
            CurrentCard.SetProperty(Frame == "Spell" ? FRAME.Spell : FRAME.Trap, Property.ToEnum<PROPERTY>());
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void Button_LR(object sender, RoutedEventArgs e)
        {
            string type = (sender as Button).Tag.ToString().Split('_')[0];
            int number = Int32.Parse((sender as Button).Tag.ToString().Split('_')[1]);
            if (type == "Level")
            {
                CurrentCard.SetLevel(number);
                CurrentCard.SetRank(double.NaN, false);
            }
            else
            {
                CurrentCard.SetLevel(double.NaN, false);
                CurrentCard.SetRank(number);
            }
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void LoadAttributeControl()
        {
            int left = 54;
            string[] Attribute_String = typeof(ATTRIBUTE).GetList().Where(o => o != ATTRIBUTE.UNKNOWN.ToString()).ToArray();
            for (int i = 1; i <= Attribute_String.Length; i++)
            {
                Button Button = new Button()
                {
                    Width = 70
                };
                AttributeCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 10);
                        Canvas.SetLeft(Button, left + ((i - 1) % 3) * (Button.Width + 4) - 14);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 10);
                        Canvas.SetLeft(Button, left + 3 * (Button.Width + 4) - 14);
                    }
                    else if (i >= 8)
                    {
                        Canvas.SetTop(Button, 92);
                        Canvas.SetLeft(Button, left + 68 + (i - 8) * (Button.Width + 16) - 14);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 46);
                        Canvas.SetLeft(Button, left + ((i - 3) % 4) * (Button.Width + 4) - 14);
                    }
                }
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Template\Attribute\" + Attribute_String[i - 1] + ".png"),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 4, 0)
                };
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
            CurrentCard.SetAttribute((sender as Button).Tag.ToString().ToEnum<ATTRIBUTE>());
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void LoadFrameControl()
        {
            List<string> frameList = typeof(FRAME).GetList().ToList();
            frameList.Insert(frameList.IndexOf("Spell"), "Pendulum");
            string[] Frame_String = frameList.ToArray();
            double left = 25;
            for (int i = 1; i <= Frame_String.Length; i++)
            {
                Button Button = new Button()
                {
                    Width = 78
                };
                FrameCanvas.Children.Add(Button);
                {
                    if (i <= 2)
                    {
                        Canvas.SetTop(Button, 10);
                        Canvas.SetLeft(Button, ((i - 1) % 3) * (Button.Width + 4) + left);
                    }
                    else if (i == 7)
                    {
                        Canvas.SetTop(Button, 10);
                        Canvas.SetLeft(Button, 3 * (Button.Width + 4) + left);
                    }
                    else if (i == 8) //Pendulum
                    {
                        Button.Width += 32;
                        Canvas.SetTop(Button, 92);
                        Canvas.SetLeft(Button, (i - 8) * (Button.Width + 16) + left);
                    }
                    else if (i >= 9)
                    {
                        Canvas.SetTop(Button, 92);
                        Canvas.SetLeft(Button, 82 + (i - 8) * (Button.Width + 4) + left);
                    }
                    else
                    {
                        Canvas.SetTop(Button, 46);
                        Canvas.SetLeft(Button, ((i - 3) % 4) * (Button.Width + 4) + left);
                    }
                }
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
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
                CurrentCard.SetPendulum(!CurrentCard.IsPendulum);
            }
            else
            {
                CurrentCard.SetFrame(frameName.ToEnum<FRAME>());
            }
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void LoadTypeControl()
        {
            string[] Type_String = typeof(TYPE).GetList().Where(o => o != TYPE.NONE.ToString()).ToArray();

            for (int i = 1; i <= Type_String.Length; i++)
            {
                Button Button = new Button()
                {
                    Width = 90
                };
                TypeCanvas.Children.Add(Button);
                {
                    if (i >= Type_String.Length - 4)
                    {
                        Button.Width += 18;
                        if (i >= Type_String.Length - 1)
                        {
                            Canvas.SetTop(Button, 224 + ((i - (Type_String.Length - 1)) / 2) * 33);
                            Canvas.SetLeft(Button, 35 + ((i - (Type_String.Length - 1)) % 2) * (Button.Width + 4) + 44);
                        }
                        else
                        {
                            Canvas.SetTop(Button, 188 + ((i - (Type_String.Length - 4)) / 3) * 33);
                            Canvas.SetLeft(Button, 35 + ((i - (Type_String.Length - 4)) % 3) * (Button.Width + 4) - 10);
                        }
                    }
                    else
                    {
                        Canvas.SetTop(Button, 6 + ((i - 1) / 4) * 33);
                        Canvas.SetLeft(Button, 35 + ((i - 1) % 4) * (Button.Width + 4) - 32);
                        if (i >= Type_String.Length - 7)
                        {
                            Canvas.SetTop(Button, Canvas.GetTop(Button) + 2);
                            Canvas.SetLeft(Button, Canvas.GetLeft(Button) + 44);
                        }
                    }
                }
                StackPanel Zone = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Image Icon = new Image()
                {
                    Source = Database.GetImage(@"Template\Type\" + Type_String[i - 1] + ".png"),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                Zone.Children.Add(Icon);
                string type = Type_String[i - 1].ToEnum<TYPE>().GetString();
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
                {
                    Button.Width += 14;
                }
                Button.Height = 30;
                Canvas.SetTop(Button, 8 + (i / 4) * (Button.Height + 10));

                int Left = 5;
                if (i >= 8)
                    Left = 32;
                else if (i >= 4)
                    Left = 5;
                Canvas.SetLeft(Button, Left + (i % 4) * (Button.Width + 10));
                Button.Click += Button_Rarity;
                Button.Tag = Rarity[i];
                RarityCanvas.Children.Add(Button);
            }
        }

        private void Button_Type(object sender, RoutedEventArgs e)
        {
            CurrentCard.SetType((sender as Button).Tag.ToString().ToEnum<TYPE>());
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void Sticker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            string Text = (sender as ComboBox).SelectedValue.ToString();
            CurrentCard.SetSticker(Text.Remove(" ").ToEnum<STICKER>());
            YgoView.Render(CurrentCard);
        }

        private void SetNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            CurrentCard.SetSet((sender as TextBox).Text);
            YgoView.Render(CurrentCard);
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            int value;
            int.TryParse((sender as TextBox).Text, out value);
            CurrentCard.SetNumber(value);
            YgoView.Render(CurrentCard);
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
            CurrentCard.SetEdition(Text.Remove(" ").Replace("1st", "First").ToEnum<EDITION>());
            YgoView.Render(CurrentCard);
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
            string futureText = RenderText((sender as TextBox).Text);
            
            CurrentCard.SetName(futureText);

            RefreshArtwork();
            RefreshControl();
        }

        private void Button_Rarity(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            CurrentCard.SetRarity(button.Tag.ToString().ToEnum<RARITY>());
            YgoView.Render(CurrentCard);
            //RefreshRarityControl();
            RefreshControl();
        }
        private void ScaleLeft_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;
            CurrentCard.SetScaleLeft(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void ScaleRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inRefreshControl) { return; }
            TextBox txtBox = sender as TextBox;
            int value;
            CurrentCard.SetScaleRight(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void ApplyDocument_Click(object sender, RoutedEventArgs e)
        {
            if (inRefreshControl) { return; }
            CurrentCard.SetDescription(RenderText(DescriptionBox.GetText()));
            CurrentCard.SetPendulumEffect(RenderText(PendulumBox.GetText()));
            RefreshControl();
            YgoView.Render(CurrentCard);
        }

        private void CardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inChangeSetCard) { return; }
            //currentIndex = cardList.SelectedIndex;
            //MessageBox.Show(currentIndex.ToString());
            if (CurrentCard != null)
            {
                ControlArtwork.Source = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray();
            }
            RefreshArtwork();
            RefreshControl();
        }

        private void CardList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (inChangeSetCard) { return; }
            if (e.Key == Key.Delete)
            {
                DeleteCurrentCard();
            }
        }

        private void DeleteCurrentCard()
        {
            if (CurrentSet.Cards.Count() <= 1)
            {
                return;
            }

            int previewIndex = CardList.SelectedIndex;
            inChangeSetCard = true;
            CurrentSet.Cards.RemoveAt(CardList.SelectedIndex);

            CardList.SelectedIndex = -1;
            //CardList.ItemsSource = CurrentSet.Cards;

            //ControlArtwork.Source = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray();

            //RefreshArtwork();
            //RefreshControl();
            inChangeSetCard = false;

            if (CardList.SelectedIndex - 1 < 0)
            {
                CardList.SelectedIndex = 0;
            }
            else
            {
                //CardList.SelectedIndex = Math.Min(previewIndex - 1, CurrentSet.Cards.Count - 1);
                CardList.SelectedIndex--;
            }
        }

        private void AddNewCard()
        {            
            YgoCard newCard = YgoCard.Default;
            while (CurrentSet.Cards.Any(o => o.Name == newCard.Name))
            {
                currentNewIndex++;
                if (currentNewIndex > 1)
                {
                    newCard.SetName(YgoCard.Default.Name + " " + currentNewIndex);
                }
                    
            }
            CurrentSet.Cards.Add(newCard);
            CardList.SelectedIndex++;            
        }



        private void ArtworkBrowser_Click(object sender, RoutedEventArgs e)
        {
            BrowseArtwork();
            
        }

        private void BrowseArtwork()
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
                CurrentCard.ArtworkByte = Database.GetImage(@"Template\NoneImage.png").GetImageArray();
                ControlArtwork.Source = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray();
                RefreshArtwork();
            }
            else if (File.Exists(filePath))
            {
                CurrentCard.ArtworkByte = Images.GetImageByte(filePath);
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
            CurrentCard.SetCreator(isChecked ? CREATOR.KazukiTakahashi : CREATOR.NONE);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void resizeArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(ArtworkPaddingHeight.Text, out value))
            {
                YgoView.ResizeArtwork(value, true);
            }
        }

        private void previewResizeArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(ArtworkPaddingHeight.Text, out value))
            {
                ControlArtwork.Source = YgoView.ResizeArtwork(value, false);
            }

        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var TxtBox = sender as TextBox;
            TxtBox.CaretIndex = TxtBox.Text.Length;
        }

        private void SetEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (inChangeSetCard) { return; }
            if (CurrentSet != null)
            {
                CurrentSet.SetName(SetEdit.Text);
            }
        }

        private void AssociateExtension(bool isAssociate)
        {
            CustomFileExtension cardExtension = new CustomFileExtension();
            cardExtension.ApplicationName = "OV.Creation.exe";
            cardExtension.Description = "OV.Creation Card";
            cardExtension.EmbeddedIcon = false;
            cardExtension.Extension = CARD_EXTENSION;
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
            setExtension.Extension = SET_EXTENSION;
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
        
        private void CardDetailsScroll_Loaded(object sender, RoutedEventArgs e)
        {
            var cardDetailsScroll = sender as ScrollViewer;
            double offset;
            if (cardDetailsScroll.Tag != null && double.TryParse(cardDetailsScroll.Tag.ToString(), out offset))
            {
                cardDetailsScroll.ScrollToVerticalOffset(offset);
            }
        }

        private void CardDetailsScroll_Unloaded(object sender, RoutedEventArgs e)
        {
            var cardDetailsScroll = sender as ScrollViewer;
            cardDetailsScroll.Tag = cardDetailsScroll.VerticalOffset;
        }

        #region ScrollToCanvas
        private void ScrollToControlCanvas(Border border)
        {
            bool inDetail = tabControl.SelectedIndex == 0;
            if (inDetail == false)
            {
                tabControl.SelectedIndex = 0;
                RoutedEventHandler eventScroll = null;
                eventScroll = (sender, e) =>
                {
                    ScrollToControlCanvasInside(border, inDetail);
                    border.Loaded -= eventScroll;
                };

                border.Loaded += eventScroll;
            }
            else
            {
                ScrollToControlCanvasInside(border, inDetail);
            }
        }

        private void ScrollToControlCanvasInside(Border border, bool inDetail)
        {
            StackPanel container = border.Parent as StackPanel;
            Point relativeLocation = border.TranslatePoint(new Point(0, 0), container);
            var offset = relativeLocation.Y;

            Scroll.ScrollToVerticalOffset(offset);
            if ((border.Child as Expander) != null)
            {
                var Ex = (border.Child as Expander);
                if (offset == Scroll.VerticalOffset && inDetail)
                {
                    Ex.IsExpanded = !Ex.IsExpanded;
                }
                else
                {
                    Ex.IsExpanded = true;
                }
            }
            if (border == NameBorder)
            {
                (RarityBorder.Child as Expander).IsExpanded = true;
            } else if (border == TypeBorder)
            {
                (AbilityBorder.Child as Expander).IsExpanded = true;
            } else if (border == ValueBorder)
            {
                (ScaleBorder.Child as Expander).IsExpanded = true;
            }
        }

        private void RenderCard_AttributeClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(AttributeBorder);
        }
        private void RenderCard_NameClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(NameBorder);
        }
        private void RenderCard_TypeClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(TypeBorder);
        }
        private void RenderCard_CirculationClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(CirculationBorder);
        }

        private void RenderCard_ValueClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(ValueBorder);
        }
        private void RenderCard_ArtworkClick(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }
        private void RenderCard_DescriptionClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(DocumentBorder);
        }
        private void RenderCard_FrameClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(FrameBorder);
        }
        private void RenderCard_MiddleClick(object sender, EventArgs e)
        {
            ScrollToControlCanvas(MiddleBorder);
        }
        #endregion ScrollToCanvas

        private void InsideBorderExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            var canvas = expander.Content as Canvas;
            var border = expander.Parent as Border;
            if (canvas != null)
            {
                Grid.Height -= canvas.Height - border.MinHeight;
                Animations.SynchroAnimation(border,
                    border.MinHeight, Border.HeightProperty);

                RefreshExpanders();
            }
        }

        

        private void InsideBorderExpander_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            var canvas = expander.Content as Canvas;
            var border = expander.Parent as Border;
            if (canvas != null)
            {
                Grid.Height += canvas.Height - border.MinHeight;
                Animations.SynchroAnimation(border, canvas.Height, Border.HeightProperty);

                RefreshExpanders();
            }
        }

        
                        
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNewCard();
        }

        private void ExportPathButton_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog folderDialog = new Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                exportPathTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void SetSetting(string value, params string[] valuePath)
        {
            string data = File.ReadAllText("settings.json");
            JObject settings = JObject.Parse(JsonConvert.DeserializeObject(data).ToString());

            JToken obj = null;
            foreach(string path in valuePath)
            {
                if (obj == null)
                {
                    obj = settings[path];
                } else
                {
                    obj = obj[path];
                }
            }
            obj = value;

            string result = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json", result);
        }

        private void ExportPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            string data = File.ReadAllText("settings.json");
            JObject settings = JObject.Parse(JsonConvert.DeserializeObject(data).ToString());
            //settings["Export"]["ExportPath"] = (sender as TextBox).Text;
            var k = settings["Export"];
            k["ExportPath"] = "omg";

            string result = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json",result);*/
            SetSetting((sender as TextBox).Text, "Export", "Path");
        }

        private void SingleImageExport()
        {
            string folderPath = exportPathTextBox.Text;
            string cardName = YgoView.CurrentCard.Name.ToNonVietnamese();

            string extension = (SingleJPG.IsChecked == true ?
                ".jpg" : (SinglePNG.IsChecked == true ?
                    ".png" : ".bmp"));

            string fileName = folderPath + @"\" + cardName + extension;
            YgoView.SaveImageTo(fileName);
        }

        private void SingleDataExport()
        {
            string folderPath = exportPathTextBox.Text;
            var main = Application.Current.MainWindow as MainWindow;
            string cardName = main.YgoView.CurrentCard.Name.ToNonVietnamese();

            string extension = CARD_EXTENSION;

            string fileName = folderPath + @"\" + cardName + extension;
            //main.YgoView.SaveDataTo(fileName);
            main.YgoView.CurrentCard.SaveTo(fileName);
        }

        private void SetImageExport()
        {            
            string folderPath = exportPathTextBox.Text;            
            YgoSet set = CurrentSet;

            folderPath += @"\" + set.Name + @"\Image\"; // in Images folder
            Directory.CreateDirectory(folderPath);

            foreach (YgoCard card in set.Cards)
            {
                string extension = (SetJPG.IsChecked == true ?
                    ".jpg" : (SetPNG.IsChecked == true ?
                        ".png" : ".bmp"));

                string fileName = folderPath + @"\" + card.Name.ToNonVietnamese() + extension;

                
                YgoView ygoView = new YgoView();

                
                //
                ygoView.Render(card);
                ygoView.SaveImageTo(fileName);
            }
        }

        private void SetDataExport()
        {
            string folderPath = exportPathTextBox.Text;
            var main = Application.Current.MainWindow as MainWindow;
            YgoSet set = main.CurrentSet;


            folderPath += @"\" + set.Name;
            Directory.CreateDirectory(folderPath);
            string extension = SET_EXTENSION;

            set.SaveTo(folderPath + @"\" + set.Name + extension);
            if (EachCardData.IsChecked == true)
            {
                Directory.CreateDirectory(folderPath + @"\Data\");
                foreach (YgoCard card in set.Cards)
                {
                    extension = CARD_EXTENSION;

                    string fileName = folderPath + @"\Data\" + card.Name.ToNonVietnamese() + extension;
                    card.SaveTo(fileName);
                }
            }
        }

        #region Event

        private void SingleImageExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            string folderPath = exportPathTextBox.Text;
            if (Directory.Exists(folderPath))
            {
                SingleImageExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        private void SingleDataExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            string folderPath = exportPathTextBox.Text;
            if (Directory.Exists(folderPath))
            {
                SingleDataExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        private void SingleAllExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            string folderPath = exportPathTextBox.Text;
            if (Directory.Exists(folderPath))
            {
                SingleDataExport();
                SingleImageExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        private Dictionary<YgoCard, int> DuplicateCards()
        {
            var list = CurrentSet.Cards.GroupBy(x => x).Where(y => y.Count() > 1)
                .ToDictionary(x => x.Key, y => y.Count());
            return list;
        }

        private void SetImageExportButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = exportPathTextBox.Text;
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            if (Directory.Exists(folderPath))
            {
                SetImageExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        private void SetDataExportButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = exportPathTextBox.Text;
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            if (Directory.Exists(folderPath))
            {
                SetDataExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        private void SetAllExportButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = exportPathTextBox.Text;
            if (DuplicateCards().Count > 0)
            {
                MessageBox.Show("A duplicate name exists on the set. Please change it name!");
                return;
            }
            if (Directory.Exists(folderPath))
            {
                SetImageExport();
                SetDataExport();
                MessageBox.Show("Completed");
            }
            else
            {
                MessageBox.Show(string.Format("The folder {0} cannot be found.", folderPath));
            }
        }

        #endregion Event
        
        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadSet();
        }
        

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //SaveSet(currentFilePath);
            tabControl.SelectedIndex = 4;
        }
        
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (IsSaved == false)
            {                
                MessageBoxResult result = MessageBox.Show("Want to save your current card/set?",
                    "", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    //SaveSet(currentFilePath);
                    tabControl.SelectedIndex = 4; //ExportTab
                    e.Cancel = false;
                }
                else if (result == MessageBoxResult.No)
                {
                    e.Cancel = false;
                }                
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void CardList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }

        private void NextCardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CardList.SelectedIndex + 1 >= CurrentSet.Cards.Count)
            {
                CardList.SelectedIndex = 0;
            }
            else
            {
                CardList.SelectedIndex++;
            }
        }

        private void PreviousCardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CardList.SelectedIndex - 1 < 0)
            {
                CardList.SelectedIndex = CurrentSet.Cards.Count - 1;
            }
            else
            {
                CardList.SelectedIndex--;
            }
        }

        private void SetCardToDefaultCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string previousName = CurrentCard.Name;
            CurrentCard = YgoCard.Default;
            CurrentCard.SetName(previousName);
            YgoView.Render(CurrentCard);
            RefreshControl();
        }

        private void NewCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            if (inChangeSetCard || CurrentSet == null)
            {
                e.CanExecute = false;
            }            
        }

        private void RemoveCurrentCard_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteCurrentCard();
        }

        private void RemoveCurrentCard_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;        
            if (CurrentSet?.Cards.Count <= 1)
            {                
                e.CanExecute = false;
            }
        }

        private void SimpleSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            QuickImageRender();
        }

        private void SwitchMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            tabControl.SelectedIndex = (tabControl.SelectedIndex == 0) ? 2 : 0;
        }
        
        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(GetLocationPath() + @"\Resources\Help\index.html"))
            {
                System.Diagnostics.Process.Start(GetLocationPath() + @"\Resources\Help\index.html");
            } else
            {
                MessageBox.Show("The Help file cannot be found!\nPlease download it again!");
            }
            
        }

        private void FeedbackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/document/d/1KNWFNbJZGC5gWUDwVntbHiQ6vQV_8nuRGnj1ehcVMmE/edit?usp=sharing");
        }

        private void FacebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/OV.Pyramid");
        }

        private void MediafireCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mediafire.com/folder/vamylw814g9iq/OV_Pyramid");
        }

        private void Card2Clipboard_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(DataText.Text);
            MessageBox.Show("Data has been copied.");
        }

        private void ExpandAllButton_Click(object sender, RoutedEventArgs e)
        {
            Grid.FindChildrens<Expander>().Where(o => o.IsExpanded == false)
                .ToList().ForEach(o => o.IsExpanded = true);
            Scroll.ScrollToTop();
        }

        private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            Grid.FindChildrens<Expander>().Where(o => o.IsExpanded == true)
                .ToList().ForEach(o => o.IsExpanded = false);
            Scroll.ScrollToTop();
        }

        private void YgoView_ArtworkDoubleClick(object sender, EventArgs e)
        {
            BrowseArtwork();
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
               if (e.ClickCount > 1)
            {
                BrowseArtwork();
            }
        }
    }


    public static class TabContent
    {
        public static bool GetIsCached(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCachedProperty);
        }

        public static void SetIsCached(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCachedProperty, value);
        }

        /// <summary>
        /// Controls whether tab content is cached or not
        /// </summary>
        /// <remarks>When TabContent.IsCached is true, visual state of each tab is preserved (cached), even when the tab is hidden</remarks>
        public static readonly DependencyProperty IsCachedProperty =
            DependencyProperty.RegisterAttached("IsCached", typeof(bool), typeof(TabContent), new UIPropertyMetadata(false, OnIsCachedChanged));


        public static DataTemplate GetTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(TemplateProperty);
        }

        public static void SetTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(TemplateProperty, value);
        }

        /// <summary>
        /// Used instead of TabControl.ContentTemplate for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent), new UIPropertyMetadata(null));


        public static DataTemplateSelector GetTemplateSelector(DependencyObject obj)
        {
            return (DataTemplateSelector)obj.GetValue(TemplateSelectorProperty);
        }

        public static void SetTemplateSelector(DependencyObject obj, DataTemplateSelector value)
        {
            obj.SetValue(TemplateSelectorProperty, value);
        }

        /// <summary>
        /// Used instead of TabControl.ContentTemplateSelector for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent), new UIPropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TabControl GetInternalTabControl(DependencyObject obj)
        {
            return (TabControl)obj.GetValue(InternalTabControlProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalTabControl(DependencyObject obj, TabControl value)
        {
            obj.SetValue(InternalTabControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalTabControl.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty InternalTabControlProperty =
            DependencyProperty.RegisterAttached("InternalTabControl", typeof(TabControl), typeof(TabContent), new UIPropertyMetadata(null, OnInternalTabControlChanged));


        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ContentControl GetInternalCachedContent(DependencyObject obj)
        {
            return (ContentControl)obj.GetValue(InternalCachedContentProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalCachedContent(DependencyObject obj, ContentControl value)
        {
            obj.SetValue(InternalCachedContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalCachedContent.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty InternalCachedContentProperty =
            DependencyProperty.RegisterAttached("InternalCachedContent", typeof(ContentControl), typeof(TabContent), new UIPropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object GetInternalContentManager(DependencyObject obj)
        {
            return (object)obj.GetValue(InternalContentManagerProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalContentManager(DependencyObject obj, object value)
        {
            obj.SetValue(InternalContentManagerProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalContentManager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalContentManagerProperty =
            DependencyProperty.RegisterAttached("InternalContentManager", typeof(object), typeof(TabContent), new UIPropertyMetadata(null));

        private static void OnIsCachedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;

            var tabControl = obj as TabControl;
            if (tabControl == null)
            {
                throw new InvalidOperationException("Cannot set TabContent.IsCached on object of type " + args.NewValue.GetType().Name +
                    ". Only objects of type TabControl can have TabContent.IsCached property.");
            }

            bool newValue = (bool)args.NewValue;

            if (!newValue)
            {
                if (args.OldValue != null && ((bool)args.OldValue))
                {
                    throw new NotImplementedException("Cannot change TabContent.IsCached from True to False. Turning tab caching off is not implemented");
                }

                return;
            }

            EnsureContentTemplateIsNull(tabControl);
            //tabControl.ContentTemplate = CreateContentTemplate();
            EnsureContentTemplateIsNotModified(tabControl);
        }

        private static DataTemplate CreateContentTemplate()
        {
            
            const string xaml =
                "<DataTemplate><Border b:TabContent.InternalTabControl=\"{Binding RelativeSource={RelativeSource AncestorType=TabControl}}\" /></DataTemplate>";

            var context = new ParserContext()
            {
                XamlTypeMapper = new XamlTypeMapper(new string[0])
            };
            context.XamlTypeMapper.AddMappingProcessingInstruction("b", typeof(TabContent).Namespace, typeof(TabContent).Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("b", "b");

            var template = (DataTemplate)XamlReader.Parse(xaml, context);
            return template;
        }

        private static void OnInternalTabControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;
            var container = obj as Decorator;

            if (container == null)
            {
                var message = "Cannot set TabContent.InternalTabControl on object of type " + obj.GetType().Name +
                    ". Only controls that derive from Decorator, such as Border can have a TabContent.InternalTabControl.";
                throw new InvalidOperationException(message);
            }

            if (args.NewValue == null) return;
            if (!(args.NewValue is TabControl))
            {
                throw new InvalidOperationException("Value of TabContent.InternalTabControl cannot be of type " + args.NewValue.GetType().Name + ", it must be of type TabControl");
            }

            var tabControl = (TabControl)args.NewValue;
            var contentManager = GetContentManager(tabControl, container);
            contentManager.UpdateSelectedTab();
        }

        private static ContentManager GetContentManager(TabControl tabControl, Decorator container)
        {
            var contentManager = (ContentManager)GetInternalContentManager(tabControl);
            if (contentManager != null)
            {
                /*
                 * Content manager already exists for the tab control. This means that tab content template is applied 
                 * again, and new instance of the Border control (container) has been created. The old container 
                 * referenced by the content manager is no longer visible and needs to be replaced
                 */
                contentManager.ReplaceContainer(container);
            }
            else
            {
                // create content manager for the first time
                contentManager = new ContentManager(tabControl, container);
                SetInternalContentManager(tabControl, contentManager);
            }

            return contentManager;
        }

        private static void EnsureContentTemplateIsNull(TabControl tabControl)
        {
            if (tabControl.ContentTemplate != null)
            {
                throw new InvalidOperationException("TabControl.ContentTemplate value is not null. If TabContent.IsCached is True, use TabContent.Template instead of ContentTemplate");
            }
        }

        private static void EnsureContentTemplateIsNotModified(TabControl tabControl)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty, typeof(TabControl));
            descriptor.AddValueChanged(tabControl, (sender, args) =>
            {
                throw new InvalidOperationException("Cannot assign to TabControl.ContentTemplate when TabContent.IsCached is True. Use TabContent.Template instead");
            });
        }

        public class ContentManager
        {
            TabControl _tabControl;
            Decorator _border;

            public ContentManager(TabControl tabControl, Decorator border)
            {
                _tabControl = tabControl;
                _border = border;
                _tabControl.SelectionChanged += (sender, args) => { UpdateSelectedTab(); };
            }

            public void ReplaceContainer(Decorator newBorder)
            {
                if (Object.ReferenceEquals(_border, newBorder)) return;

                _border.Child = null; // detach any tab content that old border may hold
                _border = newBorder;
            }

            public void UpdateSelectedTab()
            {
                _border.Child = GetCurrentContent();
            }

            private ContentControl GetCurrentContent()
            {
                var item = _tabControl.SelectedItem;
                if (item == null) return null;

                var tabItem = _tabControl.ItemContainerGenerator.ContainerFromItem(item);
                if (tabItem == null) return null;

                var cachedContent = TabContent.GetInternalCachedContent(tabItem);
                if (cachedContent == null)
                {
                    cachedContent = new ContentControl
                    {
                        DataContext = item,
                        ContentTemplate = TabContent.GetTemplate(_tabControl),
                        ContentTemplateSelector = TabContent.GetTemplateSelector(_tabControl)
                    };

                    cachedContent.SetBinding(ContentControl.ContentProperty, new Binding());
                    TabContent.SetInternalCachedContent(tabItem, cachedContent);
                }

                return cachedContent;
            }
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
}