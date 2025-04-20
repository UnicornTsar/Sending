using System;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Sending
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static String appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //String image_adress1 = appDir + "\\example.bmp";
        //String image_adress = "C:\\Users\\rodio\\OneDrive\\Рабочий стол\\example.bmp";
        //String txt_adress = "C:/Users/rodio/OneDrive/Рабочий стол/example_text.txt";
        String key1_backup1 = "м|/|JI/-|Я";
        String key1_backup2 = "13";
        //String key2 = "|<0Р0В|<А";
        int Block_Height = 10;
        int Block_Width = 10;
        int Shifter = 0;
        int K = 9;

        public MainWindow()
        {
            InitializeComponent();

            Choose_bite.ItemsSource = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Choose_bite.SelectedIndex = 0;
            Choose_color.ItemsSource = new char[] { 'R', 'G', 'B' };
            Choose_color.SelectedIndex = 2;
        }

        private int[] CharToBits(char c)
        {
            int[] res = new int[sizeof(char) * 8];
            int mask = 1;
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = ((int)(c & mask)) << Shifter;
                c >>= 1;
            }
            return res;
        }
        private char BitsToChar(int[] bits)
        {
            int res = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                res += (bits[i] >> Shifter) << i;
            }
            return ((char)res);
        }
        public static Bitmap LoadBitmap(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return new Bitmap(fs);
        }
        private void Hide(object sender, RoutedEventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
            //StreamReader strreader = new StreamReader(txt_adress);
            //String text = strreader.ReadToEnd();
            if(Key1.Text.Length < 3)
            {
                MessageBoxResult key_problem = MessageBox.Show("Некачественный ключ: "+ Key1.Text, "", MessageBoxButton.OKCancel);
                if(key_problem == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            if (Key2.Text.Length < 3)
            {
                MessageBoxResult key_problem = MessageBox.Show("Некачественный ключ: "+ Key2.Text, "", MessageBoxButton.OKCancel);
                if (key_problem == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            String text = Key1.Text + Text.Text + Key2.Text;

            int i = 1;
            int x = 0;
            int y = 0;
            int width = bmp.Width;
            int height = bmp.Height;
            int charsize = sizeof(char) * 8;
            int[] buffer = CharToBits(text[0]);
            int mask = (-1 << Shifter) - 1;
            for (int bitcount = 0; i <= text.Length;)
            {
                int pixel = 0;
                try
                {
                    pixel = bmp.GetPixel(x, y).ToArgb();
                }
                catch
                {
                    MessageBox.Show("Размер сообщения привысил размер контейнера");
                    return;
                }
                pixel &= mask;
                pixel |= buffer[bitcount];
                bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel));
                x++;
                if (x == bmp.Width)
                {
                    y++;
                    x = 0;
                }

                bitcount++;
                bitcount %= charsize;
                if (bitcount == 0)
                {
                    if(i!=text.Length)
                        buffer = CharToBits(text[i]);
                    i++;
                }
            }

            //StringBuilder error = new StringBuilder(height*width);
            var rand = new Random();
            for (; y < bmp.Height;)
            {
                int rand_bit = rand.Next(0, 2);
                //error.Append(rand_bit.ToString());

                int pixel = bmp.GetPixel(x, y).ToArgb();
                pixel &= mask;
                pixel |= (rand_bit << Shifter);
                bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel));
                x++;
                if (x == bmp.Width)
                {
                    y++;
                    x = 0;
                }
            }
            File.Delete(Image_Adress.Text);
            bmp.Save(Image_Adress.Text);
            Text.Text = "Сообщение спрятано";//error.ToString();
            //MessageBox.Show(text, "");
        }

        private void Find(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = LoadBitmap(Image_Adress.Text);
            String text = "";

            int x = 0;
            int y = 0;
            int charsize = sizeof(char) * 8;
            int[] buffer = new int[charsize];
            int mask = 1 << Shifter;
            for (int bitcount = 0; y < bmp.Height; )
            {
                int pixel = bmp.GetPixel(x, y).ToArgb();
                buffer[bitcount] = pixel & mask;
                x++;
                if (x == bmp.Width)
                {
                    y++;
                    x = 0;
                }

                bitcount++;
                if (bitcount == charsize)
                {
                    text += BitsToChar(buffer);
                    bitcount = 0;
                }
            }
            try
            {
                text = text.Split(new string[] { Key1.Text, Key2.Text }, StringSplitOptions.None)[1];
            }
            catch
            {
                MessageBox.Show("По данным ключам сообщения не найдено");
                return;
            }
            Text.Text = text;
            //MessageBox.Show(text, "");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.InitialDirectory = appDir;
            fileDialog.DefaultExt = ".bmp";
            fileDialog.Filter = "Bitmap (.bmp)|*.bmp";

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                Image_Adress.Text = fileDialog.FileName;
            }
        }

        private void LSB_Method(object sender, RoutedEventArgs e)
        {
            bool is_loaded = true;
            foreach (MenuItem item in Methods.Items)
            {
                if(item != sender && item.IsChecked)
                {
                    is_loaded = false;
                    item.IsChecked = false;
                }
            };
            if (is_loaded)
            {
                e.Handled = true;
                return;
            }

            //Key1.Visibility = Visibility.Visible;
            //Key2.Visibility = Visibility.Visible;
            //KeyRow.Height = new GridLength(25);
            Key1.Text = key1_backup1;

            Hide_Button.Click -= Hide_Block;
            Hide_Button.Click -= Hide_PRI;
            Hide_Button.Click += Hide;

            Find_Button.Click -= Find_Block;
            Find_Button.Click -= Find_PRI;
            Find_Button.Click += Find;

            Capasity_button.Click -= Capasity_button_Click_Block;
            Capasity_button.Click -= Capasity_button_Click_PRI;
            Capasity_button.Click += Capasity_button_Click;
        }

        private void Block_Method(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in Methods.Items)
            {
                if (item != sender && item.IsChecked)
                    item.IsChecked = false;
            };
            //Key1.Visibility = Visibility.Collapsed;
            //Key2.Visibility = Visibility.Collapsed;
            //KeyRow.Height = new GridLength(0);
            Key1.Text = key1_backup1;


            Hide_Button.Click -= Hide;
            Hide_Button.Click -= Hide_PRI;
            Hide_Button.Click += Hide_Block;

            Find_Button.Click -= Find;
            Find_Button.Click -= Find_PRI;
            Find_Button.Click += Find_Block;

            Capasity_button.Click -= Capasity_button_Click;
            Capasity_button.Click -= Capasity_button_Click_PRI;
            Capasity_button.Click += Capasity_button_Click_Block;
        }

        private void Hide_Block(object sender, RoutedEventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
            //StreamReader strreader = new StreamReader(txt_adress);
            //String text = strreader.ReadToEnd();

            if (Key1.Text.Length < 3)
            {
                MessageBoxResult key_problem = MessageBox.Show("Некачественный ключ: "+ Key1.Text, "", MessageBoxButton.OKCancel);
                if (key_problem == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            if (Key2.Text.Length < 3)
            {
                MessageBoxResult key_problem = MessageBox.Show("Некачественный ключ: "+ Key2.Text, "", MessageBoxButton.OKCancel);
                if (key_problem == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            String text = Key1.Text + Text.Text + Key2.Text;

            int i = 1;
            int x = 0;
            int y = 0;
            int width = bmp.Width;
            int height = bmp.Height;
            int charsize = sizeof(char) * 8;
            int[] buffer = CharToBits(text[0]);
            int bitchanger = (1 << Shifter);
            for (int bitcount = 0; i <= text.Length;)
            {

                int blockbit = 0;
                try
                {
                    blockbit = Block_xor(bmp, x, x + Block_Width, y, y + Block_Height);
                }
                catch
                {
                    MessageBox.Show("Размер сообщения привысил размер контейнера");
                    return;
                }

                if (buffer[bitcount] != blockbit)
                {
                    int pixel = bmp.GetPixel(x, y).ToArgb();
                    pixel ^= bitchanger;
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel));
                }

                x+=Block_Width;
                if (x + Block_Width >= bmp.Width)
                {
                    y+=Block_Height;
                    x = 0;
                }

                bitcount++;
                bitcount %= charsize;
                if (bitcount == 0)
                {
                    if (i != text.Length)
                        buffer = CharToBits(text[i]);
                    i++;
                }
            }
            File.Delete(Image_Adress.Text);
            bmp.Save(Image_Adress.Text);
            Text.Text = "Сообщение спрятано";
            //MessageBox.Show(text, "");
        }

        private void Find_Block(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = LoadBitmap(Image_Adress.Text);
            String text = "";

            int x = 0;
            int y = 0;
            int charsize = sizeof(char) * 8;
            int[] buffer = new int[charsize];
            for (int bitcount = 0; y + Block_Height < bmp.Height; )
            {
                buffer[bitcount] = Block_xor(bmp, x, x + Block_Width, y, y + Block_Height);

                x += Block_Width;
                if (x + Block_Width >= bmp.Width)
                {
                    y += Block_Height;
                    x = 0;
                }

                bitcount++;
                if (bitcount == charsize)
                {
                    text += BitsToChar(buffer);
                    bitcount = 0;
                }
            }
            try {
                text = text.Split(new string[] { Key1.Text, Key2.Text }, StringSplitOptions.None)[1];
                }
            catch 
            {
                MessageBox.Show("По данным ключам сообщения не найдено");
                return;
            }
            Text.Text = text;
            //MessageBox.Show(text, "");
        }
        private int Block_xor(Bitmap bitmap, int x1, int x2, int y1, int y2)
        {
            int res = 0;
            int x = x1;
            int y = y1;
            while (y < y2)
            {
                res ^= bitmap.GetPixel(x, y).ToArgb();
                x++;
                if (x == x2)
                {
                    y++;
                    x = x1;
                }
            }

            int mask = (1 << Shifter);
            return res & mask;
        }

        private void UpdateShifter(object sender, SelectionChangedEventArgs e)
        {
            Shifter = Choose_bite.SelectedIndex + (2 - Choose_color.SelectedIndex) * 8;
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void PRI_Method(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in Methods.Items)
            {
                if (item != sender && item.IsChecked)
                    item.IsChecked = false;
            }

            Key1.Text = key1_backup2;

            Hide_Button.Click -= Hide;
            Hide_Button.Click -= Hide_Block;
            Hide_Button.Click += Hide_PRI;

            Find_Button.Click -= Find;
            Find_Button.Click -= Find_Block;
            Find_Button.Click += Find_PRI;

            Capasity_button.Click -= Capasity_button_Click;
            Capasity_button.Click -= Capasity_button_Click_Block;
            Capasity_button.Click += Capasity_button_Click_PRI;
        }

        private void Hide_PRI(object sender, RoutedEventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
            //StreamReader strreader = new StreamReader(txt_adress);
            //String text = strreader.ReadToEnd();

            if (Key2.Text.Length < 3)
            {
                MessageBoxResult key_problem = MessageBox.Show("Некачественный ключ: "+ Key2.Text, "", MessageBoxButton.OKCancel);
                if (key_problem == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            String text = Text.Text + Key2.Text;

            int i = 1;
            int x;
            bool check = int.TryParse(Key1.Text, out x);
            if(!check || x <= 0)
            {
                MessageBox.Show("Первый ключ должен быть положительным ненулевым числом");
                e.Handled = true;
                return;
            }
            int y = x / bmp.Width;
            x %= bmp.Width;

            int width = bmp.Width;
            int height = bmp.Height;
            //StringBuilder error = new StringBuilder(height * width);
            int charsize = sizeof(char) * 8;
            int[] buffer = CharToBits(text[0]);
            int mask = (-1 << Shifter) - 1;
            //error.AppendLine(text[0].ToString());
            //error.AppendLine("---");
            for (int bitcount = 0; i <= text.Length;)
            {
                int pixel = 0;
                try
                {
                    pixel = bmp.GetPixel(x, y).ToArgb();
                }
                catch
                {
                    MessageBox.Show("Размер сообщения привысил размер контейнера");
                    return;
                }
                pixel &= mask;
                pixel |= buffer[bitcount];
                bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel));
                //error.AppendLine( x.ToString() + " " + y.ToString());
                x += GetStep(x,y);
                if (x >= bmp.Width)
                {
                    y += x / width;
                    x %= width;
                }

                bitcount++;
                bitcount %= charsize;
                if (bitcount == 0)
                {
                    if (i != text.Length)
                        buffer = CharToBits(text[i]);
                    i++;
                }
            }
            File.Delete(Image_Adress.Text);
            bmp.Save(Image_Adress.Text);
            Text.Text = "Сообщение спрятано";
            //MessageBox.Show(text, "");
        }
        private void Find_PRI(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = LoadBitmap(Image_Adress.Text);
            String text = "";

            int x;
            bool check = int.TryParse(Key1.Text, out x);
            if (!check || x <= 0)
            {
                MessageBox.Show("Первый ключ должен быть положительным ненулевым числом");
                e.Handled = true;
                return;
            }
            int y = x / bmp.Width;
            x %= bmp.Width;

            int charsize = sizeof(char) * 8;
            int[] buffer = new int[charsize];
            int mask = 1 << Shifter;
            for (int bitcount = 0; y + Block_Height < bmp.Height; )
            {
                int pixel = bmp.GetPixel(x, y).ToArgb();
                buffer[bitcount] = pixel & mask;
                x += GetStep(x, y);
                if (x >= bmp.Width)
                {
                    y += x / bmp.Width;
                    x %= bmp.Width;
                }

                bitcount++;
                if (bitcount == charsize)
                {
                    text += BitsToChar(buffer);
                    bitcount = 0;
                }
            }
            try
            {
                text = text.Split(new string[] { Key2.Text }, StringSplitOptions.None)[0];
            }
            catch 
            {
                MessageBox.Show("По данным ключам сообщения не найдено");
                return;
            }
            Text.Text = text;
            //MessageBox.Show(text, "");
        }
        private int GetStep(int x, int y)
        {
            int step = 0;
            while(x > 0)
            {
                step += (x % 2);
                x >>= 1;
            }
            while (y > 0)
            {
                step += (y % 2);
                y >>= 1;
            }
            return step * K;
        }

        private void Image_Adress_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string path in droppedFilePaths)
                {
                    if (path.EndsWith(".bmp"))
                    {
                        Image_Adress.Text = path;
                        return;
                    }
                }
            }
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Length_label.Content= Text.Text.Length;
        }

        private void Capasity_button_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);
                MessageBox.Show("Вместимость контейнера с этими ключами - " + (bmp.Width * bmp.Height / sizeof(char) - Key1.Text.Length - Key2.Text.Length) + " символов");
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
        }
        private void Capasity_button_Click_Block(object sender, RoutedEventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);

                int cap = (bmp.Height / Block_Height) * (bmp.Width / Block_Width) - Key1.Text.Length - Key2.Text.Length;

                MessageBox.Show("Вместимость контейнера с этими ключами - " + cap + " символов");
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
        }
        private void Capasity_button_Click_PRI(object sender, RoutedEventArgs e)
        {
            int x = 0;
            bool check = int.TryParse(Key1.Text, out x);
            if (!check || x <= 0)
            {
                MessageBox.Show("Первый ключ должен быть положительным ненулевым числом");
                e.Handled = true;
                return;
            }

            Bitmap bmp;
            try
            {
                bmp = LoadBitmap(Image_Adress.Text);

                int y = x / bmp.Width;
                x = x % bmp.Width;
                int cap = 0;
                while (y < bmp.Height)
                {
                    cap++;

                    x += GetStep(x, y);
                    if (x >= bmp.Width)
                    {
                        y += x / bmp.Width;
                        x %= bmp.Width;
                    }
                }
                cap -= Key2.Text.Length;

                MessageBox.Show("Вместимость контейнера с этими ключами - " + cap + " символов");
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла-контейнера");
                e.Handled = true;
                return;
            }
        }

        private void Image_Adress_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
