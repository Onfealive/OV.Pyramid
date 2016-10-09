using System.Windows;
using System.Windows.Documents;
using OV.Tools;
using System.Windows.Input;
using System.Windows.Controls;
using OV.Core;
using System.Windows.Media;
using static OV.Tools.Animations;
using static OV.Tools.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

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
        private YGOCard Current
        {
            get
            {
                if (currentIndex < 0) return null;                
                return Set.Cards[currentIndex];
                

            }
            set {
                if (currentIndex < 0) return;
                Set.Cards[currentIndex] = value;
                
            }
        }
        
        

        CustomFileExtensionControl.CustomFileExtension DefaultExtension
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

        

        public MainWindow()
        {
            InitializeComponent();
            Set.Cards.Add(YGOCard.Default);
            Set.Cards.Add(YGOCard.Default);
            
            FirstLoad();
            this.Loaded += new RoutedEventHandler(MainContainer_Loaded);
            //DefaultExtension.RegisterFileType();
            //DefaultExtension.RemoveFileType();
            
            cardList.ItemsSource = Set.Cards;
            cardList.SelectedIndex = 0;
            
        }

        void MainContainer_Loaded(object sender, RoutedEventArgs e)

        {
            //MessageBox.Show("here");
            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string fname = Application.Current.Properties["ArbitraryArgName"].ToString();                
                RenderCard.Load(fname);
            }

        }

        private void FirstLoad()
        {
            
            //Set default global Style for Paragraph: Margin = 0            
            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));
            Resources.Add(typeof(Paragraph), style);

            
            RenderCard.SetDefaultArtwork(Utilities.GetLocationPath() + @"\Template\NoneImage.png");
            
            
            
            //RenderCard.Render(Current);

            LoadControl();
            LoadButton();
        }

        private void Refresh()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Set.Cards);
            view.Refresh();
            if (Current == null) { return; }
            RenderCard.Render(Current);
        }

        void LoadButton()
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
            RenderCard.Load();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            RenderCard.Save();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
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
            ABILITY ability = (sender as Button).Tag.ToEnum<ABILITY>();
            if (Current.Abilities.Contains(ability))
            {
                Current.SetAbility(ability, false);
            } else
            {
                Current.SetAbility(ability, true);
            }

            RenderCard.Render(Current);
        }

        

        private void DEF_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetDEF(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);
        }

        private void ATK_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetATK(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);

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

            Canvas.SetTop(BoxScaleLeft, Canvas.GetTop(InLeft) + 40);
            Canvas.SetLeft(BoxScaleLeft, Canvas.GetLeft(InLeft) + 20);
            ScaleCanvas.Children.Add(InLeft);
            {
                Image InRight = new Image();
                InRight.Source = Images.GetImage(GetLocationPath() + @"\Template\Middle\ScaleRight.png");
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
            Current.SetScaleLeft( int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            Current.SetScaleRight( int.TryParse(button.Tag.ToString(), out value) ? value : double.NaN);
            RenderCard.Render(Current);
        }

        private void LoadMiddleControl()
        {
            Canvas Panel;
            TextBlock Caption;

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
                        { Canvas.SetTop(Button, top);
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

                        Button.Name = type.Replace(' ', '_') + "_" + i.ToString();
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
                    IconMonster.Source = Images.GetImage(GetLocationPath() + "/Attribute/" + Magic[count - 1] + ".png");
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
                            Button.Name = "Spell_" + Property_String[i - 1].Remove("-");
                        else if (Magic[count - 1] == "TRAP")
                            Button.Name = "Trap_" + Property_String[i - 1].Remove("-");

                        Button.Width = 106;
                        Button.Click += Button_Property;

                        Canvas.SetTop(Button, 34 + ((i - 1) / 3) * 33 + (count - 1) * 90);
                        Canvas.SetLeft(Button, 44 + ((i - 1) % 3) * (Button.Width + 4) - 32);
                        //    Uti.SetCanvas(Button, 40, 22 + ((i - 1) % 3) * (Button.Width + 4));


                        StackPanel Zone = new StackPanel();

                        Zone.Orientation = Orientation.Horizontal;
                        Button.Content = Zone;
                        Image Icon = new Image();
                        Icon.Source = Images.GetImage(GetLocationPath() + "/Property/" + Property_String[i - 1] + ".png");
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
            string Frame = (sender as Button).Name.Split('_')[0];
            string Property = (sender as Button).Name.Split('_')[1];            

            Current.SetProperty(Frame == "Spell" ? FRAME.Spell : FRAME.Trap, Property.ToEnum<PROPERTY>());
            RenderCard.Render(Current);
        }
    

        private void Button_LR(object sender, RoutedEventArgs e)
        {
            string type = (sender as Button).Name.Split('_')[0];
            int number = Int32.Parse((sender as Button).Name.Split('_')[1]);

            if (type == "Level")
            {
                Current.SetLevel(number);                
                Current.SetRank(double.NaN, false);
            } else
            {
                Current.SetRank(number);
                Current.SetLevel(double.NaN, false);
            }

            RenderCard.Render(Current);
        }

        private void LoadAttributeControl()
        {
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            Caption.Name = "AttributeCaption";
            Caption.Text = "Attribute";
            Caption.FontSize = 14;
            Canvas.SetLeft(Caption, 4);
            Panel.Children.Add(Caption);
            AttributeExpander.Header = Panel;
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
            Current.SetAttribute((sender as Button).Tag.ToEnum<ATTRIBUTE>());
            RenderCard.Render(Current);
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

                Button.Name = Frame_String[i - 1];
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
            string frameName = (sender as Button).Name;
            if (frameName == "Pendulum")
            {
                Current.SetPendulum(!Current.IsPendulum);
            }
            else
            {
                Current.SetFrame(frameName.ToEnum<FRAME>());
            }          
            RenderCard.Render(Current);
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
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
            Canvas Panel = new Canvas();
            TextBlock Caption = new TextBlock();
            TextBox Edit = new TextBox();
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

        private void Button_Type(object sender, RoutedEventArgs e)
        {
            Current.SetType( (sender as Button).Tag.ToEnum<TYPE>());
            RenderCard.Render(Current);
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
            CardNumber.Text = t.Next(0, 99999999 + 1).ToString().PadLeft(8, '0');
            
        }

        private void Edition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Text = (sender as ComboBox).SelectedValue.ToString();
            Current.Edition = Text.Remove(" ").Replace("1st", "First").ToEnum<EDITION>();            
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
            //cardList.ItemsSource = null;
            //cardList.ItemsSource = Set.Cards;
            
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

        private void DocumentExpander_Expanded(object sender, RoutedEventArgs e)
        {

            Grid.Height += DocumentCanvas.Height - 40;
            SynchroAnimation(DocumentBorder, DocumentCanvas.Height, Border.HeightProperty);

            TextBlock Caption =  (DocumentExpander.Header as Canvas).FindChild<TextBlock>("DocumentCaption");
            SynchroAnimation(Caption, (DocumentBorder.Width - Caption.ActualWidth) / 2 - 32, Canvas.LeftProperty);
        }

        private void DocumentExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.Height -= DocumentCanvas.Height - 40;
            SynchroAnimation(DocumentBorder, 40, Border.HeightProperty);

            TextBlock Caption = (DocumentExpander.Header as Canvas).FindChild<TextBlock>("DocumentCaption");
            SynchroAnimation(Caption, 2, Canvas.LeftProperty);
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
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetScaleLeft(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);
        }

        private void ScaleRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            int value;

            Current.SetScaleRight(int.TryParse(txtBox.Text, out value) ? value : double.NaN);
            RenderCard.Render(Current);
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

        private void Description_Click(object sender, RoutedEventArgs e)
        {            
            DescriptionButton.IsEnabled = false;
            PendulumEffectButton.IsEnabled = true;
            
            DocumentBox.Document.Blocks.Clear();
            DocumentBox.AppendText(Current.Description); 
        }

        private void ApplyDocument_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(DocumentBox.Document.ContentStart, DocumentBox.Document.ContentEnd);

            string Text = @textRange.Text;
            Text = Text.ReplaceLastOccurrence(Environment.NewLine, "");

            if (DescriptionButton.IsEnabled == false) //Current is Description
            {
                Current.Description = Text;
            } else
            {
                Current.SetPendulumEffect(Text);
            }
            
            RenderCard.Render(Current);
        }
        
        private void cardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentIndex = cardList.SelectedIndex;
            Refresh();
            ControlArtwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
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
            string Text = (sender as TextBox).Text;
            if (File.Exists(Text))
            {
                Current.ArtworkByte = Images.GetImageByte(Text);
                ControlArtwork.Source = Images.GetImage(Text);
                Refresh();
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
}
