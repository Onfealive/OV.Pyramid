using System.Windows;
using System.Windows.Documents;
using OV.Tools;
using System.Windows.Input;
using System.Windows.Controls;
using Newtonsoft.Json;
using OV.Core;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.IO;
using static OV.Tools.Animations;
using static OV.Tools.Utilities;
using System;
using System.Linq;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        YGOCard Current = YGOCard.Default;
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

            
            RenderCard.SetDefaultArtwork(Utilities.GetLocationPath() + @"\Template\NoneImage.png");
            
            
            
            RenderCard.Render(Current);

            LoadControl();
            LoadButton();
        }

        void LoadButton()
        {
            Button[] Stack = new Button[] { Home, Talk, About, Export, Save, Load, Update, Exit };
            for (int i = 0; i < Stack.Length; i++)
            {
                StackPanel Zone = new StackPanel();

                Zone.Orientation = Orientation.Vertical;

                Image Icon = new Image();
                Icon.Source = Images.GetImage(GetLocationPath() + "Template/Other/" + Stack[i].Name + ".png");
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
            RenderCard.Load();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            RenderCard.Save();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            
                // Save document 
                //CreateBitmapFromVisual(Card, dlg.FileName);
             RenderCard.Export();

            

        }

        private void ArtworkGet()
        {
            Microsoft.Win32.OpenFileDialog choofdlog = new Microsoft.Win32.OpenFileDialog();
            choofdlog.Filter = "Image Files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;
            string sFileName = null;
            if (choofdlog.ShowDialog() == true)
            {
                sFileName = choofdlog.FileName;
                /*
                Artwork.Source = Uti.GetImage(sFileName, true);
                Current.Artwork = sFileName;

                Artwork.Tag = "Already";
                Artwork.Opacity = 1;
                ChangeRarity(Current.Rarity);

                Refresh();
                */
                //card.ArtworkByte = Images.GetImageByte(sFileName);
                //RenderCard.RenderCard(card);
            }
        }

        private void LoadControl()
        {
            Canvas Panel;
            TextBlock Caption;
            TextBox Edit;

            {
                string[] Rarity = new string[] {"Common", "Rare", "Super_Rare", "Ultra_Rare",
                    "Secret_Rare", "Parallel_Rare", "Starfoil_Rare", "Mosaic_Rare",
                    "Gold_Rare", "Ghost_Rare", "Ultimate_Rare"};

                for (int i = 0; i < Rarity.Length; i++)
                {
                    Button Button = new Button();
                    Button.Name = "Rarity_" + Rarity[i];
                    Button.Content = Rarity[i].Replace('_', ' ');
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

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Edit = new TextBox();
                Caption.Name = "NameCaption";
                Edit.Name = "NameEdit";
                Panel.Children.Add(Edit);
                Panel.Children.Add(Caption);

                NameExpander.Header = Panel;
                Caption.Text = "Name";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);

                Canvas.SetLeft(Edit, 60);
                Canvas.SetTop(Edit, -2);
                Edit.Width = 280;
                Edit.FontSize = 15;
                //Edit.FontFamily = FontName.MatrixRegularSmallCaps;
                Edit.TextChanged += Name_TextBox_TextChanged;
                Edit.GotFocus += Name_TextBox_GotFocus;
                //Edit.Background = Brushes.LightSkyBlue;
                //SetBindingMath(Edit, TextBox.BackgroundProperty, "NameBorder", Border.BackgroundProperty);




            }
            
            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "CirculationCaption";

                Panel.Children.Add(Caption);

                CirculationExpander.Header = Panel;
                Caption.Text = "Circulation";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);

                {
                    ComboBox Edition = new ComboBox();
                    Edition.Name = "EditionCombo";
                    //
                    Edition.Items.Add("Unlimited Edition");
                    Edition.Items.Add("1st Edition");
                    Edition.Items.Add("Limited Edition");
                    Edition.Items.Add("Duel Terminal");
                    Edition.SelectedIndex = 0;
                    Edition.SelectionChanged += Edition_SelectionChanged;

                    Edition.Width = 120;
                    CirculationCanvas.Children.Add(Edition);
                    Canvas.SetLeft(Edition, 48);
                    Canvas.SetTop(Edition, 10);



                    
                    Button Random = new Button();
                    Random.Content = "Random";
                    Random.Click += Random_Click;
                    CirculationCanvas.Children.Add(Random);
                    Canvas.SetLeft(Random, 180);
                    Canvas.SetTop(Random, 40);

                    TextBox CardNumber = new TextBox();
                    CardNumber.Name = "CardNumberEdit";
                    CardNumber.Width = 80;
                    CardNumber.MaxLength = 8;
                    CardNumber.TextChanged += CardNumber_TextChanged;
                    CirculationCanvas.Children.Add(CardNumber);
                    Canvas.SetLeft(CardNumber, 90);
                    Canvas.SetTop(CardNumber, 40);


                    TextBlock One = new TextBlock(new Run("Edition"));
                    Canvas.SetTop(One, 12);
                    Canvas.SetLeft(One, 0);
                    CirculationCanvas.Children.Add(One);

                    TextBlock Two = new TextBlock(new Run("Card Number"));
                    Canvas.SetTop(Two, 42);
                    Canvas.SetLeft(Two, 0);
                    CirculationCanvas.Children.Add(Two);


                    TextBox SetNumber = new TextBox();
                    SetNumber.Name = "SetNumberEdit";
                    SetNumber.Width = 100;
                    SetNumber.MaxLength = 10;
                    SetNumber.TextChanged += SetNumber_TextChanged;
                    CirculationCanvas.Children.Add(SetNumber);
                    Canvas.SetLeft(SetNumber, 90);
                    Canvas.SetTop(SetNumber, 70);

                    TextBlock Three = new TextBlock(new Run("Set Number"));
                    Canvas.SetTop(Three, 72);
                    Canvas.SetLeft(Three, 0);
                    CirculationCanvas.Children.Add(Three);
                    {
                        TextBlock Four = new TextBlock(new Run("Eye of Anubis Hologram"));
                        Canvas.SetTop(Four, 12);
                        Canvas.SetLeft(Four, 230);
                        CirculationCanvas.Children.Add(Four);

                        ComboBox Sticker = new ComboBox();
                        Sticker.Name = "StickerCombo";
                        //
                        Sticker.Items.Add("None");
                        Sticker.Items.Add("Gold");
                        Sticker.Items.Add("Silver");
                        Sticker.Items.Add("Promo Silver");
                        Sticker.Items.Add("Promo Gold");
                        Sticker.SelectedIndex = 0;
                        Sticker.SelectionChanged += Sticker_SelectionChanged;
                        Sticker.Width = 110;
                        CirculationCanvas.Children.Add(Sticker);
                        Canvas.SetLeft(Sticker, 250);
                        Canvas.SetTop(Sticker, 40);
                    }

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

            }

            
            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "TypeCaption";
                Caption.Text = "Monster Type";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                TypeExpander.Header = Panel;

                string[] Type_String = Enum.GetValues(typeof(TYPE)).OfType<object>().Select(o => o.ToString()).ToArray();

                

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
                    Zone.Children.Add(new TextBlock(new Run(Type_String[i - 1])));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Button.Name = Type_String[i - 1].Replace(' ', '_').Replace("-", "0X0");
                    Button.Content = Zone;

                    Button.Click += Button_Type;

                }
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "FrameCaption";
                Caption.Text = "Card Type";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                FrameExpander.Header = Panel;

                string[] Frame_String = Enum.GetValues(typeof(FRAME)).OfType<object>().Select(o => o.ToString()).ToArray();


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
                        else if (i == 7) {
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

                    Button.Name = Frame_String[i - 1];
                    Button.Content = Zone;

                    //Button.Click += Button_Frame;

                    /*
                    if (Current.IsFrame(Frame_String[i - 1]))
                        Button.IsEnabled = false;
                    else
                        Button.IsEnabled = true;
                        */
                }
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "AttributeCaption";
                Caption.Text = "Attribute";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                AttributeExpander.Header = Panel;
                int left = 44;


                string[] Attribute_String = Enum.GetValues(typeof(ATTRIBUTE)).OfType<object>().Select(o => o.ToString()).ToArray();
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
                    Icon.Source = Images.GetImage("/Attribute/" + Attribute_String[i - 1]);
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run(Attribute_String[i - 1])));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    Button.Name = Attribute_String[i - 1];
                    Button.Content = Zone;

                    //Button.Click += Button_Attribute;

                }
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "LRCaption";
                Caption.Text = "Level";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                LevelExpander.Header = Panel;

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

                    /*
                    MiddleReload(Level.Content as Canvas, "Level");
                    MiddleReload(Rank.Content as Canvas, "Rank");
                    MiddleReload(Property.Content as Canvas, "Property");
                    */

                    //Style style = new Style(typeof(TabItem));
                    //ContentPresenter x = new ContentPresenter();
                    //x.Content = Tab.ContentTemplate;
                    //x.LayoutTransform = new RotateTransform(270);
                    //style.Setters.Add(new Setter(TabItem.HeaderTemplateProperty, new DataTemplate(x)));
                    //Tab.Resources.Add(typeof(TabItem), style);
                    Tab.Resources.Add(typeof(TabItem), this.FindResource("RotatedTab") as Style);
                }
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "ScaleCaption";
                Caption.Text = "Pendulum Scale";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                ScaleExpander.Header = Panel;

                Image InLeft = new Image();
                InLeft.Source = Images.GetImage(GetLocationPath() + "/Scale Left.png");
                InLeft.Width = 32;
                InLeft.Height = 32;
                Canvas.SetLeft(InLeft, 178);
                Canvas.SetTop(InLeft, 54);

                Canvas.SetTop(BoxScaleLeft, Canvas.GetTop(InLeft) + 40);
                Canvas.SetLeft(BoxScaleLeft, Canvas.GetLeft(InLeft) + 20);
                ScaleCanvas.Children.Add(InLeft);
                {
                    Image InRight = new Image();
                    InRight.Source = Images.GetImage(GetLocationPath() + "/Scale Right.png");
                    InRight.Width = 32;
                    InRight.Height = 32;
                    Canvas.SetLeft(InRight, 322);
                    Canvas.SetTop(InRight, Canvas.GetTop(InLeft));
                    ScaleCanvas.Children.Add(InRight);

                    Canvas.SetTop(BoxScaleRight, Canvas.GetTop(InRight) + 40);
                    Canvas.SetLeft(BoxScaleRight, Canvas.GetLeft(InRight) - 30);
                }


                Button Button = new Button();

                TextBlock Text = new TextBlock();
                Text.Text = "? - Change - ?";
                Text.TextAlignment = TextAlignment.Center;
                Text.FontSize = 14;
                Button.Name = "Pendulum_0";
                //Button.Click += Button_Scale;
                Button.Width = 100;
                Button.Content = Text;
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
                    Icon.Source = Images.GetImage(GetLocationPath() + "/Pendulum.png");
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("x" + i.ToString())));
                    Zone.Margin = new Thickness(0, 4, 0, 0);

                    ButtonX.Name = "Pendulum_" + i.ToString();
                    ButtonX.Content = Zone;

                    //ButtonX.Click += Button_Scale;
                }

            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "ATKCaption";
                Caption.Text = "ATK";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                ATKExpander.Header = Panel;

                Edit = new TextBox();
                Panel.Children.Add(Edit);
                Edit.Name = "ATKEdit";
                Canvas.SetLeft(Edit, 80);
                Canvas.SetTop(Edit, -2);
                Edit.Width = 70;
                Edit.FontSize = 14;
                Edit.MaxLength = 5;
                //Edit.TextChanged += ATK_TextBox_TextChanged;
                Edit.TextAlignment = TextAlignment.Right;
                //Edit.Background = Brushes.LightSkyBlue;
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
                Caption.Name = "DEFCaption";
                Caption.Text = "DEF";
                Caption.FontSize = 14;
                Canvas.SetLeft(Caption, 4);
                Panel.Children.Add(Caption);
                DEFExpander.Header = Panel;

                Edit = new TextBox();
                Panel.Children.Add(Edit);
                Edit.Name = "DEFEdit";
                Canvas.SetLeft(Edit, 80);
                Canvas.SetTop(Edit, -2);
                Edit.Width = 70;
                Edit.FontSize = 14;
                Edit.MaxLength = 5;
                //Edit.TextChanged += DEF_TextBox_TextChanged;
                Edit.TextAlignment = TextAlignment.Right;
                //Edit.Background = Brushes.LightSkyBlue;
            }

            {
                Panel = new Canvas();
                Caption = new TextBlock();
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

                    Button.Name = Ability_String[i - 1].Replace(' ', '_').Replace("-", "0X0");
                    Button.Content = Zone;

                    //Button.Click += Button_Ability;

                    if (Ability_String[i - 1] == "Effect")
                        Button.IsEnabled = false;

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
                    Icon.Source =Images.GetImage(GetLocationPath() + "Template/Reset");
                    Icon.Width = 20;
                    Icon.Height = 20;
                    Icon.Stretch = Stretch.Uniform;
                    Icon.Margin = new Thickness(0, 0, 4, 0);
                    Zone.Children.Add(Icon);
                    Zone.Children.Add(new TextBlock(new Run("Reset")));
                    Zone.Margin = new Thickness(0, 4, 0, 0);


                    Reset.Content = Zone;
                }

                TextBlock Cap = new TextBlock(new Run("Double-Ability:"));
                AbilityCanvas.Children.Add(Cap);
                Cap.FontSize = 14;
                Canvas.SetTop(Cap, 83);
                Canvas.SetLeft(Cap, -11);

                ComboBox Combo = new ComboBox();
                Combo.Name = "MultiAbilityCombo";
                Canvas.SetTop(Combo, 39 + 39);
                Canvas.SetLeft(Combo, 90);
                Combo.FontSize = 17;
                Combo.Width = 144;
                Combo.Items.Add("");
                Combo.Items.Add("Flip / Tuner");
                Combo.Items.Add("Gemini / Tuner");
                Combo.Items.Add("Spirit / Tuner");
                Combo.Items.Add("Toon / Tuner");
                Combo.Items.Add("Union / Tuner");
                AbilityCanvas.Children.Add(Combo);

                //Combo.SelectionChanged += MultiAbility_SelectionChanged;
                //Combo.DropDownClosed += MultiAbility_DropDownClosed;
            }


            //SetColorControl();
        }

        private void Button_Type(object sender, RoutedEventArgs e)
        {
            
        }

        private void Sticker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.Sticker = Text.Remove(" ").ToEnum<STICKER>();
            RenderCard.Render(Current);
        }

        private void SetNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            Current.Set = (sender as TextBox).Text;

            RenderCard.Render(Current);
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            Current.Number = (sender as TextBox).Text.ToInt32();
            
            RenderCard.Render(Current);
        }

        void Random_Click(object sender, RoutedEventArgs e)
        {
            TextBox CardNumber = CirculationCanvas.FindChild<TextBox>("CardNumberEdit");
            Random t = new Random();
            CardNumber.Text = t.Next(0, 99999999).ToString();
            while (CardNumber.Text.Length < 8)
            {
                CardNumber.Text = "0" + CardNumber.Text;
            }
        }

        private void Edition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.Edition = Text.Remove(" ").ToEnum<EDITION>();            
            RenderCard.Render(Current);
        }

        private void Name_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NameExpander.IsExpanded = true;
        }

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Canvas Card = Uti.FindChild<Canvas>(mainCanvas, "Card");
            Current.Name = (sender as TextBox).Text;

            //_Pending.Text = (Artwork.Source as ImageSource).ToString();
            RenderCard.Render(Current);
        }

        private void Symbol_Click(object sender, RoutedEventArgs e)
        {
            TextBox Edit = (NameExpander.Header as Canvas).FindChild<TextBox>("NameEdit");

            Edit.Text = Edit.Text.Insert(Edit.CaretIndex, (sender as Button).Content.ToString());

        }

        private void Button_Rarity(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //ChangeRarity(Button.Content.ToString().Replace('_', ' '));
            Current.Rarity = button.Tag.ToEnum<RARITY>();
            RenderCard.Render(Current);
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

        private void CirculationExpander_Expanded(object sender, RoutedEventArgs e)
        {

            Grid.Height += 172 - 40;
            SynchroAnimation(CirculationBorder, 172, Border.HeightProperty);

            TextBlock Caption = (CirculationExpander.Header as Canvas).FindChild<TextBlock>("CirculationCaption");
            SynchroAnimation(Caption, (CirculationBorder.Width - Caption.ActualWidth) / 2 - 32, Canvas.LeftProperty);

            Scroll.ScrollToEnd();

        }

        private void CirculationExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 172 - 40;
            SynchroAnimation(CirculationBorder, 40, Border.HeightProperty);

            TextBlock Caption = (CirculationExpander.Header as Canvas).FindChild<TextBlock>("CirculationCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void NameExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 190 - 40;
            SynchroAnimation(NameBorder, 190, Border.HeightProperty);

            TextBlock Caption = (NameExpander.Header as Canvas).FindChild<TextBlock>("NameCaption");
            SynchroAnimation(Caption, 148, Canvas.LeftProperty);

            TextBox Edit = (NameExpander.Header as Canvas).FindChild<TextBox>("NameEdit");
            Edit.Focus();
            Edit.SelectAll();
            SynchroAnimation(Edit, 20, Canvas.TopProperty);
            SynchroAnimation(Edit, 2, Canvas.LeftProperty);
        }

        private void NameExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 190 - 40;
            SynchroAnimation(NameBorder, 40, Border.HeightProperty);

            TextBlock Caption =( NameExpander.Header as Canvas).FindChild<TextBlock>("NameCaption");
            
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);

            TextBox Edit = (NameExpander.Header as Canvas).FindChild<TextBox>("NameEdit");
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


                TextBlock Caption = (AttributeExpander.Header as Canvas).FindChild<TextBlock>("AttributeCaption");
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


                TextBlock Caption = (AttributeExpander.Header as Canvas).FindChild<TextBlock>("AttributeCaption");
                SynchroAnimation(Caption, 2, Canvas.LeftProperty);
            }
            //SynchroAnimation(MiddleBorder, 195, Border.WidthProperty);

        }


        private void LevelExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 210 - 40;
            {
                SynchroAnimation(MiddleBorder, 40, Border.HeightProperty);
                //SynchroAnimation(MiddleBorder, 195, Border.WidthProperty);

                TextBlock Caption = (LevelExpander.Header as Canvas).FindChild<TextBlock>("LRCaption");
                SynchroAnimation(Caption, 2, Canvas.LeftProperty);
            }
            // SynchroAnimation(AttributeBorder, 195, Border.WidthProperty);

        }

        private void LevelExpander_Expanded(object sender, RoutedEventArgs e)
        {
            TextBlock Caption = (LevelExpander.Header as Canvas).FindChild<TextBlock>("LRCaption");
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

            TextBlock Caption = (FrameExpander.Header as Canvas).FindChild<TextBlock>("FrameCaption");
            SynchroAnimation(Caption, 134, Canvas.LeftProperty);


            /*
            Image Frame = Uti.FindChild<Image>(Card, "Frame");
            for (int i = 0; i < Frame_String.Length; i++)
            {
                Button Z = Uti.FindChild<Button>(FrameCanvas, Frame_String[i]);
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

            TextBlock Caption = (FrameExpander.Header as Canvas).FindChild<TextBlock>("FrameCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void TypeExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 294 - 40;
            //Scroll.ScrollToVerticalOffset(Grid.Height - Scroll.Height - (294 - 40));
            SynchroAnimation(TypeBorder, 294, Border.HeightProperty);

            TextBlock Caption = (TypeExpander.Header as Canvas).FindChild<TextBlock>("TypeCaption");
            SynchroAnimation(Caption, 122, Canvas.LeftProperty);
        }

        private void TypeExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 294 - 40;
            SynchroAnimation(TypeBorder, 40, Border.HeightProperty);

            TextBlock Caption = (TypeExpander.Header as Canvas).FindChild<TextBlock>("TypeCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);

        }

        private void AbilityExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 152 - 40;
            SynchroAnimation(AbilityBorder, 40, Border.HeightProperty);

            TextBlock Caption = (AbilityExpander.Header as Canvas).FindChild<TextBlock>("AbilityCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }

        private void AbilityExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 152 - 40;
            //Scroll.ScrollToVerticalOffset(Grid.Height - Scroll.Height);
            SynchroAnimation(AbilityBorder, 152, Border.HeightProperty);

            TextBlock Caption = (AbilityExpander.Header as Canvas).FindChild<TextBlock>("AbilityCaption");
            SynchroAnimation(Caption, 142, Canvas.LeftProperty);
        }

        private void ScaleExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.Height += 190 - 40;
            SynchroAnimation(ScaleBorder, 190, Border.HeightProperty);

            TextBlock Caption = (ScaleExpander.Header as Canvas).FindChild<TextBlock>("ScaleCaption");

            SynchroAnimation(Caption, (ScaleBorder.Width - Caption.ActualWidth) / 2 - 34, Canvas.LeftProperty);
            Scroll.ScrollToEnd();
        }

        private void ScaleExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= 190 - 40;
            //ScaleBorder.MinHeight = 40;
            SynchroAnimation(ScaleBorder, 40, Border.HeightProperty);

            TextBlock Caption = (ScaleExpander.Header as Canvas).FindChild<TextBlock>("ScaleCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
        }




        private void ScaleLeft_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if (InChangeScale)
            {
                InChangeScale = false;
                return;
            }


            if (!IsScaleAllowed((sender as TextBox).Text))
            {
                InChangeScale = true;
                MessageBox.Show("Dữ liệu không hợp lệ!");
                BoxScaleLeft.Text = BoxScaleLeft.Tag as string;
            }
            else
            {

                BoxScaleLeft.Tag = BoxScaleLeft.Text;
                Button PendulumButton = Uti.FindChild<Button>(FrameCanvas, "Pendulum");
                if (!Current.IsFrame("Pendulum"))
                    Uti.PerformClick(PendulumButton);

                BoxScaleLeft.Text = BoxScaleLeft.Tag.ToString();
                if (String.IsNullOrEmpty(BoxScaleRight.Text))
                {

                    BoxScaleRight.Text = "1";
                }

                ChangeScale(BoxScaleLeft.Text + "/" + BoxScaleRight.Text);
                ChangeScale(BoxScaleLeft.Text + "/" + BoxScaleRight.Text);


            }

            Refresh(); */
        }

        private void ScaleRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if (InChangeScale)
            {
                InChangeScale = false;
                return;
            }

            if (!IsScaleAllowed((sender as TextBox).Text))
            {
                InChangeScale = true;
                MessageBox.Show("Dữ liệu không hợp lệ!");
                BoxScaleRight.Text = BoxScaleRight.Tag as string;
            }
            else
            {

                BoxScaleRight.Tag = BoxScaleRight.Text;
                Button PendulumButton = Uti.FindChild<Button>(FrameCanvas, "Pendulum");
                if (!Current.IsFrame("Pendulum"))
                    Uti.PerformClick(PendulumButton);

                BoxScaleRight.Text = BoxScaleRight.Tag.ToString();
                if (String.IsNullOrEmpty(BoxScaleLeft.Text))
                {

                    BoxScaleLeft.Text = "1";

                }


                ChangeScale(BoxScaleLeft.Text + "/" + BoxScaleRight.Text);
                ChangeScale(BoxScaleLeft.Text + "/" + BoxScaleRight.Text);


            }

            Refresh(); */
        }

        void CardText_Click(object sender, RoutedEventArgs e)
        {
            //CallEditor((sender as Button).Content.ToString());
        }

        void PendulumEffect_Click(object sender, RoutedEventArgs e)
        {
            //CallEditor((sender as Button).Content.ToString());
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

    }
}
