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
            public byte red { get; set; }
            public byte green { get; set; }
            public byte blue { get; set; }
        }

        public ColorRGB mcolor { get; set; }
        public Color clr { get; set; }
        double pX, pY, tool, prevPointX, prevPointY;

        // Списки List штрихов для кнопок Undo и Redo
        List<System.Windows.Ink.StrokeCollection> strokeAdded = new List<System.Windows.Ink.StrokeCollection>(10);
        List<System.Windows.Ink.StrokeCollection> strokeRemoved = new List<System.Windows.Ink.StrokeCollection>(10);
        private bool handle = true;


        public MainWindow()
        {
            InitializeComponent();
            // Добавления обработчика нажатия клавиш
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            mcolor = new ColorRGB();
            mcolor.red = 0;
            mcolor.green = 0;
            mcolor.blue = 0;
            this.lbl1.Background = new SolidColorBrush(Color.FromRgb(mcolor.red, mcolor.green, mcolor.blue));
            inkCanvas1.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        // Постоянно получает координаты мышки, когда она на холсте
        private void inkCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(inkCanvas1);
            pX = position.X;
            pY = position.Y;
        }

        // Фиксация добавления штрихов на полотно. Новые штрихи добавляются в список strokeAdded
        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            if (handle)
            {
                if (strokeAdded.Count == 10)
                    // Если кол-во штрихов в strokeAdded достигло 10, то удаляется первый штрих в списке 
                    strokeAdded.RemoveAt(0);
                strokeAdded.Add(e.Added);
                strokeRemoved.Clear();
            }
        }

        // Обработка нажатий клавиш
        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl + Z
                if (e.Key == Key.Z)
                {
                    Undo();
                }
                // Ctrl + Y
                else if (e.Key == Key.Y)
                {
                    Redo();
                }
            }
        }

        // Undo - Отмена
        private void btn_Undo(object sender, MouseButtonEventArgs e)
        {
            Undo();
        }

        private void btn_Redo(object sender, MouseButtonEventArgs e)
        {
            Redo();
        }

        private void Undo()
        {
            handle = false;
            if (strokeAdded.Count > 0)
            {
                int lastelement = strokeAdded.Count - 1;

                // Последний штрих в strokeAdded удаляется из полотна
                inkCanvas1.Strokes.Remove(strokeAdded[lastelement]);
                // И затем заносится в список strokeRemoved
                strokeRemoved.Add(strokeAdded[lastelement]);
                // А из strokeAdded уаляется
                strokeAdded.RemoveAt(lastelement);
            }
            handle = true;
        }

        // Redo - Повтор
        private void Redo()
        {
            handle = false;
            if (strokeRemoved.Count > 0)
            {
                int lastelement = strokeRemoved.Count - 1;

                // Последний элемент strokeRemoved возвращается на полотно
                inkCanvas1.Strokes.Add(strokeRemoved[lastelement]);
                // Затем добавляется в strokeAdded
                strokeAdded.Add(strokeRemoved[lastelement]);
                // И удаляется из strokeRemoved
                strokeRemoved.RemoveAt(lastelement);
            }
            handle = true;
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
            string hexclr = Convert.ToString(clr);
            lbl1.Background = new SolidColorBrush(Color.FromRgb(mcolor.red, mcolor.green, mcolor.blue));
            lbl1.Content = hexclr.Remove(1, 2);
            inkCanvas1.DefaultDrawingAttributes.Color = clr;
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
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.Title = "Загрузить картинку в формате InkCanvas...";
            opendialog.CheckPathExists = true;
            opendialog.Filter = 
                "Image Files|*.jpg;*.jpeg;*.png|isf files (*.isf)|*.isf";

            if (opendialog.ShowDialog() == true)
            {
                // FileStream открывает файл в режиме чтения
                FileStream fs = new FileStream(opendialog.FileName, FileMode.Open, 
                    FileAccess.Read, FileShare.ReadWrite);
                var extension = System.IO.Path.GetExtension(opendialog.FileName);

                // Из файла isf (расширение inkcanvas) перенесутся все штрихи на полотно
                if (extension == ".isf")
                {
                    inkCanvas1.Strokes = new StrokeCollection(fs);
                }
                // Остальное - файлы изображения
                else
                {
                    MessageBoxResult result = MessageBox.Show("Подстроить размер полотна под картинку?", 
                        "Вставка изображения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var decoder = BitmapDecoder.Create(fs, 
                            BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                        // Получение размера изображения для настройки холста
                        int imgW = decoder.Frames[0].PixelWidth;
                        int imgH = decoder.Frames[0].PixelHeight;
                        if (imgW > 3840)
                        {
                            imgW = 3840;
                        }
                        if (imgH > 2160)
                        {
                            imgH = 2160;
                        }
                        txt_CanvasWidth.Text = Convert.ToString(imgW);
                        txt_CanvasHeight.Text = Convert.ToString(imgH);
                        inkCanvas1.Width = imgW;
                        inkCanvas1.Height = imgH;
                    }
                    // Загрузка изображения как бекграунда
                    ImageBrush canvasbg = new ImageBrush();
                    canvasbg.ImageSource = new BitmapImage(new Uri(opendialog.FileName, UriKind.Relative));
                    inkCanvas1.Background = canvasbg;
                }
                fs.Close();
            }
        }

        // Очистка холста с подтверждением
        private void btn_CanvasClear_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgBoxResult = 
                MessageBox.Show("Вы уверены, что хотите очистить холст?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

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
            
            if (btncolor == cwhite & actualColor != cwhite)
            {
                // Если цвет кнопки белый, то он будет закрашен тем, что используется сейчас.
                // Также если юзер тыкнул по закрашенной кнопке белым, то кнопка становится белой (2 случай).
                btn.Content = string.Empty;
                btn.Background = new SolidColorBrush(actualColor);
            }
            else if (btncolor != cwhite & actualColor == cwhite)
            {
                btn.Content = "⬜";
                btn.Background = bwhite;
            }
            else if (btncolor != cwhite & actualColor != cwhite)
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

        // Вкл/выкл сглаживание штрихов
        private void checkFitToCurve_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true) 
            { 
                inkCanvas1.DefaultDrawingAttributes.FitToCurve = true;
            }
            else
            {
                inkCanvas1.DefaultDrawingAttributes.FitToCurve = false;
            }
            
        }

        // Изменение цвета фона приложения
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Background = new SolidColorBrush(inkCanvas1.DefaultDrawingAttributes.Color);
        }

        // Изменение фона полотна
        private void btn_WindowBackgroundColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkCanvas1.Background = new SolidColorBrush(inkCanvas1.DefaultDrawingAttributes.Color);
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
                var bitmapTarget = new RenderTargetBitmap((int)size.Width, 
                    (int)size.Height, 96, 96, PixelFormats.Default);
                bitmapTarget.Render(inkCanvas1);
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapTarget));
                    encoder.Save(fs);
                }
                inkCanvas1.Margin = new Thickness(234, 0, 0, 0);
                this.inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
                MessageBox.Show("Файл сохранен", "Успех", MessageBoxButton.OK);
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

        // Слайдер размера кисти
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

            // Защита от пустоты
            if (String.IsNullOrEmpty(txtbox.Text))
            {
                txtbox.Text = "1";
            }

            // Сюда уже поступают только циферные значения благодаря txtOnlyDigit
            double val = Convert.ToDouble(txtbox.Text);

            // По имени текстбокса применяется ограничение по высоте и ширине (4к)
            // И значение сразу применяется к полотну
            if (boxname == "txt_CanvasWidth")
            {
                if (val > 3840) 
                {
                    txtbox.Text = "3840";
                    val = 3840;
                }
                inkCanvas1.Width = val;
                
            }
            else if (boxname == "txt_CanvasHeight")
            {
                if (val > 2160)
                {
                    txtbox.Text = "2160";
                    val = 2160;
                }
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
