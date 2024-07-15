using date_update.Properties;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WinForms = System.Windows.Forms;

namespace date_update.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        //путь к папке Dynamic, в которой динамически будет создаваться промежуточный файл
        DirectoryInfo directoryDynamic = new DirectoryInfo(@"Dynamic//");
        //переменная для пути к созданному файлу
        string fileWay = null;

        public MainPage()
        {
            InitializeComponent();
            //скрытие stackpanel и richtextbox и button
            stpMain.Visibility = Visibility.Collapsed;
            txbText.Visibility = Visibility.Collapsed;
            btnOpenFile.Visibility = Visibility.Collapsed;
            //если переменная не пустая
            if (Settings.Default["way"] != null)
            {
                //то путь до папки, куда будет сохраняться измененный файл, будет как в предыдущий раз
                txbWay.Text = (string)Settings.Default["way"];
            }
        }

        //выбор файла
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in directoryDynamic.GetFiles())//проходим по файлам
            {
                //все содержимое папки Dynamic удаляется
                System.IO.File.Delete(f.FullName);
            }
            //переменная для полного пути до папки Dynamic
            string path = directoryDynamic.FullName;
            //создание диалогового окна для выбора файла (возможен выбор нескольких файлов, но интерфейс программы расчитан на одиночные файлы)
            OpenFileDialog OpDi = new OpenFileDialog { Multiselect = true };
            //установление форматов файлов, которые будут видны в диалоговом окне
            OpDi.Filter = "Файлы в формате (*.gpx)|*.gpx;";
            //если окно открыто
            if (OpDi.ShowDialog() == true)
            {
                //проход по всем выбраным файлам
                foreach (String file in OpDi.FileNames)
                {
                    //переменная, куда присваивается полный путь до файла, который будет изменен
                    string fileToMove = file;
                    //переменная, куда присваивается название файла
                    string s = System.IO.Path.GetFileName(file);
                    //переменная, которая содержит в себе путь до файла, который создастся в папке Dynamic в формате .txt
                    string tomove = path + s.Remove(s.LastIndexOf('.')) + ".txt";
                    //само создание файла (путь до файла исходного, путь до преобразованного файла)
                    File.Copy(fileToMove, tomove);

                    foreach (var f in directoryDynamic.GetFiles())//проходим по файлам в папке Dynamic
                    {
                        //чтение файла из папки
                        StreamReader sr = new StreamReader(f.FullName);
                        //переменная для строк
                        string line;
                        //переменная для количества изменений
                        int num = 0;
                        //пока чтение файла не закончилось
                        while (!sr.EndOfStream)
                        {
                            //чтение построчно
                            line = sr.ReadLine();
                            //если в строке содержится "<time>" то к переменной num добавляется +1
                            if (line.Contains("<time>")) num++;
                        }
                        //закрытие файла
                        sr.Close();
                        //заполнение лейбла с полным путем до файла
                        lblFullName.Content = "Полный путь: " + fileToMove;
                        //заполнение лейбла с количеством замененных дат
                        lblQuantityReplace.Content = "Заменить потребуется: " + num.ToString();
                        //вызов метода для заполнения richtextbox (в скобках путь до файла в паке Dynamic в формате .txt, который надо открыть)
                        LoadTxt(f.FullName);
                        //изменение tooltip richtextbox'а на название открытого файла
                        txbText.ToolTip = s.Remove(s.LastIndexOf('.'));
                        //показывает stackpanel и richtextbox
                        stpMain.Visibility = Visibility.Visible;
                        txbText.Visibility = Visibility.Visible;
                        //заполнение поля с названием измененного файла и в конце +(Changed). Пользователь может изменить название при желании
                        txbFileName.Text = s.Remove(s.LastIndexOf('.')) + " (Changed)";
                    }
                }
            }
        }

        //метод для заполнения richtextbox (в скобках полный путь до файла)
        private void LoadTxt(string fileName)
        {
            //чтение файла
            using (StreamReader reader = new StreamReader(fileName))
            {
                //чтение построчно
                string text = reader.ReadToEnd();
                //хз че тут происходит
                FlowDocument document = new FlowDocument();
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(text));
                document.Blocks.Add(paragraph);
                //само заполнение rixhtextbox'а готовым текстом
                txbText.Document = document;
            }
        }

        //кнопка изменения
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            //если поле с путем до папки не заполнено
            if(txbWay.Text == null || txbWay.Text == "")
            {
                //вывод сообщения
                MessageBox.Show("Путь сохранения файла не выбран!");
            }
            //если заполнено
            else
            {
                //если дата выбрана
                if (dtpDate.SelectedDate != null)
                {
                    foreach (var f in directoryDynamic.GetFiles())//проходим по файлам
                    {
                        //чтение файла
                        StreamReader reader = new StreamReader(f.FullName);
                        //просмотр построчно
                        string content = reader.ReadToEnd();
                        //закрытие файла
                        reader.Close();
                        //переменная для даты, чтобы потом преобразовать ее
                        DateTime date = dtpDate.SelectedDate.Value;
                        //преобразование datetime в формат ISO 8601
                        var utcTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);
                        //в строках, где содержится дата формата ISO 8601 заменить эту дату на дату из переменной utcTime.ToString("yyyy-MM-dd") 
                        content = Regex.Replace(content, @"\d{4}-\d{2}-\d{2}", utcTime.ToString("yyyy-MM-dd"));

                        //переменная для проверки есть ли файл с таким названием в папке
                        string filePath = Path.Combine(txbWay.Text, txbFileName.Text + ".gpx");
                        //если такого файла нет
                        if (!File.Exists(filePath))
                        {
                            //запись файла (в скобках: путь к выбранной папке, название измененного файла, формат .gpx)
                            StreamWriter writer = new StreamWriter(txbWay.Text + "/" + txbFileName.Text + ".gpx");
                            //уведомление об успешности
                            MessageBox.Show("Файл успешно изменен!");
                            //непосредственно сама запись файла
                            writer.Write(content);
                            //путь к созданному файлу
                            fileWay = txbWay.Text + "/" + txbFileName.Text + ".gpx";
                            //закрытие файла
                            writer.Close();
                            //вызов метода для заполнения richtextbox (в скобках путь до только что созданного файла)
                            LoadTxt(txbWay.Text + "/" + txbFileName.Text + ".gpx");
                            //изменение tooltip richtextbox'a на название только что созданного файла
                            txbText.ToolTip = txbFileName.Text;
                            //запуск папки, в которую создался файл (выбрал пользователь)
                            Process.Start(txbWay.Text + "/");
                            //удаление промежуточного файла из папки Dynamic
                            //System.IO.File.Delete(f.FullName);
                            //видимость кнопки для открытия файла
                            btnOpenFile.Visibility = Visibility.Visible;
                        }
                        //если такой файл есть
                        else
                        {
                            //сообщение об ошибке
                            MessageBox.Show("Файл с таким названием уже существует!");
                        }
                    }
                }
                //если не выбрана
                else
                {
                    //вывод сообщения
                    MessageBox.Show("Нужно выбрать дату!");
                }
            }
        }

        //кнопка выхода
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in directoryDynamic.GetFiles())//проходим по файлам
            {
                //удаляем все файлы а папке Dynamic
                System.IO.File.Delete(f.FullName);
            }
            //переменную в сеттингах приравниваем к null
            Settings.Default["way"] = null;
            //сохраняем ее
            Settings.Default.Save();
            //выход из приложения
            App.Current.Shutdown();
        }

        //кнопка выбора пути
        private void btnSelectWay_Click(object sender, RoutedEventArgs e)
        {
            //создание диалогового окна для выбора папки
            WinForms.FolderBrowserDialog folderBrowserDialog = new WinForms.FolderBrowserDialog();
            //если диалоговое окно открыто
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //заполнение поля пути до папки
                txbWay.Text = folderBrowserDialog.SelectedPath;
                //сохранение в сеттингах этого пути, чтобы при повторном входе можно было не выбирать путь заново
                Settings.Default["way"] = txbWay.Text;
                //само сохранение
                Settings.Default.Save();
            }
        }

        //кнопка для открытия файла сторонней программой
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            //запуск созданного файла
            //OpenAs(fileWay);
            Process.Start(fileWay);
        }

        [Serializable]
        public struct ShellExecuteInfo
        {
            public int Size;
            public uint Mask;
            public IntPtr hwnd;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public uint Show;
            public IntPtr InstApp;
            public IntPtr IDList;
            public string Class;
            public IntPtr hkeyClass;
            public uint HotKey;
            public IntPtr Icon;
            public IntPtr Monitor;
        }

        // Code For OpenWithDialog Box

        [DllImport("shell32.dll", SetLastError = true)]
        extern public static bool
               ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

        public const uint SW_NORMAL = 1;

        static void OpenAs(string file)
        {
            ShellExecuteInfo sei = new ShellExecuteInfo();
            sei.Size = Marshal.SizeOf(sei);
            sei.Verb = "openas";
            sei.File = file;
            sei.Show = SW_NORMAL;
            if (!ShellExecuteEx(ref sei));
                //throw new System.ComponentModel.Win32Exception();
        }
    }
}
