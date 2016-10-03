using Newtonsoft.Json;
using OV.Core;
using OV.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using static OV.Tools.Utilities;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for YGOView.xaml
    /// </summary>
    public partial class YGOView : UserControl
    {
        YGOCard Current = YGOCard.Default;
        private string NoneImagePath = "";

        public YGOView()
        {
            InitializeComponent();
            
        }

        public class Fonts
        {  
            public static FontFamily StoneSerifLTBold
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#StoneSerif LT Bold");
                }
            }

            public static FontFamily StoneSerifRegular
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#Stone Serif");
                }
            }

            public static FontFamily MatrixBook
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#Matrix-Book");
                }
            }


            public static FontFamily YGOMatrixSmallCap2
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#Yu-Gi-Oh! Matrix Small Caps 2");
                }
            }

            public static FontFamily StoneSerifItalic
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#Stone Serif");
                }
            }
            /*
            public static FontFamily MatrixBoldFractions
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#MatrixBoldFractions");
                }
            }  */

            public static FontFamily PalatinoLinotype
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#Palatino Linotype");
                }
            }

            public static FontFamily BankGothicMdBT
            {
                get
                {
                    return new FontFamily(
                        new Uri(Utilities.GetLocationPath() + @"\Fonts\"), "./#BankGothic Md BT");
                }
            }
        };
                

        private void FirstLoad()
        {
            CardName.FontFamily = Fonts.YGOMatrixSmallCap2;
            //Attribute
            Ability.FontFamily = BracketLeft.FontFamily = BracketRight.FontFamily = Fonts.StoneSerifLTBold;
            ATKBlock.FontFamily = DEFBlock.FontFamily = Fonts.YGOMatrixSmallCap2;
            Creator.FontFamily = Fonts.StoneSerifRegular;
            CardNumber.FontFamily = Fonts.StoneSerifRegular;
            SetNumber.FontFamily = Fonts.StoneSerifRegular;
            CardBorder.Source = Images.GetImage(Utilities.GetLocationPath() + @"\Template\Border\Card\Default.png");
            AttributeText.FontFamily = Fonts.PalatinoLinotype;
            ArtworkBorder.Source = Images.GetImage(Utilities.GetLocationPath() + @"\Template\Border\Artwork\Default.png");
            LoreBorder.Source = Images.GetImage(Utilities.GetLocationPath() + @"\Template\Border\Lore\Default.png");
            ScaleLeftBlock.FontFamily = ScaleRightBlock.FontFamily = Fonts.YGOMatrixSmallCap2;


            ScaleLeft.Source = Images.GetImage(GetLocationPath() + @"\Template\Middle\ScaleLeft.png");
            ScaleRight.Source = Images.GetImage(GetLocationPath() + @"\Template\Middle\ScaleRight.png");
        }

        public void SetDefaultArtwork(string filePath)
        {
            NoneImagePath = filePath;
            Current.ArtworkByte = Images.GetImageByte(NoneImagePath);
        }


        internal void Render(YGOCard card)
        {
            Current = card;
            RenderCard();
        }

        private void RenderCard()
        {
            FirstLoad();            
            HandleName();
            HandleAttribute();
            HandleMiddle();
            HandleArtwork();
            HandleFrame();
            HandleAD();
            HandleAbility();
            HandleRarity();
            HandleCreator();
            HandleScale();
            HandleDescription();
            HandleSticker();
            HandleEdition();
            HandleCartNumber();
            HandleSetCard();
        }

        private void HandleScale()
        {
            if (Current.IsPendulum == false)
            {
                ScaleLeftBlock.Text = ScaleRightBlock.Text = "";
                ScaleLeft.Visibility = ScaleRight.Visibility = Visibility.Hidden;
                return;
            }
            ScaleLeft.Visibility = ScaleRight.Visibility = Visibility.Visible;
            ScaleLeftBlock.Text = double.IsNaN(Current.ScaleLeft) ? "?" : Current.ScaleLeft.ToString();
            ScaleRightBlock.Text = double.IsNaN(Current.ScaleRight) ? "?" : Current.ScaleRight.ToString();


            if (Pendulum.CountLine() < 4)
            {
                Canvas.SetTop(ScaleLeftBlock, 824);
                Canvas.SetTop(ScaleRightBlock, 824);
            }
            else
            {
                Canvas.SetTop(ScaleLeftBlock, 800);
                Canvas.SetTop(ScaleRightBlock, 800);
            }


            //ScaleLeft
            if (!double.IsNaN(Current.ScaleLeft))
            {
                if (Current.ScaleLeft >= 10)
                {
                    if (Current.ScaleLeft == 11)
                    {
                        Canvas.SetLeft(ScaleLeftBlock, 74 - 16);
                    }
                    else
                    {
                        Canvas.SetLeft(ScaleLeftBlock, 74 - 19);
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
            if (!double.IsNaN(Current.ScaleRight))
            { 
                if (Current.ScaleRight >= 10)
                {
                    if (Current.ScaleRight == 11)
                    {
                        Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 42);
                    }
                    else
                    {
                        Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 45);
                    }
                }
                else
                {
                    Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 22);
                }
            }
            else
            {                
                Canvas.SetLeft(ScaleRightBlock, CardCanvas.Width - 74 - 22);
            }
            Canvas.SetTop(ScaleLeft, ScaleLeftBlock.Top() - 30);
            Canvas.SetTop(ScaleRight, ScaleRightBlock.Top() - 30);
            //MessageBox.Show(ScaleLeftText.GetFormattedText().Width.ToString());
        }

        private void HandleSetCard()
        {
            if (Current.Set == null) { return; }
            SetNumber.Text = Current.Set.ToUpper();
            if (Current.IsFrame(FRAME.Xyz) && !Current.IsPendulum)
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
            if (Current.Number > 0)
            {
                string number = Current.Number.ToString().PadLeft(8, '0');
                CardNumber.Text = number;
                if (CardNumber.GetFormattedText().Width > 109)
                {
                    CardNumberViewbox.Width = 109;
                }
                else
                {
                    CardNumberViewbox.Width = double.NaN;
                }
            } else
            {
                CardNumber.Text = null;
                CardNumberViewbox.Width = double.NaN;
            }


            if (Current.IsFrame(FRAME.Xyz) && !Current.IsPendulum)
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
            if (Current.IsFrame(FRAME.Xyz) && !Current.IsPendulum)
            {
                Edition.Foreground = Brushes.White;
            }
            else
            {
                Edition.Foreground = Brushes.Black;
            }

            //Edition.Text = Current.Edition.ToString();


            if (Current.Edition == EDITION.UnlimitedEdition)
            {
                Edition.Text = null;
            }
            else if (Current.Edition == EDITION.FirstEdition)
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
            else if (Current.Edition == EDITION.LimitedEdition)
            {
                Edition.FontFamily = Fonts.PalatinoLinotype;
                Edition.FontSize = 24;
                Edition.FontWeight = FontWeights.SemiBold;
                Edition.Text = Current.Edition.ToString().AddSpaceBetweenCapital().ToUpper();
            }
            else if (Current.Edition == EDITION.DuelTerminal)
            {
                Edition.FontFamily = Fonts.BankGothicMdBT;
                Edition.Text = Current.Edition.ToString().AddSpaceBetweenCapital().ToUpper();
                Edition.FontWeight = FontWeights.Normal;
                Edition.FontSize = 30;

                //ChangeRarity(Text);
            }
            
        }

        private void HandleAD()
        {
            if (Current.IsMonster())
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
                    ATKBlock.Text += SpecialPoint(Current.ATK);
                    //Canvas.SetLeft(ATKBlock, ATKBlock.Left() - ATKBlock.GetFormattedText(SpecialPoint(Current.ATK)).Width);
                    //MessageBox.Show(ATKBlock.Left().ToString());
                }
                //if (!Double.IsNaN(Current.DEF))
                {
                    DEFBlock.Text += SpecialPoint(Current.DEF);

                    //Canvas.SetLeft(DEFBlock, DEFBlock.Left() - DEFBlock.GetFormattedText(SpecialPoint(Current.DEF)).Width);
                }
                Canvas.SetTop(DescriptionBorder, 926);
            }
            else
            {
                ATKBlock.Text = DEFBlock.Text = "";
                Canvas.SetTop(DescriptionBorder, 891);
            }
        }

        private void HandleSticker()
        {
            if (Current == null) { return; }
            Sticker.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Sticker/" 
                + Current.Sticker.ToString() + ".png");
        }

        private void HandleCreator()
        {
            if (Current == null) { return; }
            if (Current.IsFrame(FRAME.Xyz))
            {
                Creator.Foreground = Brushes.White;
            } else
            {
                Creator.Foreground = Brushes.Black;
            }
            Creator.Text = Current.Creator;
        }

        public void Save()
        {
            string text = JsonConvert.SerializeObject(Current, Formatting.Indented);
            //MessageBox.Show(xml);


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            if (Current.Name == null)
                dlg.FileName = "Card Name";
            else
                dlg.FileName = Current.Name.Replace(":", " -"); // Default file name
            //dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "JSON File|*.json"; // Filter files by extension 

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Save document 
                string filename = dlg.FileName;
                File.WriteAllText(filename, text);
            }
        }

        public void Load()
        {

            Microsoft.Win32.OpenFileDialog choofdlog = new Microsoft.Win32.OpenFileDialog();
            choofdlog.Filter = "JSON File|*.json";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;
            string path = null;
            if (choofdlog.ShowDialog() == true)
            {
                path = choofdlog.FileName;
            }

            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            //var settings = new ObjectCustomerSettings();
            Current = JsonConvert.DeserializeObject<YGOCard>(File.ReadAllText(path));
            Render(Current);
        }

        public void Export()
        {
            if (Current == null)
            {

                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            //dlg.FileName = NameEdit.Text.Replace(":", " -"); // Default file name
            if (String.IsNullOrEmpty(dlg.FileName))
                dlg.FileName = Current.Name != null ? Current.Name.ToNonVietnamese() : "Card Name";
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

        public void CreateBitmapFromVisual(Visual target, string fileName)
        {
            //double scale = 96 / 96;


            if (target == null || string.IsNullOrEmpty(fileName))
            {
                return;
            }
            /*
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, scale * 96, scale * 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            Size size = new Size(bounds.Size.Width / (scale), bounds.Size.Height / (scale));

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(target);
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), size));
                //context.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(813, 1185)));
            }
            //MessageBox.Show(bounds.Size.ToString());
            renderTarget.Render(visual);
            */
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
            if (fileName.IsEndWith(".png"))
                bitmapEncoder = new PngBitmapEncoder();
            else if (fileName.IsEndWith(".jpg"))
            {
                bitmapEncoder = new JpegBitmapEncoder();
            }
            else if (fileName.IsEndWith(".bmp"))
                bitmapEncoder = new BmpBitmapEncoder();


            bitmapEncoder.Frames.Add(BitmapFrame.Create(source));
            //else if (target == AllView_Triad)
            //bitmapEncoder.Frames.Add(BitmapFrame.Create(ImageSource_Triad as BitmapSource));

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
                if (fileName.IsEndWith(".png"))
                {
                    extension = System.Drawing.Imaging.ImageFormat.Png;
                }
                else if (fileName.IsEndWith(".jpg"))
                {
                    extension = System.Drawing.Imaging.ImageFormat.Jpeg;
                }
                else if (fileName.IsEndWith(".bmp"))
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

                double scale = 96 / 96;
                //Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
                //MessageBox.Show(bounds.Height.ToString());
                //RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, scale * 96, scale * 96, PixelFormats.Default);
                RenderTargetBitmap renderTarget = new RenderTargetBitmap(813, 1185, scale * 96, scale * 96, PixelFormats.Pbgra32);

                DrawingVisual visual = new DrawingVisual();

                Size size = new Size(813, 1185);
                //Size size = new Size(bounds.Size.Width / (scale), bounds.Size.Height / (scale));
                using (DrawingContext context = visual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(target);

                    context.DrawRectangle(visualBrush, null, new Rect(new Point(), size));

                    //context.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(813, 1185)));
                }
                //MessageBox.Show(bounds.Size.ToString());

                renderTarget.Render(visual);



                return renderTarget;
            }
        }

        private void HandleDescription()
        {
            if (Current == null)
            {

                return;
            }

            string Text = Current.Description;
            RichTextBox Target = Description;
            Paragraph Para = new Paragraph();

            Para.TextAlignment = TextAlignment.Justify;
            Para.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            double Default = 0;


            Description.Height = 153;
            Description.Width = Current.IsFrame(FRAME.Normal) ? 694 : 698;




            {
                if (Current.IsFrame(FRAME.Normal))
                {
                    if (Text != null)
                        Text = Text.Replace("\"", "''");
                    Target.Document.Blocks.Clear();
                    Target.Document.Blocks.Add(Para);

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
                    Default = 27; //29.8-32.4 -- 33.1
                    Para.FontSize = Default;
                }
                else
                {
                    Target.Document.Blocks.Clear();
                    Target.Document.Blocks.Add(Para);
                    Para.Inlines.Add(new Run(Text));

                    Para.FontFamily = Fonts.MatrixBook;
                    Default = 32.4; //32.4-33.1
                    Description.Height = 158;//160
                    if (Current.IsMagic())
                    {
                        Default = 32.8;
                        Description.Height = 230;//226
                    }
                }
            }

            Para.FontSize = Default;
            // 6-26.5
            while (Para.FontSize * (Current.IsFrame(FRAME.Normal) && (Text != null && Text.IsVietnamese()) ? 1.1 : 1) * Target.CountLine() > Target.Height)
            {
                Para.FontSize -= 0.07; //0.1
                Target.Document.Blocks.Clear();
                Target.Document.Blocks.Add(Para);
            }

            int Line = Target.CountLine();
            //Para.LineHeight = (Current.IsFrame("Normal") ? 1.1 : 1) * Para.FontSize;
            if (Current.IsFrame(FRAME.Normal))
            {
                double param = 1;
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
            }
            else
            {
                if (Line <= (Current.IsMagic() ? 7 : 4))
                    Para.LineHeight = 0.95 * Para.FontSize;
                else
                    Para.LineHeight = Target.Height / (Line * 1.05);
            }

            if (Para.Inlines.Count > 1)
            {
                Para.Inlines.ElementAt(1).FontSize = Para.Inlines.ElementAt(0).FontSize * 1.15; //29.8-32.4
            }
            //if (IsAlert && Line > (Current.IsMagic ? 10 : 7))
            //    MessageBox.Show("Số dòng đề nghị là " + (Current.IsMagic ? 10 : 7) + " để dễ hiển thị và in ấn.", "Tiêu chuẩn");

        }

        private void HandleRarity()
        {
            if (Current == null)
            {

                return;
            }

            BitmapSource Source = null;
            //if (!String.IsNullOrEmpty(Current.Artwork))
            Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
            //else
            //    Source = Uti.GetImage("/Artwork/_None");//XX
            //Initialize
            {
                ArtworkFoil.Opacity = 1;
                ArtworkFoil.Source = null;
                CardFoil.Source = null; 
                CardBorder.Source = null; //XX
                CardName.Effect = null;
                LoreBorder.Source = null;
                ArtworkBorder.Source = ArtworkBorder.Source ?? null;

                /*
                if (Current.Attribute !=  AttributeEnum.UNKNOWN)
                    Attribute.Source = Uti.GetImage("/Attribute/" + Current.Attribute + Language);
                if (!Current.IsMagic && Current.Middle != null)
                {
                    if (Current.Middle.Split('_').Length > 1)
                    {
                        string type = Current.Middle.Split('_')[0];
                        int number = Int32.Parse(Current.Middle.Split('_')[1]);
                        ChangeLevel(number, type);
                    }
                }
                */
                //Frame.Source = Uti.GetImage("/Frame/" + Current.Frame );

            }
            if (Current.Rarity == RARITY.Common)
            {
                if (Current.IsFrame(FRAME.Xyz) || Current.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;
            }
            else if (Current.Rarity == RARITY.Rare)
            {
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(79, 79, 79)); //180


                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;
                //new SolidColorBrush(Color.FromRgb(206, 212, 217));
            }
            else if (Current.Rarity == RARITY.SuperRare)
            {
                if (Current.IsFrame(FRAME.Xyz) || Current.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;



                ArtworkFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Super.png");
            }
            else if (Current.Rarity == RARITY.UltraRare)
            {
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(88, 76, 12));//222, 178, 29));
                ArtworkFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Super.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(255, 255, 177);

                CardName.Effect = shadow;
            }
            else if (Current.Rarity == RARITY.SecretRare)
            {
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(211, 252, 252));//234,255,255
                ArtworkFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Secret.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(214, 255, 255);

                CardName.Effect = shadow;
            }
            else if (Current.Rarity == RARITY.ParallelRare)
            {
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(88, 76, 12));//234,255,255
                CardFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Parallel.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(255, 215, 0);

                CardName.Effect = shadow;
            }
            else if (Current.Rarity == RARITY.StarfoilRare)
            {
                if (Current.IsFrame(FRAME.Xyz) || Current.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;
                CardFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Star.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;
            }
            else if (Current.Rarity == RARITY.MosaicRare)
            {
                if (Current.IsFrame(FRAME.Xyz) || Current.IsMagic())
                    CardName.Foreground = Brushes.White;
                else
                    CardName.Foreground = Brushes.Black;
                CardFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Mosaic.png");

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;
            }
            else if (Current.Rarity == RARITY.GoldRare)
            {
                CardBorder.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Card/Gold.png");
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(88, 76, 12));//234,255,255

                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(255, 215, 0);

                CardName.Effect = shadow;
                /*
                if (!Current.IsMagic && Current.Middle != null)
                {
                    string type = Current.Middle.Split('_')[0];
                    int number = Int32.Parse(Current.Middle.Split('_')[1]);
                    ChangeLevel(number, type);
                }

                if (!Current.IsFrame("Pendulum"))
                {
                    Lore_Border.Source = Uti.GetImage("/Lore_Border_Gold");
                    Artwork_Border.Source = Uti.GetImage("/Border_Artwork_Gold");
                }
                else
                {
                    if (GetLine(Pendulum) < 4)
                        Artwork_Border.Source = Uti.GetImage("/Border_Pendulum_Small_Gold");
                    else
                        Artwork_Border.Source = Uti.GetImage("/Border_Pendulum_Medium_Gold");
                }
                */
            }
            else if (Current.Rarity == RARITY.UltimateRare)
            {
                /*
                if (Current.Attribute != null)
                    Attribute.Source = Uti.GetImage("/Attribute/" + Current.Attribute + "_Emboss" + Language);
                    */

                /*
                if (!Current.IsMagic && Current.Middle != null)
                {
                    string type = Current.Middle.Split('_')[0];
                    int number = Int32.Parse(Current.Middle.Split('_')[1]);
                    ChangeLevel(number, type);
                }
                if (!Current.IsFrame("Pendulum"))
                    Lore_Border.Source = Uti.GetImage("/Lore_Border_Emboss");
                    */
                CardName.Foreground = new SolidColorBrush(Color.FromRgb(69, 34, 12));//222, 178, 29));
                ArtworkFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Ultimate.png");
                ArtworkFoil.Opacity = 0.2;

                CardBorder.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Card/Emboss.png");
                //Frame.Source = Uti.GetImage("/Frame/" + Current.Frame + "_Emboss");
                //Frame.Source = G.ToBitmapSource(G.Brightness(G.ToBitmap(Uti.GetImage("/Frame/" + Current.Frame) as BitmapImage), -40, 100, 100));
                //Frame.Source = G.ToBitmapSource(G.Brightness(G.ToBitmap(Uti.GetBitmapImage(@"C:\Effect.ovpng")), -40));
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = 90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Colors.White;

                CardName.Effect = shadow;

                //Source = Graphic.ToBitmapSource(Graphic.EmbossArtwork(Graphic.ToBitmap(Source)));
                //Source = Graphic.ToBitmapSource(Graphic.Brightness(Graphic.ToBitmap(Source), -40));
                //Source = Graphic.ToBitmapSource(Graphic.Contrast(Graphic.ToBitmap(Source), -10));

                /*Source = Source.ToBitmap().EmbossArtwork().ToBitmapSource();
                Source = Source.ToBitmap().Brightness(-40).ToBitmapSource();
                Source = Source.ToBitmap().Contrast(-10).ToBitmapSource();*/
                Source = Source.ToBitmap().EmbossArtwork().Brightness(-40).Contrast(-10).ToBitmapSource();

            }

            else if (Current.Rarity == RARITY.GhostRare)
            {
                if (Source != null)
                {
                    //Source = Graphic.ToBitmapSource(Graphic.Brightness(Graphic.ToBitmap(Source), -110));
                    //Source = Graphic.ToBitmapSource(Graphic.ColorBalance(Graphic.ToBitmap(Source), 255, 0, -35));
                    //Source = Graphic.ToBitmapSource(Graphic.Contrast(Graphic.ToBitmap(Source), 10));
                    //Source = Graphic.Negative(Source);
                    Source = Source.ToBitmap().Brightness(-110)
                        .ColorBalance(255, 0, -35).Contrast(10).Negative().ToBitmapSource();
                }

                CardName.Foreground = new SolidColorBrush(Color.FromRgb(234, 255, 255));
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = 2;
                shadow.RenderingBias = RenderingBias.Quality;
                shadow.Direction = -90;
                shadow.ShadowDepth = 1.5;
                shadow.Color = Color.FromRgb(182, 162, 255);

                CardName.Effect = shadow;
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



            if (Current.Edition == EDITION.DuelTerminal)
            {
                CardFoil.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Foil/Shatter.png");
            }



            Artwork.Source = Source;
        }

        private void HandleFrame()
        {
            if (Current == null)
            {
                return;
            }
            Frame.Source = Images.GetImage(Utilities.GetLocationPath() + 
                "Template/Frame/" + Current.Frame.ToString() + ".png");
            if (Current.IsPendulum)
            {
                ArtworkBorder.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/Small.png");
                LoreBorder.Visibility = Visibility.Hidden;
                PendulumBoxMiddle.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/SmallBox.png");
                PendulumBoxText.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/TextBox.png");
                PendulumLine.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/Line"+ Current.Frame.ToString() + ".png");
                SpellHalf.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/SpellHalf.png");
                LoreBackground.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/Border/Pendulum/LoreBoxMedium.png");
                Artwork.Width = 707;
                Artwork.Height = 663;
                Canvas.SetLeft(Artwork, 53);
                Canvas.SetTop(Artwork, 212);
            }
            else
            {
                ArtworkBorder.Source = null;
                LoreBorder.Visibility = Visibility.Visible;
                PendulumBoxMiddle.Source = PendulumBoxText.Source = null;
                PendulumLine.Source = null;
                SpellHalf.Source = null;
                LoreBackground.Source = null;
                Artwork.Width = 620;
                Artwork.Height = 620;
                Canvas.SetLeft(Artwork, 98);
                Canvas.SetTop(Artwork, 217);
            }
            
        }

        private string SpecialPoint(double value)
        {
            string result = "";
            if (double.IsNaN(value))
            {
                result = "?";
            } else
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
            if (Current == null)
            {
                return;
            }
            this.Ability.Inlines.Clear();
            if (Current.IsMonster())
            {
                BracketLeft.Text = "["; BracketRight.Text = "]";
                double theWidth = 637;
                string futureText = null;

                {
                    string type = Current.Type != TYPE.NONE ? Current.Type.ToString() : "";// TransAbility(Current.Type);  

                    
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
                    if (Current.Frame != FRAME.Normal && Current.Frame != FRAME.Effect)
                    {
                        abilityText.Add(Current.Frame.ToString());
                    }
                    if (Current.IsPendulum)
                    {
                        abilityText.Add("Pendulum");
                    }
                    Current.Abilities.Sort();
                    foreach(ABILITY ability in Current.Abilities.Where(o => o != ABILITY.Effect && o != ABILITY.Tuner))
                    {
                        abilityText.Add(ability.ToString());
                    }
                    if (Current.Abilities.Contains(ABILITY.Tuner))
                    {
                        abilityText.Add(ABILITY.Tuner.ToString());
                    }

                    if (Current.Frame == FRAME.Effect || Current.Abilities.Contains(ABILITY.Effect))
                    {
                        abilityText.Add("Effect");
                    }

                    futureText = string.Join("_/_", abilityText);
                }
                int time = 0;

                futureText = futureText ?? "";
                double Default = 32; //12.2 - 11.4 | 31
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
            if (Current == null) { return; }
            if (Current.ArtworkByte == null)
            {
                Current.ArtworkByte = Images.GetImageByte(NoneImagePath);
                
            }
            Artwork.Source = Current.ArtworkByte.GetBitmapImageFromByteArray();
            
        }

        private void HandleMiddle()
        {
            if (Current == null)
            {
                return;
            }
            Middle.Children.Clear();

            //Property S/T
            if (!Current.IsMonster())
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

                string Ability = Current.Abilities.Count > 0 
                    ? Current.Abilities[0].ToString()
                    : "";


                double theWidth = 637;
                string futureText = Current.Frame.ToString() + " Card";
                //if (vietnameseLang.IsChecked == true)
                //{
                //    futureText = TransLanguage(Current.Frame.ToString());
                //}
                if (Current.Property != PROPERTY.NONE && Current.Property !=  PROPERTY.Normal)
                {
                    futureText += "    ";
                    Image property = new Image();
                    property.Source = Images.GetImage(Utilities.GetLocationPath()
                        + "Template/Middle/" + Current.Property.ToString() + ".png");
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

                if (!Double.IsNaN(Current.Level) || !Double.IsNaN(Current.Rank))
                {

                    int number = !Double.IsNaN(Current.Level) ? (int)Current.Level : (int)Current.Rank;

                    string type = Current.IsFrame(FRAME.Xyz) ? "Rank" : "Level";
                    for (int i = 1; i <= number; i++)
                    {
                        Image Star = new Image();
                        Star.Source = Images.GetImage(Utilities.GetLocationPath() + "Template/" + type + 
                            (Current.Rarity == RARITY.UltimateRare ? "Emboss" : null) + ".png");


                        Star.Width = MiddleBorder.Height - 3;
                        Star.Height = Star.Width;
                        Canvas.SetTop(Star, 2);
                        Middle.Children.Add(Star);

                        if (type == "Level")
                            Canvas.SetLeft(Star, Canvas.GetLeft(MiddleBorder)
                                + MiddleBorder.Width - (Star.Width + 0.6) * (i + 1.5));
                        else
                            Canvas.SetRight(Star, Canvas.GetLeft(MiddleBorder)
                                 + MiddleBorder.Width - (Star.Width + 0.6) * (i + 1.5));

                    }
                }

            }
        }
    

        private void HandleName()
        {
            if (Current == null)
            {

                return;
            }
            TextBlock Name = CardName;
            Name.Text = Current.Name;
            /*
            if (Current.Name.Length > 0)
            {
                int Position = TextBox.SelectionStart;
                if (Char.IsLower(TextBox.Text[0]) && TextBox.Text[0] != 'α' && TextBox.Text[0] != 'β' && TextBox.Text[0] != 'Ω')
                {
                    //if ()
                    TextBox.Text = TextBox.Text.Replace(TextBox.Text[0], Char.ToUpper(TextBox.Text[0]));
                    TextBox.SelectionStart = Position; // add some logic if length is 0
                    TextBox.SelectionLength = 0;
                    return;
                }
            }
            */



            FormattedText formattedText = new FormattedText
                (
                    Name.Text,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(Name.FontFamily, Name.FontStyle, Name.FontWeight, Name.FontStretch, null),
                    Name.FontSize,
                    Brushes.Black
                );

            if (formattedText.Width > NameViewbox.Width)
                Name.Width = double.NaN;
            else
                Name.Width = NameViewbox.Width;
        }

        private void HandleAttribute()
        {
            if (Current == null)
            {
                return;
            }
            Attribute.Source = Images.GetImage(Utilities.GetLocationPath() + 
                @"\Template\Attribute\" + Current.Attribute.ToString() + ".png");
            
            if (Current.Attribute !=ATTRIBUTE.UNKNOWN)
            {
                //AttributeText.Text = vietnameseLang.IsChecked == true ?
                //    TransLanguage(Current.Attribute.ToString()) : Current.Attribute.ToString();
                AttributeText.Text = Current.Attribute.ToString();
            }
            else
            {
                AttributeText.Text = null;
            }
        }
    }
}
