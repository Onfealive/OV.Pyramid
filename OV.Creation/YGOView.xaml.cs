using Newtonsoft.Json;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using static OV.Tools.Utilities;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for YGOView.xaml
    /// </summary>
    public partial class YgoView : UserControl, INotifyPropertyChanged
    {
        internal YgoCard CurrentCard { get; private set; }
        private static string DatabasePath;

        private ByteDatabase Database;
        private byte[] NoneImageArray;
        private bool isInitialize;

        public YgoView()
        {
            InitializeComponent();
            DatabasePath = GetLocationPath() + @"\Resources\Datas.ld";
            Database = new ByteDatabase(DatabasePath);
            isInitialize = false;

            foreach (Border border in ControlZone.FindChildrens<Border>())
            {
                border.MouseEnter += ControlZone_MouseEnter;
                border.MouseLeave += ControlZone_MouseLeave;
            }
        }

        public event EventHandler ValueClick;        
        public event EventHandler AbilityClick;
        public event EventHandler ArtworkClick;
        public event EventHandler AttributeClick;
        public event EventHandler CirculationClick;
        public event EventHandler DescriptionClick;
        public event EventHandler FrameClick;
        public event EventHandler MiddleClick;
        public event EventHandler NameClick;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class Fonts
        {
            public static FontFamily StoneSerifLTBold
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#StoneSerif LT Bold");
                }
            }
            public static FontFamily StoneSerifRegular
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Stone Serif");
                }
            }
            public static FontFamily MatrixBook
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Matrix-Book");
                }
            }
            public static FontFamily YGOMatrixSmallCap2
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Yu-Gi-Oh! Matrix Small Caps 2");
                }
            }

            public static FontFamily StoneSerifItalic
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Stone Serif");
                }
            }
            public static FontFamily PalatinoLinotype
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Palatino Linotype");
                }
            }
            public static FontFamily BankGothicMdBT
            {
                get
                {
                    return new FontFamily(new Uri("pack://application:,,,/"), "./resources/#BankGothic Md BT");
                }
            }
        }

        private void SetDefaultValue()
        {
            if (isInitialize) { return; }
            isInitialize = true;
            CardName.FontFamily = Fonts.YGOMatrixSmallCap2;
            Ability.FontFamily = BracketLeft.FontFamily = BracketRight.FontFamily = Fonts.StoneSerifLTBold;
            ATKBlock.FontFamily = DEFBlock.FontFamily = Fonts.YGOMatrixSmallCap2;
            Creator.FontFamily = Fonts.StoneSerifRegular;
            CardNumber.FontFamily = Fonts.StoneSerifRegular;
            SetNumber.FontFamily = Fonts.StoneSerifRegular;
            AttributeText.FontFamily = Fonts.PalatinoLinotype;
            ScaleLeftBlock.FontFamily = ScaleRightBlock.FontFamily = Fonts.YGOMatrixSmallCap2;
            Pendulum.FontFamily = Fonts.MatrixBook;
            //Pendulum.FontStyle = FontStyles.Normal;

            CardBorder.Source = Database.GetImage(@"Template\Border\Card\Default.png");
            ArtworkBorder.Source = Database.GetImage(@"Template\Border\Artwork\Default.png");
            LoreBorder.Source = Database.GetImage(@"Template\Border\Lore\Default.png");
            ScaleLeft.Source = Database.GetImage(@"Template\Middle\ScaleLeft.png");
            ScaleRight.Source = Database.GetImage(@"Template\Middle\ScaleRight.png");
        }

        public void SetDefaultArtwork(byte[] imageBytes)
        {
            NoneImageArray = imageBytes;
            if (CurrentCard != null)
            {
                CurrentCard.ArtworkByte = imageBytes;
            }
        }

        private void RenderAll()
        {
            HandleName();
            HandleAttribute();
            HandleMiddle();
            HandleArtwork();
            HandleAD();
            HandleAbility();
            HandleCreator();
            HandleDescription();
            HandlePendulum();
            HandleFrame();
            HandleScale();
            HandleRarity();
            HandleSticker();
            HandleEdition();
            HandleCartNumber();
            HandleSetCard();
        }

        internal void Render(YgoCard card)
        {
            SetDefaultValue();
            YgoCard preCard = this.CurrentCard != null ? this.CurrentCard.Clone() : null;
            this.CurrentCard = card.Clone();

            if (preCard == null)
            {
                RenderAll();
            }
            else
            {
                if (preCard.Name != CurrentCard.Name)
                {
                    HandleName();
                }

                if (preCard.Name != CurrentCard.Name
                    || preCard.Rarity != CurrentCard.Rarity
                    || preCard.Frame != CurrentCard.Frame)
                {
                    HandleNameRarity();
                }

                if (preCard.Attribute != CurrentCard.Attribute
                    || preCard.Rarity != CurrentCard.Rarity)
                {
                    HandleAttribute();
                }
                if (preCard.Level.CompareTo(CurrentCard.Level) != 0
                    || preCard.Rank.CompareTo(CurrentCard.Rank) != 0
                    || preCard.Property != CurrentCard.Property
                    || preCard.Rarity != CurrentCard.Rarity)
                {
                    HandleMiddle();
                }

                if (preCard.ArtworkByte.SequenceEqual(CurrentCard.ArtworkByte) == false)
                    HandleArtwork();

                if (preCard.ATK.CompareTo(CurrentCard.ATK) != 0
                    || preCard.DEF.CompareTo(CurrentCard.DEF) != 0)
                {
                    HandleAD();
                }
                if (preCard.Abilities.ListEquals(CurrentCard.Abilities) == false
                    || preCard.Type != CurrentCard.Type
                    || (CurrentCard.IsMagic() && preCard.IsMonster())
                    || (CurrentCard.IsMonster() && preCard.IsMagic())
                    || (CurrentCard.IsFrame(FRAME.Normal) != preCard.IsFrame(FRAME.Normal)))
                {
                    HandleAbility();
                }

                if (CurrentCard.Description != preCard.Description
                    || (CurrentCard.IsMagic() && preCard.IsMonster())
                    || (CurrentCard.IsMonster() && preCard.IsMagic())
                    || (CurrentCard.IsFrame(FRAME.Normal) != preCard.IsFrame(FRAME.Normal))
                    )
                {
                    HandleDescription();

                }

                if (preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.PendulumEffect != CurrentCard.PendulumEffect
                    || (preCard.IsPendulum && preCard.Frame != CurrentCard.Frame))
                {
                    HandlePendulum();
                }

                if (preCard.Frame != CurrentCard.Frame
                    || preCard.Equals(YgoCard.Default))
                {
                    HandleFrame();
                }
                if (preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.ScaleLeft.Equals(CurrentCard.ScaleLeft) == false
                    || preCard.ScaleRight.Equals(CurrentCard.ScaleRight) == false)
                {
                    HandleScale();
                }

                if (preCard.Rarity != CurrentCard.Rarity)
                {
                    HandleRarity();
                }
                //Circulation
                if (preCard.Creator != CurrentCard.Creator
                    || preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.Frame != CurrentCard.Frame)
                {
                    HandleCreator();
                }
                if (preCard.Sticker != CurrentCard.Sticker)
                {
                    HandleSticker();
                }
                if (preCard.Edition != CurrentCard.Edition
                    || preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.Frame != CurrentCard.Frame)
                {
                    HandleEdition();
                }
                if (preCard.Number != CurrentCard.Number
                    || preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.Frame != CurrentCard.Frame)
                {
                    HandleCartNumber();
                }
                if (preCard.Set != CurrentCard.Set
                    || preCard.IsPendulum != CurrentCard.IsPendulum
                    || preCard.Frame != CurrentCard.Frame)
                {
                    HandleSetCard();
                }
            }
            //this.OnPropertyChanged(this, "RenderedImageSource");
            //return RenderedImageSource;
            RefreshZone();
        }

        private void RefreshZone()
        {
            AbilityZone.Visibility = CurrentCard.IsMonster() ? Visibility.Visible : Visibility.Hidden;
        }

        private void HandlePendulum()
        {
            if (CurrentCard == null) { return; }

            string Text = CurrentCard.PendulumEffect ?? "";
            double fontDefault = 31;

            Pendulum.Inlines.Clear();
            Pendulum.Text = Text;
            Pendulum.FontSize = fontDefault;

            /*
            
            Pendulum.LineHeight = fontDefault * 1;
            
            while (Pendulum.FontSize * Pendulum.GetLines().Count() > Pendulum.MaxHeight)
            {
                Pendulum.FontSize -= 0.1;                
            }

            if (Pendulum.GetLines().Count() != 4)
            {
                Pendulum.LineHeight = 0.95 * Pendulum.FontSize;
            }
            else
            {
                Pendulum.LineHeight = 1.04 * Pendulum.FontSize;
            } */

            Pendulum.MaxFontSize = fontDefault;
            Pendulum.ScaledLineHeight = 1;
            if (Pendulum.GetLines().Count() < 4)
            {
                Canvas.SetTop(Pendulum, 785);
                Pendulum.Height = 90;
            }
            else
            {
                Canvas.SetTop(Pendulum, 746);
                Pendulum.Height = 133;
            }

            Pendulum.MaxHeight = Pendulum.Height;
            Pendulum.MaxWidth = Pendulum.Width;



            //Is Pendulum?
            if (CurrentCard.IsPendulum)
            {
                LoreBorder.Visibility = Visibility.Hidden;
                if (Pendulum.GetLines().Count() < 4)
                {
                    ArtworkBorder.Source = Database.GetImage(@"Template\Border\Pendulum\Small.png");
                    PendulumBoxMiddle.Source = Database.GetImage(@"Template\Border\Pendulum\SmallBox.png");
                    PendulumBoxText.Source = Database.GetImage(@"Template\Border\Pendulum\TextBox.png");
                }
                else
                {
                    ArtworkBorder.Source = Database.GetImage(@"Template\Border\Pendulum\Medium.png");
                    PendulumBoxMiddle.Source = Database.GetImage(@"Template\Border\Pendulum\MediumBox.png");
                    PendulumBoxText.Source = Database.GetImage(@"Template\Border\Pendulum\TextBox.png");
                }
                PendulumLine.Source = Database.GetImage(@"Template\Border\Pendulum\Line" + CurrentCard.Frame.ToString() + ".png");
                SpellHalf.Source = Database.GetImage(@"Template\Border\Pendulum\SpellHalf.png");
                LoreBackground.Source = Database.GetImage(@"Template\Border\Pendulum\LoreBoxMedium.png");
                Artwork.Width = 707;
                Artwork.Height = 663;
                Canvas.SetLeft(Artwork, 53);
                Canvas.SetTop(Artwork, 212);
                //Set Card
                Canvas.SetLeft(SetNumber, 68);
                Canvas.SetRight(SetNumber, double.NaN);
                Canvas.SetTop(SetNumber, 1083);
                SetNumber.TextAlignment = TextAlignment.Left;
            }
            else
            {
                ArtworkBorder.Source = Database.GetImage(@"Template\Border\Artwork\Default.png");
                LoreBorder.Visibility = Visibility.Visible;
                PendulumBoxMiddle.Source = PendulumBoxText.Source = null;
                PendulumLine.Source = null;
                SpellHalf.Source = null;
                LoreBackground.Source = null;
                Artwork.Width = 620;
                Artwork.Height = 620;
                Canvas.SetLeft(Artwork, 98);
                Canvas.SetTop(Artwork, 217);
                //Set Card
                //Canvas.Right = "82" Canvas.Top = "849"
                Canvas.SetRight(SetNumber, 82);
                Canvas.SetLeft(SetNumber, double.NaN);
                Canvas.SetTop(SetNumber, 849);
                SetNumber.TextAlignment = TextAlignment.Left;
            }
        }

        private void HandleScale()
        {
            if (CurrentCard.IsPendulum == false)
            {
                ScaleLeftBlock.Text = ScaleRightBlock.Text = "";
                ScaleLeft.Visibility = ScaleRight.Visibility = Visibility.Hidden;
                return;
            }
            ScaleLeft.Visibility = ScaleRight.Visibility = Visibility.Visible;
            ScaleLeftBlock.Text = double.IsNaN(CurrentCard.ScaleLeft) ? "?" : CurrentCard.ScaleLeft.ToString();
            ScaleRightBlock.Text = double.IsNaN(CurrentCard.ScaleRight) ? "?" : CurrentCard.ScaleRight.ToString();

            if (Pendulum.GetLines().Count() < 4)
            {
                Canvas.SetTop(ScaleLeftBlock, 824);
                Canvas.SetTop(ScaleRightBlock, 824);
            }
            else
            {
                Canvas.SetTop(ScaleLeftBlock, 804);
                Canvas.SetTop(ScaleRightBlock, 804);
            }


            //ScaleLeft
            if (!double.IsNaN(CurrentCard.ScaleLeft))
            {
                if (CurrentCard.ScaleLeft >= 10)
                {
                    if (CurrentCard.ScaleLeft == 11)
                    {
                        Canvas.SetLeft(ScaleLeftBlock, 74 - 11);
                    }
                    else
                    {
                        Canvas.SetLeft(ScaleLeftBlock, 74 - 14);
                    }
                }
                else
                {
                    Canvas.SetLeft(ScaleLeftBlock, 74);
                }
            }
            else
            {
                Canvas.SetLeft(ScaleLeftBlock, 74);
            }
            //Scale Right
            if (!double.IsNaN(CurrentCard.ScaleRight))
            {
                if (CurrentCard.ScaleRight >= 10)
                {
                    if (CurrentCard.ScaleRight == 11)
                    {
                        Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 32);
                    }
                    else
                    {
                        Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 35);
                    }
                }
                else
                {
                    Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 23);
                }
            }
            else
            {
                Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 22);
            }
            Canvas.SetTop(ScaleLeft, ScaleLeftBlock.Top() - 30);
            Canvas.SetTop(ScaleRight, ScaleRightBlock.Top() - 30);
        }

        private void HandleSetCard()
        {
            if (CurrentCard.Set == null) { return; }
            SetNumber.Text = CurrentCard.Set.ToUpper();
            if (CurrentCard.IsFrame(FRAME.Xyz) && !CurrentCard.IsPendulum)
            {
                SetNumber.Foreground = Brushes.White;
            }
            else
            {
                SetNumber.Foreground = Brushes.Black;
            }
        }

        private void HandleCartNumber()
        {
            if (CurrentCard.Number > 0)
            {
                string number = CurrentCard.Number.ToString().PadLeft(8, '0');
                CardNumber.Text = number;
                if (CardNumber.GetFormattedText().Width > 109)
                {
                    CardNumberViewbox.Width = 109;
                }
                else
                {
                    CardNumberViewbox.Width = double.NaN;
                }
            }
            else
            {
                CardNumber.Text = null;
                CardNumberViewbox.Width = double.NaN;
            }

            if (CurrentCard.IsFrame(FRAME.Xyz) && !CurrentCard.IsPendulum)
            {
                CardNumber.Foreground = Brushes.White;
            }
            else
            {
                CardNumber.Foreground = Brushes.Black;
            }
        }

        private void HandleEdition()
        {
            if (CurrentCard.IsFrame(FRAME.Xyz) && !CurrentCard.IsPendulum)
            {
                Edition.Foreground = Brushes.White;
            }
            else
            {
                Edition.Foreground = Brushes.Black;
            }

            if (CurrentCard.Edition == EDITION.UnlimitedEdition)
            {
                Edition.Text = null;
            }
            else if (CurrentCard.Edition == EDITION.FirstEdition)
            {
                Edition.FontFamily = Fonts.PalatinoLinotype;
                Edition.FontSize = 24;
                Edition.Inlines.Clear();
                Edition.Inlines.Add(new Run("1"));
                Run Superscript = new Run("st");
                Superscript.Typography.Variants = FontVariants.Superscript;
                Edition.Inlines.Add(Superscript);
                Edition.Inlines.Add(new Run(" Edition"));
                Edition.FontWeight = FontWeights.UltraBlack;
            }
            else if (CurrentCard.Edition == EDITION.LimitedEdition)
            {
                Edition.FontFamily = Fonts.PalatinoLinotype;
                Edition.FontSize = 24;
                Edition.FontWeight = FontWeights.SemiBold;
                Edition.Text = CurrentCard.Edition.ToString().AddSpaceBetweenCapital().ToUpper();
            }
            else if (CurrentCard.Edition == EDITION.DuelTerminal)
            {
                Edition.FontFamily = Fonts.BankGothicMdBT;
                Edition.Text = CurrentCard.Edition.ToString().AddSpaceBetweenCapital().ToUpper();
                Edition.FontWeight = FontWeights.Normal;
                Edition.FontSize = 30;
            }
        }

        private void HandleAD()
        {
            if (CurrentCard.IsMonster())
            {
                /*
                if (vietnameseLang.IsChecked == true)
                {
                    ATKText.SetRight(322);
                    DEFText.SetRight(148);

                    ATKText.Text = "CÔNG/";
                    DEFText.Text = "THỦ/";
                    ATKText.FontSize = DEFText.FontSize = 44.5;
                    Canvas.SetTop(ATK, 1072.5);
                    Canvas.SetTop(DEF, 1072.5);
                }
                else */
                {
                    ATKBlock.SetLeft(431);
                    DEFBlock.SetLeft(600);

                    ATKBlock.Text = "ATK/";
                    DEFBlock.Text = "DEF/";
                    ATKBlock.FontSize = DEFBlock.FontSize = 46;
                    Canvas.SetTop(ATKBlock, 1072);
                    Canvas.SetTop(DEFBlock, 1072);
                }
                /*
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 1;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = 90;
                shadow.ShadowDepth = 0.5;
                shadow.Color = Colors.Black;

                AD.Effect = shadow;
                */


                //if (!Double.IsNaN(Current.ATK))
                {
                    ATKBlock.Text += SpecialPoint(CurrentCard.ATK);
                    //Canvas.SetLeft(ATKBlock, ATKBlock.Left() - ATKBlock.GetFormattedText(SpecialPoint(Current.ATK)).Width);
                    //MessageBox.Show(ATKBlock.Left().ToString());
                }
                //if (!Double.IsNaN(Current.DEF))
                {
                    DEFBlock.Text += SpecialPoint(CurrentCard.DEF);

                    //Canvas.SetLeft(DEFBlock, DEFBlock.Left() - DEFBlock.GetFormattedText(SpecialPoint(Current.DEF)).Width);
                }
            }
            else
            {
                ATKBlock.Text = DEFBlock.Text = "";
            }
        }

        private void HandleSticker()
        {
            if (CurrentCard == null) { return; }

            Sticker.Source = Database.GetImage(@"Template\Sticker\" + CurrentCard.Sticker.ToString() + ".png");
        }

        private void HandleCreator()
        {
            if (CurrentCard == null) { return; }
            if (CurrentCard.IsFrame(FRAME.Xyz) && CurrentCard.IsPendulum == false)
            {
                Creator.Foreground = Brushes.White;
            }
            else
            {
                Creator.Foreground = Brushes.Black;
            }
            Creator.Text = CurrentCard.Creator == CREATOR.KazukiTakahashi ? "© 1996 KAZUKI TAKAHASHI" : "";
        }

        public void Save()
        {
            string text = JsonConvert.SerializeObject(CurrentCard, Formatting.Indented);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            if (CurrentCard.Name == null)
                dlg.FileName = "Card Name";
            else
                dlg.FileName = CurrentCard.Name.Replace(":", " -"); // Default file name
            //dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "OV.Creation Card|*.occ"; // Filter files by extension 
            // Process save file dialog box results 
            if (dlg.ShowDialog() == true)
            {
                // Save document 
                File.WriteAllText(dlg.FileName, text);
            }
        }

        internal void Load(YgoCard card)
        {
            //Current.CleanUp();
            CurrentCard = card;
            Render(CurrentCard);
            //Render(Current);
        }               

        internal void SaveImageTo(string fileName)
        {            
            CreateBitmapFromVisual(CardCanvas, fileName);
        }

        internal void SaveImageTo(YgoCard card, string fileName)
        {
            
            CreateBitmapFromVisual(CardCanvas, fileName);
            //Render(origin);
        }

        public void SaveDataTo(string fileName)
        {
            string text = JsonConvert.SerializeObject(CurrentCard, Formatting.Indented);
            File.WriteAllText(fileName, text);
        }

        internal void SaveDataTo(YgoCard card, string fileName)
        {
            string text = JsonConvert.SerializeObject(card, Formatting.Indented);
            File.WriteAllText(fileName, text);
        }

        public void CallExport()
        {
            if (CurrentCard == null)
            {
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            //dlg.FileName = NameEdit.Text.Replace(":", " -"); // Default file name
            if (String.IsNullOrEmpty(dlg.FileName))
                dlg.FileName = CurrentCard.Name != null ? CurrentCard.Name.ToNonVietnamese() : "Card Name";
            //dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "JPEG Images|*.jpg|PNG Images|*.png|BMP Images|*.bmp"; // Filter files by extension 

            // Show save file dialog box

            // Process save file dialog box results 
            if (dlg.ShowDialog() == true)
            {
                // Save document 
                CreateBitmapFromVisual(CardCanvas, dlg.FileName);
            }

        }

        private void CreateBitmapFromVisual(Visual target, string fileName)
        {            
            if (target == null || string.IsNullOrEmpty(fileName))
            {
                return;
            }
            
            BitmapSource source = null;
            if (target == CardCanvas)
            {
                source = RenderedImageSource as BitmapSource;
            }
            else
            {

                source = (target as Image).Source as BitmapSource;
            }
            RenderOptions.SetBitmapScalingMode(source, BitmapScalingMode.Fant);
            RenderOptions.SetCacheInvalidationThresholdMaximum(source, 100);
            RenderOptions.SetEdgeMode(source, EdgeMode.Aliased);
            TextOptions.SetTextRenderingMode(source, TextRenderingMode.Aliased);
            TextOptions.SetTextFormattingMode(source, TextFormattingMode.Display);

            BitmapEncoder bitmapEncoder = null;
            if (fileName.IsExtension(".png"))
            {
                bitmapEncoder = new PngBitmapEncoder();
            }
            else if (fileName.IsExtension(".jpg"))
            {
                bitmapEncoder = new JpegBitmapEncoder();
            }
            else if (fileName.IsExtension(".bmp"))
            {
                bitmapEncoder = new BmpBitmapEncoder();
            }

            //source = source.CreateResizedImage(813, 1185, 0);
            bitmapEncoder.Frames.Add(BitmapFrame.Create(source));
            
            using (Stream stm = File.Create(fileName))
            {

                bitmapEncoder.Save(stm);
            }


            //Other
            /*
            System.Drawing.Bitmap bit = Graphic.ToBitmap(source);
            using (System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(bit))
            {
                newBitmap.SetResolution(300, 300);
                System.Drawing.Imaging.ImageFormat extension = null;
                if (fileName.IsExtension(".png"))
                {
                    extension = System.Drawing.Imaging.ImageFormat.Png;
                }
                else if (fileName.IsExtension(".jpg"))
                {
                    extension = System.Drawing.Imaging.ImageFormat.Jpeg;
                }
                else if (fileName.IsExtension(".bmp"))
                {
                    extension = System.Drawing.Imaging.ImageFormat.Bmp;
                }
                newBitmap.Save(fileName, extension);
            }
            */
        }

        public ImageSource RenderedImageSource
        {
            get
            {
                CardCanvas.Measure(new Size(CardCanvas.Width, CardCanvas.Height));
                CardCanvas.Arrange(new Rect(new Size(CardCanvas.Width, CardCanvas.Height)));
                CardCanvas.UpdateLayout();
                Visual target = CardCanvas;

                double scale = 310 / 96;
                Size realSize = new Size(813, 1185);
                /*
                double realRatio = realSize.Width / realSize.Height;

                Size setSize = new Size(600, 800);
                double setRatio = setSize.Width / setSize.Height;
                if (setRatio > realRatio)
                {
                    setSize.Width = setSize.Height * realRatio;
                } else
                {
                    setSize.Height = setSize.Width / realRatio;
                }
                realSize = setSize;
                */
                Size newSize = new Size(
                    Math.Round(realSize.Width / scale, MidpointRounding.ToEven), 
                    Math.Round(realSize.Height / scale, MidpointRounding.ToEven));
                RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                    (int)realSize.Width, (int)realSize.Height, scale * 96, scale * 96, PixelFormats.Pbgra32);

                DrawingVisual visual = new DrawingVisual();
                
                using (DrawingContext context = visual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(target);
                    context.DrawRectangle(visualBrush, null, new Rect(new Point(), newSize));
                }

                renderTarget.Render(visual);
                return renderTarget;
            }
        }

        

        private void HandleDescription()
        {
            if (CurrentCard == null) { return; }

            string Text = CurrentCard.Description ?? "";
            
            const double fontDefault = 32.4;            
            TextBlockDescription.Width = 694;
            TextBlockDescription.Inlines.Clear();
            RichTextBoxDescription.Visibility = Visibility.Hidden;
            TextBlockDescription.Visibility = Visibility.Visible;

            TextBlockDescription.FontSize = fontDefault;
            if (CurrentCard.IsFrame(FRAME.Normal))
            {
                Canvas.SetTop(TextBlockDescription, 923); //922 - 153
                TextBlockDescription.Height = 152;

                if (Regex.IsMatch(Text, @"(\(.+?\.\))"))
                {
                    RichTextBoxDescription.Visibility = Visibility.Visible;
                    TextBlockDescription.Visibility = Visibility.Hidden;
                    TextBlockDescription.Visibility = Visibility.Hidden;
                    /* test */
                    Paragraph Para = new Paragraph()
                    {
                        TextAlignment = TextAlignment.Justify,
                        LineStackingStrategy = LineStackingStrategy.BlockLineHeight
                    };

                    //if (Text != null)
                    //Text = Text.Replace("\r\n", Environment.NewLine);
                    RichTextBoxDescription.Document.Blocks.Clear();
                    RichTextBoxDescription.Document.Blocks.Add(Para);

                    string First = Text != null ? (Text.Split('(') != null ? Text.Split('(')[0] : null) : null;
                    //MessageBox.Show(Text.Split('(')[0]);
                    string Second = null;
                    if (Text != null && Text.Contains("(") && Text.Contains(")"))
                    {
                        Second = Text.Substring(Text.IndexOf("("), Text.LastIndexOf(")") + 1 - Text.IndexOf("("));

                    }
                    if (Second == null)
                        First = Text;

                    //MessageBox.Show(First);
                    Para.Inlines.Add(new Run(First));
                    Para.Inlines.Add(new Run(Second));
                    Para.Inlines.ElementAt(0).FontFamily = Fonts.StoneSerifItalic; ;//new FontFamily("Times New Roman Italic");
                    Para.Inlines.ElementAt(0).FontStyle = FontStyles.Italic;
                    Para.Inlines.ElementAt(1).FontFamily = Fonts.MatrixBook;
                    double Default = 27; //29.8-32.4 -- 33.1
                    Para.FontSize = Default;
                    while (Para.FontSize * 1.1 * RichTextBoxDescription.CountLine() > RichTextBoxDescription.Height)
                    {
                        Para.FontSize -= 0.07; //0.1
                        RichTextBoxDescription.Document.Blocks.Clear();
                        RichTextBoxDescription.Document.Blocks.Add(Para);
                    }

                    int Line = RichTextBoxDescription.CountLine();
                    if (Line > 6)
                    {
                        RichTextBoxDescription.Height = 150;
                    }
                    else
                    {
                        RichTextBoxDescription.Height = 153;
                    }
                    double param = 1.1;
                    if (Text != null)
                    {
                        if (Text.IsVietnamese())
                        {
                            param = 1.2;
                        }
                        if (Text.ContainsAny("Ễ"))
                        {
                            param = 1.3;
                        }
                    }
                    Para.LineHeight = param * Para.FontSize;


                    if (Para.Inlines.Count > 1)
                    {
                        Para.Inlines.ElementAt(1).FontSize = Para.Inlines.ElementAt(0).FontSize * 1.15; //29.8-32.4
                    }
                }
                else
                {

                    TextBlockDescription.FontFamily = Fonts.StoneSerifItalic;
                    TextBlockDescription.FontStyle = FontStyles.Italic;
                    //Description.MaxFontSize = 25.57;
                    //Description.ScaledLineHeight = 1.25;
                    TextBlockDescription.MaxHeight = TextBlockDescription.Height;
                    TextBlockDescription.MaxWidth = TextBlockDescription.Width;
                    TextBlockDescription.Text = Text;
                }
            }
            else
            {                
                DescriptionByRichTextBox(Text);
            }            
        }

        private void DescriptionByTextBlock(string Text)
        {
            double fontDefault = 32.4;
            TextBlockDescription.FontFamily = Fonts.MatrixBook;
            TextBlockDescription.FontStyle = FontStyles.Normal;
            //Description.FontSize = fontDefault;

            //Description.LineHeight = fontDefault * 1;


            

            TextBlockDescription.MaxFontSize = fontDefault;
            TextBlockDescription.ScaledLineHeight = 1.02;

            TextBlockDescription.Text = Text;


            /*
            double lineScale = 0.95;
            if (Description.GetLines().Count() > 6)
            {
                lineScale = 0.94;
            }
            Description.LineHeight = lineScale * Description.FontSize;
            */
            TextBlockDescription.FontSize = TextBlockDescription.MaxFontSize;

            /*
            Description.UpdateLayout();
            while (Description.FontSize * Description.GetLines().Count() > Description.Height)
            {
                Description.FontSize -= 0.1;
                Description.UpdateLayout();
            } */
            double lineScale = 0.95;
            while (TextBlockDescription.FontSize * (Text.IsVietnamese()
                ? 1.1 : 1) * TextBlockDescription.GetLines().Count() > TextBlockDescription.Height)
            {
                TextBlockDescription.FontSize -= 0.05; //0.1

                if (TextBlockDescription.GetLines().Count() > 6)
                {
                    lineScale = 0.94;
                }
                TextBlockDescription.LineHeight = lineScale * TextBlockDescription.FontSize;
            }
        }

        private void DescriptionByRichTextBox(string Text)
        {
            if (CurrentCard.IsMonster())
            {
                Canvas.SetTop(TextBlockDescription, 922);
                //RichTextBoxDescription.Height = 155;//160
                RichTextBoxDescription.Height = 158;
            }
            else
            {
                Canvas.SetTop(TextBlockDescription, 891);
                RichTextBoxDescription.Height = 226;//226
                                                    //MessageBox.Show(RichTextBoxDescription.Height.ToString());
                                                    //RichTextBoxDescription.Background = Brushes.Azure;
            }


            //Text.ReplaceLastOccurrence("\n", "");
            this.RichTextBoxDescription.Visibility = Visibility.Visible;
            TextBlockDescription.Visibility = Visibility.Hidden;
            Paragraph Para = new Paragraph();

            Para.TextAlignment = TextAlignment.Justify;
            Para.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            double Default = 32.4;

            RichTextBoxDescription.Document.Blocks.Clear();
            RichTextBoxDescription.Document.Blocks.Add(Para);
            Para.Inlines.Add(new Run(Text));

            Para.FontFamily = Fonts.MatrixBook;
            Para.FontSize = Default;
            // 6-26.5
            while (Default * (CurrentCard.IsFrame(FRAME.Normal) && (Text != "" && Text.IsVietnamese()) ? 1.1 : 1)
                * RichTextBoxDescription.CountLine() > RichTextBoxDescription.Height)
            {
                Default -= 0.1;
                Para = new Paragraph(new Run(Text));
                Para.TextAlignment = TextAlignment.Justify;
                Para.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                Para.FontFamily = Fonts.MatrixBook;
                Para.FontSize = Default;
                RichTextBoxDescription.Document.Blocks.Clear();
                RichTextBoxDescription.Document.Blocks.Add(Para);
            }


            int Line = RichTextBoxDescription.CountLine();
            Para.LineHeight = (CurrentCard.IsFrame(FRAME.Normal) ? 1.1 : 1) * Para.FontSize;
            if (CurrentCard.IsFrame(FRAME.Normal))
                Para.LineHeight = ((Text != null && Text.IsVietnamese()) ? 1.1 : 1.0) * Para.FontSize;
            else
            {
                if (Line <= (CurrentCard.IsMagic() ? 7 : 4))
                    Para.LineHeight = 0.95 * Para.FontSize;
                else
                    Para.LineHeight = RichTextBoxDescription.Height / (Line * 1.05);
            }

            if (Para.Inlines.Count > 1)
            {
                Para.Inlines.ElementAt(1).FontSize = Para.Inlines.ElementAt(0).FontSize * 1.15; //29.8-32.4
            }
        }

        private void HandleRarity()
        {
            if (CurrentCard == null) { return; }
            BitmapSource Source = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray();

            ArtworkFoil.Opacity = 1;
            ArtworkFoil.Source = null;
            CardFoil.Source = null;
            CardBorder.Source = null; //XX
            LoreBorder.Source = null;
            ArtworkBorder.Source = null;
            
            if (CurrentCard.Rarity == RARITY.Common)
            {

            }
            else if (CurrentCard.Rarity == RARITY.Rare)
            {
                                
            }
            else if (CurrentCard.Rarity == RARITY.SuperRare)
            {
                ArtworkFoil.Source = Database.GetImage(@"Template\Foil\Super.png");
            }
            else if (CurrentCard.Rarity == RARITY.UltraRare)
            {                
                ArtworkFoil.Source = Database.GetImage(@"Template\Foil\Super.png");                
            }
            else if (CurrentCard.Rarity == RARITY.SecretRare)
            {                
                ArtworkFoil.Source = Database.GetImage(@"Template\Foil\Secret.png");
            }
            else if (CurrentCard.Rarity == RARITY.ParallelRare)
            {               
                CardFoil.Source = Database.GetImage(@"Template\Foil\Parallel.png");                
            }
            else if (CurrentCard.Rarity == RARITY.StarfoilRare)
            {                
                CardFoil.Source = Database.GetImage(@"Template\Foil\Star.png");                
            }
            else if (CurrentCard.Rarity == RARITY.MosaicRare)
            {                
                CardFoil.Source = Database.GetImage(@"Template\Foil\Mosaic.png");
            }
            else if (CurrentCard.Rarity == RARITY.GoldRare)
            {
                CardBorder.Source = Database.GetImage(@"Template\Border\Card\Gold.png");
                LoreBorder.Source = Database.GetImage(@"Template\Border\Lore\Gold.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(255, 215, 0);
                
                if (CurrentCard.IsPendulum)
                {
                    if (Pendulum.GetLines().Count() < 4)
                    {
                        //ArtworkBorder.Source = Database.GetImage(@"Template\Border\Pendulum\Small.png");
                        ArtworkBorder.Source = Database.GetImage(@"Template\Border\Pendulum\SmallGold.png");
                        //PendulumBoxMiddle.Source = Images.GetImage(GetLocationPath() + "Template/Border/Pendulum/SmallBox.png");
                        //PendulumBoxText.Source = Images.GetImage(GetLocationPath() + "Template/Border/Pendulum/TextBox.png");
                    }
                    else
                    {
                        ArtworkBorder.Source = Database.GetImage(@"Template\Border\Pendulum\MediumGold.png");
                        //PendulumBoxMiddle.Source = Images.GetImage(GetLocationPath() + "Template/Border/Pendulum/MediumBox.png");
                        //PendulumBoxText.Source = Images.GetImage(GetLocationPath() + "Template/Border/Pendulum/TextBox.png");
                    }
                    shadow = new DropShadowEffect();
                    shadow.BlurRadius = 15;
                    shadow.ShadowDepth = 0;
                    ArtworkBorder.Effect = shadow;
                }
                else
                {
                    ArtworkBorder.Source = Database.GetImage(@"Template\Border\Artwork\Gold.png");
                }
            }
            else if (CurrentCard.Rarity == RARITY.UltimateRare)
            {
                /*
                if (CurrentCard.Attribute != ATTRIBUTE.UNKNOWN)
                    Attribute.Source = Database.GetImage(@"Template\Attribute\"
                        + CurrentCard.Attribute + "_Emboss");
                 */

                /*
                
                if (!Current.IsFrame("Pendulum"))
                    Lore_Border.Source = Uti.GetImage("/Lore_Border_Emboss");
                    */
                
                ArtworkFoil.Source = Database.GetImage(@"Template\Foil\Ultimate.png");
                ArtworkFoil.Opacity = 0.2;

                CardBorder.Source = Database.GetImage(@"Template\Border\Card\Emboss.png");
                ArtworkBorder.Source = Database.GetImage(@"Template\Border\Artwork\Emboss.png");
                Source = (Source as BitmapImage).ToBitmap().Emboss().Brightness(-40).Contrast(-10).ToBitmapSource();
                
            }

            else if (CurrentCard.Rarity == RARITY.GhostRare)
            {
                if (Source != null)
                {
                    
                    Source = (Source as BitmapImage).ToBitmap().Brightness(-110)
                        .ColorBalance(255, 0, -35).Contrast(10).Negative().ToBitmapSource();
                }
                
            }
            /*
            else if (Rarity == "OV Rare")
            {
                //Custom
                LinearGradientBrush l = new LinearGradientBrush();
                l.EndPoint = new Point(1, 1);
                l.StartPoint = new Point(1, 0);

                l.GradientStops.Add(new GradientStop(Colors.Black, 0));
                l.GradientStops.Add(new GradientStop(Colors.White, -2));
                l.GradientStops.Add(new GradientStop(Colors.Gray, 1));

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                Name.Effect = shadow;
                Name.Foreground = l;
                //End


                Foil_Artwork.Source = Uti.GetImage("/Rarity/Foil_Super");
            }
            */



            if (CurrentCard.Edition == EDITION.DuelTerminal)
            {
                CardFoil.Source = Database.GetImage(@"Template\Foil\Shatter.png");
            }
            Artwork.Source = Source;
        }

        private void SilverHolofoil()
        {
            LinearGradientBrush b = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(160, 0.5),
                SpreadMethod = GradientSpreadMethod.Repeat,
                MappingMode = BrushMappingMode.Absolute
            };
            b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
            b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(200, 200, 200), Offset = 0.6 });
            b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1 });

            DropShadowEffect e = new DropShadowEffect();
            e.ShadowDepth = 1;
            e.Direction = -180;
            e.BlurRadius = 10;
            e.Color = Colors.Black;
            e.RenderingBias = RenderingBias.Quality;

            CardName.Foreground = b;
            CardName.Effect = e;
        }

        private void GoldHolofoil()
        {
            LinearGradientBrush b = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(80, 1),
                SpreadMethod = GradientSpreadMethod.Reflect,
                MappingMode = BrushMappingMode.Absolute
            };
            b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(250, 230, 60), Offset = 0 });
            b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(180, 120, 60), Offset = 1.8 });
            //b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(255, 215, 0), Offset = 0 });
            //b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(179, 156, 6), Offset = 1.8 });

            DropShadowEffect e = new DropShadowEffect();
            e.ShadowDepth = 1;
            e.Direction = -180;
            e.BlurRadius = 10;
            e.Color = Colors.Black;
            e.RenderingBias = RenderingBias.Quality;

            CardName.Foreground = b;
            CardName.Effect = e;
        }

        private void RainbowHolofoil()
        {
            LinearGradientBrush b = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(80, 1),
                SpreadMethod = GradientSpreadMethod.Reflect,
                MappingMode = BrushMappingMode.Absolute
            };
            //b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(211, 252, 252), Offset = 0 });
            //b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(214, 255, 255), Offset = 1.8 });
            b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(234, 255, 255), Offset = 0 });
            b.GradientStops.Add(new GradientStop { Color = Color.FromRgb(182, 162, 255), Offset = 1.8 });
            
            DropShadowEffect e = new DropShadowEffect();
            e.ShadowDepth = 1;
            e.Direction = -180;
            e.BlurRadius = 10;
            e.Color = Colors.Black;
            e.RenderingBias = RenderingBias.Quality;

            CardName.Foreground = b;
            CardName.Effect = e;
        }

        private void HandleNameRarity()
        {
            if (CurrentCard == null) { return; }

            CardName.Effect = null;
            if (CurrentCard.Rarity == RARITY.Common)
            {
                //New
                if (CurrentCard.IsFrame(FRAME.Xyz) || CurrentCard.IsMagic())
                {
                    this.CardName.Foreground = Brushes.White;
                }
                else
                {
                    this.CardName.Foreground = Brushes.Black;
                }
            }
            else if (CurrentCard.Rarity == RARITY.Rare)
            {
                /* New */
                SilverHolofoil();    
            }
            else if (CurrentCard.Rarity == RARITY.SuperRare)
            {
                //New
                if (CurrentCard.IsFrame(FRAME.Xyz) || CurrentCard.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;
            }
            else if (CurrentCard.Rarity == RARITY.UltraRare)
            {
                //New
                GoldHolofoil();               
            }
            else if (CurrentCard.Rarity == RARITY.SecretRare)
            {
                //New
                RainbowHolofoil();              
            }
            else if (CurrentCard.Rarity == RARITY.ParallelRare)
            {                
                GoldHolofoil();
               
            }
            else if (CurrentCard.Rarity == RARITY.StarfoilRare)
            {
                if (CurrentCard.IsFrame(FRAME.Xyz) || CurrentCard.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;
            }
            else if (CurrentCard.Rarity == RARITY.MosaicRare)
            {
                if (CurrentCard.IsFrame(FRAME.Xyz) || CurrentCard.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;

                /*
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;*/
            }
            else if (CurrentCard.Rarity == RARITY.GoldRare)
            {
                GoldHolofoil();
            }
            else if (CurrentCard.Rarity == RARITY.UltimateRare)
            {                
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(69, 34, 12));//222, 178, 29));

                
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = 90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;
                
                
            }
            else if (CurrentCard.Rarity == RARITY.GhostRare)
            {
                /*
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(234, 255, 255));
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(182, 162, 255);

                CardName.Effect = shadow;*/
                RainbowHolofoil();
            }
        }

        internal YgoCard SetFrame(FRAME frame)
        {
            //Current.SetF
            return CurrentCard;
        }

        private void HandleFrame()
        {
            if (CurrentCard == null)
            {
                return;
            }
            Frame.Source = Database.GetImage(@"Template\Frame\" + CurrentCard.Frame.ToString() + ".png");
        }

        private string SpecialPoint(double value)
        {
            string result = "";
            if (double.IsNaN(value))
            {
                result = "?";
            }
            else
            {
                result = value.ToString();
            }
            if (result.Length == 1)
                result = "      " + result;
            else if (result.Length == 2)
                result = "    " + result;
            else if (result.Length == 3)
                result = "  " + result;
            return result;
        }

        private void HandleAbility()
        {
            if (CurrentCard == null)
            {
                return;
            }
            this.Ability.Inlines.Clear();
            if (CurrentCard.IsMonster())
            {
                BracketLeft.Text = "["; BracketRight.Text = "]";
                double theWidth = 637;
                string futureText = null;

                {
                    string type = CurrentCard.Type != TYPE.NONE ? CurrentCard.Type.ToString() : "";// TransAbility(Current.Type);  


                    {
                        type = type.Replace("WingedBeast", "Winged Beast");
                        type = type.Replace("BeastWarrior", "Beast-Warrior");
                        type = type.Replace("SeaSerpent", "Sea Serpent");
                        type = type.Replace("DivineBeast", "Divine-Beast");
                        type = type.Replace("CreatorGod", "Creator God");
                        //type = TransLanguage(type);
                    }

                    List<string> abilityText = new List<string>();
                    abilityText.Add(type);
                    if (CurrentCard.Frame != FRAME.Normal && CurrentCard.Frame != FRAME.Effect)
                    {
                        abilityText.Add(CurrentCard.Frame.ToString());
                    }
                    if (CurrentCard.IsPendulum)
                    {
                        abilityText.Add("Pendulum");
                    }

                    foreach (ABILITY ability in CurrentCard.Abilities.Where(o => o != ABILITY.Effect && o != ABILITY.Tuner))
                    {
                        abilityText.Add(ability.ToString());
                    }
                    if (CurrentCard.Abilities.Contains(ABILITY.Tuner))
                    {
                        abilityText.Add(ABILITY.Tuner.ToString());
                    }

                    if (CurrentCard.Frame == FRAME.Effect || CurrentCard.Abilities.Contains(ABILITY.Effect))
                    {
                        abilityText.Add("Effect");
                    }

                    futureText = string.Join("_/_", abilityText);
                }
                int time = 0;

                futureText = futureText ?? "";
                double Default = 31; //12.2 - 11.4 | 31
                BracketLeft.FontSize = BracketRight.FontSize = 1.0 * Default; //||0.93
                do
                {
                    this.Ability.Inlines.Clear();
                    theWidth = 0;
                    double maxSize = Default - time * 0.1; // 0.1
                    time++;


                    for (int i = 0; i < futureText.Length; i++)
                    {
                        TextBlock perChar = new TextBlock();
                        char character = futureText[i];
                        if (Char.IsUpper(character))
                            perChar.FontSize = maxSize;
                        else
                        {
                            if (character == '/')
                                perChar.FontSize = 11.06 / 11.5 * maxSize;
                            else
                            {
                                if (character == '_')
                                {
                                    character = ' ';
                                    perChar.FontSize = 5.04 / 11.5 * maxSize; //6.94
                                }
                                else
                                {
                                    perChar.FontSize = 9.62 / 11.5 * maxSize;

                                }
                            }
                            character = Char.ToUpper(character);
                        }
                        perChar.Text = character.ToString();

                        FormattedText formattedText = new FormattedText
                        (
                            perChar.Text,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface(this.Ability.FontFamily, this.Ability.FontStyle, this.Ability.FontWeight, this.Ability.FontStretch, null),
                            perChar.FontSize,
                            Brushes.Black
                        );

                        theWidth += formattedText.Width;
                        this.Ability.Inlines.Add(perChar);
                    }
                } while (theWidth > 609);
            }
            else
            {
                BracketLeft.Text = BracketRight.Text = null;
            }


        }

        private string TransLanguage(string v)
        {
            return v;
        }

        private void HandleArtwork()
        {
            if (CurrentCard == null) { return; }
            if (CurrentCard.ArtworkByte == null)
            {
                CurrentCard.ArtworkByte = NoneImageArray;
                //Current.ArtworkByte = Database.GetImage(@"Template\NoneImage.png").GetImageArray();
            }
            Artwork.Source = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray();

        }

        private void HandleMiddle()
        {
            if (CurrentCard == null)
            {
                return;
            }
            Middle.Children.Clear();

            //Property S/T
            if (!CurrentCard.IsMonster())
            {
                Grid Grid = new Grid();
                Grid.VerticalAlignment = VerticalAlignment.Center;
                Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(17) });
                Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(17) });
                Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
                Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Middle.Children.Add(Grid);
                Canvas.SetRight(Grid, -13);
                Canvas.SetTop(Grid, 3);

                TextBlock BL = new TextBlock();
                Grid.SetColumn(BL, 0);
                Grid.SetRow(BL, 0);
                Grid.SetRowSpan(BL, 2);
                BL.Text = "[";
                BL.FontFamily = Fonts.StoneSerifLTBold;

                TextBlock BR = new TextBlock();
                Grid.SetColumn(BR, 2);
                Grid.SetRow(BR, 0);
                Grid.SetRowSpan(BR, 2);
                BR.Text = "]";
                BR.HorizontalAlignment = HorizontalAlignment.Right;
                BR.FontFamily = BL.FontFamily;

                Grid.Children.Add(BL);
                Grid.Children.Add(BR);

                TextBlock AbilityBlock = new TextBlock();
                Grid.SetColumn(AbilityBlock, 1);
                Grid.SetRow(AbilityBlock, 1);
                Grid.Children.Add(AbilityBlock);
                AbilityBlock.FontFamily = BL.FontFamily;

                string Ability = CurrentCard.Abilities.Count > 0
                    ? CurrentCard.Abilities[0].ToString()
                    : "";


                double theWidth = 637;
                string futureText = CurrentCard.Frame.ToString() + " Card";
                //if (vietnameseLang.IsChecked == true)
                //{
                //    futureText = TransLanguage(Current.Frame.ToString());
                //}
                if (CurrentCard.Property != PROPERTY.NONE && CurrentCard.Property != PROPERTY.Normal)
                {
                    futureText += "    ";
                    Image property = new Image();
                    property.Source = Database.GetImage(
                        @"Template\Middle\" + CurrentCard.Property.ToString() + ".png");
                    Middle.Children.Add(property);
                    //MessageBox.Show(Current.Property.ToString());
                    Canvas.SetLeft(property, 575);
                    Canvas.SetTop(property, 6);
                }
                futureText = "_" + futureText + "_";
                int time = 0;
                double Default = 38; //12.2 - 11.4
                BL.FontSize = Default; // 0.93
                BR.FontSize = BL.FontSize;
                do
                {
                    AbilityBlock.Inlines.Clear();
                    theWidth = 0;
                    double maxSize = Default - time * 0.1; // 0.1
                    time++;


                    for (int i = 0; i < futureText.Length; i++)
                    {
                        TextBlock perChar = new TextBlock();
                        char character = futureText[i];
                        if (Char.IsUpper(character))
                            perChar.FontSize = maxSize;
                        else
                        {
                            if (character == '/')
                                perChar.FontSize = 11.06 / 11.5 * maxSize;
                            else
                            {
                                if (character == '_')
                                {
                                    character = ' ';
                                    perChar.FontSize = 5.04 / 11.5 * maxSize; //6.94
                                }
                                else
                                {
                                    perChar.FontSize = 9.62 / 11.5 * maxSize;

                                }
                            }
                            character = Char.ToUpper(character);
                        }
                        perChar.Text = character.ToString();

                        FormattedText formattedText = new FormattedText
                        (
                            perChar.Text,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface(AbilityBlock.FontFamily, AbilityBlock.FontStyle, AbilityBlock.FontWeight, AbilityBlock.FontStretch, null),
                            perChar.FontSize,
                            Brushes.Black
                        );

                        theWidth += formattedText.Width;
                        AbilityBlock.Inlines.Add(perChar);
                    }
                } while (theWidth > 609);
            }
            //Monster
            else
            {

                if (!double.IsNaN(CurrentCard.Level) || !double.IsNaN(CurrentCard.Rank))
                {

                    int number = !double.IsNaN(CurrentCard.Level) ? (int)CurrentCard.Level : (int)CurrentCard.Rank;

                    string type = CurrentCard.IsFrame(FRAME.Xyz) ? "Rank" : "Level";
                    for (int i = 1; i <= number; i++)
                    {
                        Image Star = new Image();
                        Star.Source = Database.GetImage(@"Template\" + type +
                            (CurrentCard.Rarity == RARITY.UltimateRare ? "Emboss" : null) + ".png");


                        Star.Width = Middle.Height - 3;
                        Star.Height = Star.Width;
                        Canvas.SetTop(Star, 2);
                        Middle.Children.Add(Star);

                        if (type == "Level")
                            Canvas.SetLeft(Star, Canvas.GetLeft(Middle)
                                + Middle.Width - (Star.Width + 0.6) * (i + 1.5));
                        else
                            Canvas.SetRight(Star, Canvas.GetLeft(Middle)
                                 + Middle.Width - (Star.Width + 0.6) * (i + 1.5));

                    }
                }

            }
        }


        private void HandleName()
        {
            if (CurrentCard == null)
            {
                return;
            }
            CardName.Text = CurrentCard.Name;            

            if (CardName.GetFormattedText().Width > NameViewbox.Width)
                CardName.Width = double.NaN;
            else
                CardName.Width = NameViewbox.Width;
        }

        private void HandleAttribute()
        {
            if (CurrentCard == null)
            {
                return;
            }
            Attribute.Source = Database.GetImage(
                @"Template\Attribute\" + (CurrentCard.Rarity == RARITY.UltimateRare ? @"Emboss\" : "") +
                CurrentCard.Attribute.ToString() + ".png");

            if (CurrentCard.Attribute != ATTRIBUTE.UNKNOWN)
            {
                AttributeText.Text = CurrentCard.Attribute.ToString();
            }
            else
            {
                AttributeText.Text = null;
            }

            if (CurrentCard.Rarity == RARITY.UltimateRare)
            {
                Attribute.Source = Database.GetImage(
                   @"Template\Attribute\Emboss\" + CurrentCard.Attribute.ToString() + ".png");
                AttributeText.Foreground = Brushes.White;
            } else
            {
                Attribute.Source = Database.GetImage(
                   @"Template\Attribute\" + CurrentCard.Attribute.ToString() + ".png");
                AttributeText.Foreground = Brushes.Gray;
            }
        }

        public int RecommendedHeight
        {
            get
            {
                if (CurrentCard.IsPendulum)
                {
                    if (Pendulum.GetLines().Count() < 4)
                    {
                        return -85;
                    }
                    else
                    {
                        return -122;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }


        internal ImageSource ResizeArtwork(int padding, bool isAplly)
        {
            System.Drawing.Bitmap bitmap = CurrentCard.ArtworkByte.GetBitmapImageFromByteArray().ToBitmap();
            ImageSource Source = bitmap.ReduceHeightThenPadding(padding).ToBitmapSource();
            if (isAplly)
            {
                CurrentCard.ArtworkByte = (Source as BitmapSource).GetImageArray();
                Render(CurrentCard);
            }
            else
            {
                Render(CurrentCard);
                Artwork.Source = Source;
            }
            return Source;
        }

        #region EventHandler

        private void Value_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           ValueClick.Invoke(sender, e);
        }
        private void Ability_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AbilityClick.Invoke(sender, e);
        }
        private void Artwork_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtworkClick.Invoke(sender, e);
        }
        private void Attribute_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AttributeClick.Invoke(sender, e);
        }
        private void Circulation_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CirculationClick.Invoke(sender, e);
        }
        private void Description_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DescriptionClick.Invoke(sender, e);
        }
        private void Frame_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameClick.Invoke(sender, e);
        }
        private void Middle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MiddleClick.Invoke(sender, e);
        }
        private void Name_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NameClick.Invoke(sender, e);
        }
        #endregion EventHandler

        private void ControlZone_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).BorderBrush = Brushes.Red;
        }

        private void ControlZone_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Border).BorderBrush = Brushes.Transparent;
        }
    }

    public class TextBlockAutoShrink : TextBlock, INotifyPropertyChanged
    {
        // private Viewbox _viewBox;
        //private double _defaultMargin = 6;
        private Typeface _typeface;
        public const string LineCountPropertyName = "LineCount";

        static TextBlockAutoShrink()
        {
            TextBlock.TextProperty.OverrideMetadata(typeof(TextBlockAutoShrink), new FrameworkPropertyMetadata(new PropertyChangedCallback(TextPropertyChanged)));
        }

        public TextBlockAutoShrink() : base()
        {
            _typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontFamily);
            base.DataContextChanged += new DependencyPropertyChangedEventHandler(TextBlockAutoShrink_DataContextChanged);
            this.Loaded += OnLoaded;
            this.SizeChanged += TextBlockAutoShrink_SizeChanged;
        }

        private void TextBlockAutoShrink_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged(LineCountPropertyName);
        }

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var t = sender as TextBlockAutoShrink;
            if (t != null)
            {
                t.FitSize();
            }
        }

        void TextBlockAutoShrink_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FitSize();
        }

        #region LineCount
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        public int LineCount
        {
            get { return (int)GetPrivatePropertyInfo(typeof(TextBlock), LineCountPropertyName).GetValue(this, null); }
        }

        private PropertyInfo GetPrivatePropertyInfo(Type type, string propertyName)
        {
            var props = type.GetProperties(Flags);
            return props.FirstOrDefault(propInfo => propInfo.Name == propertyName);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NotifyPropertyChanged(LineCountPropertyName);
        }

        private BindingFlags Flags = BindingFlags.Instance
                                   | BindingFlags.GetProperty
                                   | BindingFlags.NonPublic;

        #endregion Linecount

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            FitSize();

            base.OnRenderSizeChanged(sizeInfo);
        }

        private double DefaultLineHeight;

        public double ScaledLineHeight
        {
            get { return (double)GetValue(ScaledLineHeightProperty); }
            set
            {
                DefaultLineHeight = this.LineHeight;
                SetValue(ScaledLineHeightProperty, value);
            }
        }

        public static readonly DependencyProperty ScaledLineHeightProperty
            = DependencyProperty.Register(
              "ScaledLineHeight",
              typeof(double),
              typeof(TextBlockAutoShrink),
              new PropertyMetadata(1.0)
          );

        public double MaxFontSize
        {
            get { return (double)GetValue(MaxFontSizeProperty); }
            set
            {
                SetValue(MaxFontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty MaxFontSizeProperty
            = DependencyProperty.Register(
              "MaxFontSize",
              typeof(double),
              typeof(TextBlockAutoShrink),
              new PropertyMetadata(double.NaN)
          );

        private void FitSize()
        {
           // return;
            if (this.ActualHeight > 0 && this.Height > 0)
            {
                
                var ratio = this.Height / (this.ActualHeight);
                // Normalize due to Height miscalculation. We do it step by step repeatedly until the requested height is reached. Once the fontsize is changed, this event is re-raised
                // And the ActualHeight is lowered a bit more until it doesnt enter the enclosing If block.
                ratio = (1 - ratio > 0.04) ? Math.Sqrt(ratio) : ratio;
                

                double fitFontSize = Math.Min(this.FontSize * ratio, this.ActualHeight / this.LineCount);
                
                if (double.IsNaN(MaxFontSize) == false)
                {
                    fitFontSize = Math.Min(MaxFontSize, fitFontSize);
                }
                double fitLineHeight = fitFontSize;
                if (double.IsNaN(this.ScaledLineHeight) == false)
                {
                    fitLineHeight = fitFontSize / this.ScaledLineHeight;
                }
                else
                {
                    fitLineHeight = this.DefaultLineHeight;
                }

                this.FontSize = Math.Round(fitFontSize, 2);
                this.LineHeight = Math.Round(fitLineHeight, 2);
                this.LineStackingStrategy = LineStackingStrategy.MaxHeight;

                /*
                while (this.ActualHeight > this.Height)                
                {                    
                    double fitFontSize = Math.Min(MaxFontSize, (this.ActualHeight) / this.LineCount);
                    double fitLineHeight = fitFontSize;
                    if (double.IsNaN(this.ScaledLineHeight) == false)
                    {
                        fitLineHeight = fitFontSize / this.ScaledLineHeight;
                    }
                    else
                    {
                        fitLineHeight = this.DefaultLineHeight;
                    }

                    this.FontSize = Math.Round(fitFontSize, 2);
                    this.LineHeight = Math.Round(fitLineHeight, 2);
                    this.UpdateLayout();                    
                } */



            }

        }

        /* Old FitSize
        private void FitSize()
        {
            FrameworkElement parent = this.Parent as FrameworkElement;
            if (parent != null)
            {
                var targetWidthSize = this.FontSize;
                var targetHeightSize = this.FontSize;

                var maxWidth = double.IsInfinity(this.MaxWidth) ? parent.ActualWidth : this.MaxWidth;
                var maxHeight = double.IsInfinity(this.MaxHeight) ? parent.ActualHeight : this.MaxHeight;

                if (this.ActualWidth > maxWidth)
                {
                    targetWidthSize = (double)(this.FontSize * (maxWidth / (this.ActualWidth + _defaultMargin)));

                }

                if (this.ActualHeight > maxHeight)
                {
                    var ratio = maxHeight / (this.ActualHeight);

                    // Normalize due to Height miscalculation. We do it step by step repeatedly until the requested height is reached. Once the fontsize is changed, this event is re-raised
                    // And the ActualHeight is lowered a bit more until it doesnt enter the enclosing If block.
                    ratio = (1 - ratio > 0.04) ? Math.Sqrt(ratio) : ratio;

                    targetHeightSize = (double)(this.FontSize * ratio);
                }
                double fitFontSize = Math.Min(targetWidthSize, targetHeightSize);
                if (double.IsNaN(this.MaxFontSize) == false)
                {
                    fitFontSize = Math.Min(fitFontSize, this.MaxFontSize);
                }

                this.FontSize = fitFontSize;
                if (double.IsNaN(this.ScaledLineHeight) == false)
                {
                    double modified = 0;
                    if (this.LineCount > 6)
                    {
                        modified = 0.01;
                        //this.FontSize = (maxHeight / this.LineCount);
                    }
                    this.LineHeight = this.FontSize / (1 / this.ScaledLineHeight - modified);

                }
                else
                {
                    this.LineHeight = this.DefaultLineHeight;
                }
            }
        }  
        */

    }

    public class MathConverter :
#if !SILVERLIGHT
         System.Windows.Markup.MarkupExtension,
                IMultiValueConverter,
#endif
        IValueConverter
    {

        public MathConverter()
        {

        }
        Dictionary<string, IExpression> _storedExpressions = new Dictionary<string, IExpression>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                decimal result = Parse(parameter.ToString()).Eval(values);
                if (targetType == typeof(decimal)) return result;
                if (targetType == typeof(string)) return result.ToString();
                if (targetType == typeof(int)) return (int)result;
                if (targetType == typeof(double)) return (double)result;
                if (targetType == typeof(long)) return (long)result;
                throw new ArgumentException(String.Format("Unsupported target type {0}", targetType.FullName));
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

#if !SILVERLIGHT
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
#endif
        protected virtual void ProcessException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private IExpression Parse(string s)
        {
            IExpression result = null;
            if (!_storedExpressions.TryGetValue(s, out result))
            {
                result = new Parser().Parse(s);
                _storedExpressions[s] = result;
            }

            return result;
        }

        interface IExpression
        {
            decimal Eval(object[] args);
        }

        class Constant : IExpression
        {
            private decimal _value;

            public Constant(string text)
            {
                if (!decimal.TryParse(text, out _value))
                {
                    throw new ArgumentException(String.Format("'{0}' is not a valid number", text));
                }
            }

            public decimal Eval(object[] args)
            {
                return _value;
            }
        }

        class Variable : IExpression
        {
            private int _index;

            public Variable(string text)
            {
                if (!int.TryParse(text, out _index) || _index < 0)
                {
                    throw new ArgumentException(String.Format("'{0}' is not a valid parameter index", text));
                }
            }

            public Variable(int n)
            {
                _index = n;
            }

            public decimal Eval(object[] args)
            {
                if (_index >= args.Length)
                {
                    throw new ArgumentException(String.Format("MathConverter: parameter index {0} is out of range. {1} parameter(s) supplied", _index, args.Length));
                }

                return System.Convert.ToDecimal(args[_index]);
            }
        }

        class BinaryOperation : IExpression
        {
            private Func<decimal, decimal, decimal> _operation;
            private IExpression _left;
            private IExpression _right;

            public BinaryOperation(char operation, IExpression left, IExpression right)
            {
                _left = left;
                _right = right;
                switch (operation)
                {
                    case '+': _operation = (a, b) => (a + b); break;
                    case '-': _operation = (a, b) => (a - b); break;
                    case '*': _operation = (a, b) => (a * b); break;
                    case '/': _operation = (a, b) => (a / b); break;
                    default: throw new ArgumentException("Invalid operation " + operation);
                }
            }

            public decimal Eval(object[] args)
            {
                return _operation(_left.Eval(args), _right.Eval(args));
            }
        }

        class Negate : IExpression
        {
            private IExpression _param;

            public Negate(IExpression param)
            {
                _param = param;
            }

            public decimal Eval(object[] args)
            {
                return -_param.Eval(args);
            }
        }

        class Parser
        {
            private string text;
            private int pos;

            public IExpression Parse(string text)
            {
                try
                {
                    pos = 0;
                    this.text = text;
                    var result = ParseExpression();
                    RequireEndOfText();
                    return result;
                }
                catch (Exception ex)
                {
                    string msg =
                        String.Format("MathConverter: error parsing expression '{0}'. {1} at position {2}", text, ex.Message, pos);

                    throw new ArgumentException(msg, ex);
                }
            }

            private IExpression ParseExpression()
            {
                IExpression left = ParseTerm();

                while (true)
                {
                    if (pos >= text.Length) return left;

                    var c = text[pos];

                    if (c == '+' || c == '-')
                    {
                        ++pos;
                        IExpression right = ParseTerm();
                        left = new BinaryOperation(c, left, right);
                    }
                    else
                    {
                        return left;
                    }
                }
            }

            private IExpression ParseTerm()
            {
                IExpression left = ParseFactor();

                while (true)
                {
                    if (pos >= text.Length) return left;

                    var c = text[pos];

                    if (c == '*' || c == '/')
                    {
                        ++pos;
                        IExpression right = ParseFactor();
                        left = new BinaryOperation(c, left, right);
                    }
                    else
                    {
                        return left;
                    }
                }
            }

            private IExpression ParseFactor()
            {
                SkipWhiteSpace();
                if (pos >= text.Length) throw new ArgumentException("Unexpected end of text");

                var c = text[pos];

                if (c == '+')
                {
                    ++pos;
                    return ParseFactor();
                }

                if (c == '-')
                {
                    ++pos;
                    return new Negate(ParseFactor());
                }

                if (c == 'x' || c == 'a') return CreateVariable(0);
                if (c == 'y' || c == 'b') return CreateVariable(1);
                if (c == 'z' || c == 'c') return CreateVariable(2);
                if (c == 't' || c == 'd') return CreateVariable(3);

                if (c == '(')
                {
                    ++pos;
                    var expression = ParseExpression();
                    SkipWhiteSpace();
                    Require(')');
                    SkipWhiteSpace();
                    return expression;
                }

                if (c == '{')
                {
                    ++pos;
                    var end = text.IndexOf('}', pos);
                    if (end < 0) { --pos; throw new ArgumentException("Unmatched '{'"); }
                    if (end == pos) { throw new ArgumentException("Missing parameter index after '{'"); }
                    var result = new Variable(text.Substring(pos, end - pos).Trim());
                    pos = end + 1;
                    SkipWhiteSpace();
                    return result;
                }

                const string decimalRegEx = @"(\d+\.?\d*|\d*\.?\d+)";
                var match = Regex.Match(text.Substring(pos), decimalRegEx);
                if (match.Success)
                {
                    pos += match.Length;
                    SkipWhiteSpace();
                    return new Constant(match.Value);
                }
                else
                {
                    throw new ArgumentException(String.Format("Unexpeted character '{0}'", c));
                }
            }

            private IExpression CreateVariable(int n)
            {
                ++pos;
                SkipWhiteSpace();
                return new Variable(n);
            }

            private void SkipWhiteSpace()
            {
                while (pos < text.Length && Char.IsWhiteSpace((text[pos]))) ++pos;
            }

            private void Require(char c)
            {
                if (pos >= text.Length || text[pos] != c)
                {
                    throw new ArgumentException("Expected '" + c + "'");
                }

                ++pos;
            }

            private void RequireEndOfText()
            {
                if (pos != text.Length)
                {
                    throw new ArgumentException("Unexpected character '" + text[pos] + "'");
                }
            }
        }
    }
}
