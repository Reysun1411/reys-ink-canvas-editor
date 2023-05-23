using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        
        public class ColorRGB
        {
            public byte alpha { get; set; }
            public byte red { get; set; }
            public byte green { get; set; }
            public byte blue { get; set; }
        }

        public ColorRGB mcolor { get; set; }
        public Color clr { get; set; }
        bool lineMode = false;
        private Point PreviousPoint, point;
        double pX, pY, tool, prevPointX, prevPointY, v;


        public MainWindow()
        {
            InitializeComponent();
            mcolor = new ColorRGB();
            mcolor.red = 0;
            mcolor.green = 0;
            mcolor.blue = 0;
            this.lbl1.Background = new SolidColorBrush(Color.FromRgb(mcolor.red, mcolor.green, mcolor.blue));
        }

        // Постоянно получает координаты мышки, когда она на холсте
        private void inkCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(inkCanvas1);
            pX = position.X;
            pY = position.Y;
        }

        // Получение координат, куда нажала мышка
        private void inkCanvas1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            prevPointX = pX;
            prevPointY = pY;
        }

        private void inkCanvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // line
            if (tool == 1)
            {
                StylusPointCollection strokePoints = new StylusPointCollection();
                strokePoints.Add(new StylusPoint(prevPointX, prevPointY));
                strokePoints.Add(new StylusPoint(pX, pY));

                Stroke line = new Stroke(strokePoints);
                DrawingAttributes attribs = new DrawingAttributes();
                attribs.Color = inkCanvas1.DefaultDrawingAttributes.Color;
                attribs.Width = inkCanvas1.DefaultDrawingAttributes.Width;
                attribs.Height = inkCanvas1.DefaultDrawingAttributes.Width;
                attribs.FitToCurve = false;
                line.DrawingAttributes = attribs;
                inkCanvas1.Strokes.Add(line);
            }
            // square
            else if (tool == 2)
            {
                StylusPointCollection strokePoints = new StylusPointCollection();
                strokePoints.Add(new StylusPoint(prevPointX, prevPointY));
                strokePoints.Add(new StylusPoint(prevPointX, pY));
                strokePoints.Add(new StylusPoint(pX, pY));
                strokePoints.Add(new StylusPoint(pX, prevPointY));
                strokePoints.Add(new StylusPoint(prevPointX, prevPointY));

                Stroke square = new Stroke(strokePoints);
                DrawingAttributes attribs = new DrawingAttributes();
                attribs.Color = inkCanvas1.DefaultDrawingAttributes.Color;
                attribs.Width = inkCanvas1.DefaultDrawingAttributes.Width;
                attribs.Height = inkCanvas1.DefaultDrawingAttributes.Width;
                attribs.FitToCurve = false;
                square.DrawingAttributes = attribs;
                inkCanvas1.Strokes.Add(square);
            }
        }


        // Обработчик прокручивания ползунков
        private void sld_Color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            string crlName = slider.Name;  // Определение имени ползунка
            double value = slider.Value;  // Определение значения прокрученного ползунка 

            // Выбор цвета, который изменится, изменение его с переводом в тип byte
            if (crlName.Equals("sld_RedColor"))
            {
                mcolor.red = Convert.ToByte(value);
            }
            else if (crlName.Equals("sld_GreenColor"))
            {
                mcolor.green = Convert.ToByte(value);
            }
            else if (crlName.Equals("sld_BlueColor"))
            {
                mcolor.blue = Convert.ToByte(value);
            }

            clr = Color.FromRgb(mcolor.red, mcolor.green, mcolor.blue);
            this.lbl1.Background = new SolidColorBrush(Color.FromRgb(mcolor.red, mcolor.green, mcolor.blue));
            this.lbl1.Content = Convert.ToString(clr);
            
            this.inkCanvas1.DefaultDrawingAttributes.Color = clr;
        }

        // Сохранение холста в .isf (собственный формат InkCanvas)
        private void btn_SaveInk_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Title = "Сохранить картинку в формате InkCanvas...";
            savedialog.FileName = "inkimage";
            savedialog.DefaultExt = ".isf";
            savedialog.OverwritePrompt = true;
            savedialog.CheckPathExists = true;
            savedialog.Filter = "isf files (*.isf)|*.isf";

            if (savedialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(savedialog.FileName, FileMode.Create);
                inkCanvas1.Strokes.Save(fs);
                fs.Close();
            }
        }

        // Загрузка isf в холст
        private void btn_Load_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog opendialog = new SaveFileDialog();
            opendialog.Title = "Загрузить картинку в формате InkCanvas...";
            opendialog.Filter = "isf files (*.isf)|*.isf";

            if (opendialog.ShowDialog() == true)
            {

                FileStream fs = new FileStream(opendialog.FileName,
                                               FileMode.Open);
                //var extension = System.IO.Path.GetExtension(opendialog.FileName);
                inkCanvas1.Strokes = new StrokeCollection(fs);
                fs.Close();
            }
        }

        // Очистка холста с подтверждением
        private void btn_CanvasClear_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgBoxResult = 
                System.Windows.MessageBox.Show("Вы уверены, что хотите очистить холст?", 
                "Подтверждение", System.Windows.MessageBoxButton.YesNo);

            if (msgBoxResult == MessageBoxResult.Yes)
            {
                inkCanvas1.Strokes.Clear();
            }
        }

        // Обработчик кнопок палитры
        private void btnPalette_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            // Задаем белый цвет в 2 типах: Color и Brush
            var cwhite = Color.FromRgb(255, 255, 255);
            var bwhite = new SolidColorBrush(cwhite);
            // Цвет, который сейчас используется
            var actualColor = inkCanvas1.DefaultDrawingAttributes.Color;
            // Конвертирование цвета кнопки из формата HEX в формат RGB для проверки на белый цвет
            var btncolor = (Color)ColorConverter.ConvertFromString(Convert.ToString(btn.Background));
            
            if (btncolor == cwhite || actualColor == cwhite)
            {
                // Если цвет кнопки белый, то он будет закрашен тем, что используется сейчас.
                // Также если юзер тыкнул по закрашенной кнопке белым, то кнопка становится белой (2 случай).
                btn.Background = new SolidColorBrush(actualColor);
            }
            else if (btn.Background != bwhite)
            {
                // Если на кнопку назначен цвет, то он будет использован
                Color clr = ((SolidColorBrush)btn.Background).Color;
                double r = Convert.ToDouble(clr.R);
                double g = Convert.ToDouble(clr.G);
                double b = Convert.ToDouble(clr.B);
                sld_RedColor.Value = r;
                sld_GreenColor.Value = g;
                sld_BlueColor.Value = b;
                inkCanvas1.DefaultDrawingAttributes.Color = clr;
            }
        }

        // Изменение цвета фона приложения
        private void btn_WindowBackgroundColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Background = new SolidColorBrush(inkCanvas1.DefaultDrawingAttributes.Color);
        }

        // Экспорт рисунка в png
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            this.inkCanvas1.EditingMode = InkCanvasEditingMode.None;
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Title = "Сохранить картинку как...";
            savedialog.FileName = "savedimage";
            savedialog.DefaultExt = ".png";
            savedialog.OverwritePrompt = true;
            savedialog.CheckPathExists = true;
            savedialog.Filter = "Image (.png)|*.png";

            Nullable<bool> result = savedialog.ShowDialog();

            if (result == true)
            {
                string fileName = savedialog.FileName;

                var size = new Size(inkCanvas1.ActualWidth, inkCanvas1.ActualHeight);
                inkCanvas1.Margin = new Thickness(0, 0, 0, 0);
                inkCanvas1.Measure(size);
                inkCanvas1.Arrange(new Rect(size));
                var encoder = new PngBitmapEncoder();
                var bitmapTarget = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Default);
                bitmapTarget.Render(inkCanvas1);
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapTarget));
                    encoder.Save(fs);
                }
                inkCanvas1.Margin = new Thickness(234, 0, 0, 0);
                this.inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
                MessageBox.Show("Файл сохранен");
            }

        }

        // Режимы
        private void Mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = sender as RadioButton;
            string modeName = mode.Name;  // Определение имени ползунка

            // Кисть
            if (modeName.Equals("rbtn_Ink"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
                tool = 0;
            }
            // Линия
            else if (modeName.Equals("rbtn_Stroke"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.None;
                tool = 1;
            }
            // Квадрат
            else if (modeName.Equals("rbtn_Square"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.None;
                tool = 2;
            }
            // Круг (еще не придумал как сделать его чернилами)
            else if (modeName.Equals("rbtn_Circle"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.None;
                tool = 3;
            }
            // Выделение
            else if (modeName.Equals("rbtn_Select"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.Select;
                tool = 0;
            }
            // Стерка
            else if (modeName.Equals("rbtn_Eraser"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                tool = 5;
            }
            // Стерка штрихов
            else if (modeName.Equals("rbtn_GigaEraser"))
            {
                inkCanvas1.EditingMode = InkCanvasEditingMode.EraseByStroke;
                tool = 0;
            }
        }

        private void sld_BrushSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Изменение толщины кисти
            var slider = sender as Slider;
            inkCanvas1.DefaultDrawingAttributes.Width = slider.Value;

            // А также размера ластика
            if (tool == 5)
            {
                // Если пользователь уже в режиме ластика, то режим надо переназначить,
                // чтобы он принял изменения ластика. Иначе не примит
                inkCanvas1.EditingMode = InkCanvasEditingMode.None;
                inkCanvas1.EraserShape = new RectangleStylusShape(slider.Value, slider.Value);
                inkCanvas1.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else
            {
                // Если же он не в режиме ластика, то можно сразу менять его толщину
                inkCanvas1.EraserShape = new RectangleStylusShape(slider.Value, slider.Value);
            }
        }

        // Изменение разрешения полотна
        private void txt_CanvasSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtbox = sender as TextBox;
            string boxname = txtbox.Name;

            if (String.IsNullOrEmpty(txtbox.Text) || Convert.ToDouble(txtbox.Text) <= 0)
            {
                txtbox.Text = "1";
            }
            else if (Convert.ToDouble(txtbox.Text) > 2160)
            {
                txtbox.Text = "2160";
            }

            double val = Convert.ToDouble(txtbox.Text);

            if (boxname == "txt_CanvasWidth")
            {
                inkCanvas1.Width = val;
            }
            else if (boxname == "txt_CanvasHeight")
            {
                inkCanvas1.Height = val;
            }
        }
        

        private void txtOnlyDigit_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
    
}
