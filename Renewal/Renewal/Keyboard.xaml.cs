﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
// stt
using System.IO;
using System.Net;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using NAudio.Wave;
// json parsing
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Renewal
{
    public partial class Keyboard : Window
    {
        #region variable
        // 버튼 크기 결정
        double ButtonWidth = SystemParameters.PrimaryScreenWidth / 10;
        double ButtonHeight = SystemParameters.PrimaryScreenHeight / 6; // 버튼 높이는 해상도 너비의 1/6

       // 키보드 이벤트 API
       [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // Virtual-Key Code
        public const int KEYEVENTF_KEYDOWN = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const int DOT = 0xBE; // '.' flag

        // for문으로 qwerty 순서로 버튼을 생성하기 위한 문자별 array
        static string korean = "ㅂㅈㄷㄱㅅㅛㅕㅑㅐㅔㅁㄴㅇㄹㅎㅗㅓㅏㅣㅋㅌㅊㅍㅠㅜㅡ";
        static string ssang = "ㅃㅉㄸㄲㅆ";
        static string qwerty = "QWERTYUIOPASDFGHJKLZXCVBNM";
        static string small = "qwertyuiopasdfghjklzxcvbnm";
        static string special = "!@#$%^&*()~-=+[]<>?/:;'\"\\|,.";

        private bool isShifted = false;

        // voice click
        private string path = @"C:\Audio\";
        private string rawFile = @"raw.wav";
        private string wavFile = @"audio.wav";
        private string flacFile = @"audio.flac";
        private string flac_path = @"C:\Audio\audio.flac";
        private string speech_output = "";
        private bool isStart = true;

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        // login
        private bool isPW = false;

        #endregion

        #region make-keyboard
        public Keyboard()
        {
            InitializeComponent();

            // 버튼 위에 띄어지는 조그만 박스 초기화, 크기 설정
            label.Text = "";
            label.Width = ButtonWidth;
            label.Visibility = Visibility.Hidden;

            if (!Internet.isLogin)
                textBox.Text = "";
            else
                textBox.Text = "ID 를 입력하세요";
            
            
            // Panel 사이즈, 위치 조정
            topPanel.Height = ButtonHeight;
            topPanel.Width = ButtonWidth * 10;
            leftPanel.Width = ButtonWidth;
            leftPanel.Height = ButtonHeight * 2;
            leftPanel.Margin = new Thickness(0, ButtonHeight, 0, 0);
            rightPanel.Width = ButtonWidth * 2;
            rightPanel.Height = ButtonHeight * 2;
            rightPanel.Margin = new Thickness(ButtonWidth * 8, ButtonHeight, 0, 0);
            koreanPanel.Height /= 2;
            koreanPanel.Width = ButtonWidth * 10;
            koreanPanel.Margin = new Thickness(0, ButtonHeight * 3, 0, 0);
            ssangPanel.Height /= 6;
            ssangPanel.Width = ButtonWidth * 5;
            ssangPanel.Margin = new Thickness(0, ButtonHeight * 3, 0, 0);
            englishPanel.Width = ButtonWidth * 10;
            englishPanel.Height /= 2;
            englishPanel.Margin = new Thickness(0, ButtonHeight * 3, 0, 0);
            smallEnglishPanel.Width = ButtonWidth * 10;
            smallEnglishPanel.Height /= 2;
            smallEnglishPanel.Margin = new Thickness(0, ButtonHeight * 3, 0, 0);
            specialPanel.Height /= 2;
            specialPanel.Width = ButtonWidth * 10;
            specialPanel.Margin = new Thickness(0, ButtonHeight * 3, 0, 0);

            // TextBox 사이즈, 위치 조정
            textBox.Width = 0;
            textBox.Height = 0;
            //textBox.Margin = new Thickness(ButtonWidth, ButtonHeight, 0, 0);


            // fakeLabel 사이즈, 위치 조정
            fakeLabel.Width = ButtonWidth * 7;
            fakeLabel.Height = ButtonHeight * 2;
            fakeLabel.Margin = new Thickness(ButtonWidth, ButtonHeight, 0, 0);




            // 숫자 버튼 생성(Top Pannel)
            for (var i = 1; i <= 9; i++)
            {
                topPanel.Children.Add(new Button { Content = i.ToString(), Tag = "Digit", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });
            }
            topPanel.Children.Add(new Button { Content = "0", Tag = "Digit", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });

            // 좌측 시스템 버튼 위치 지정(Left Panel)
            Speech.Width = ButtonWidth;
            Speech.Height = ButtonHeight;
            Shift.Width = ButtonWidth;
            Shift.Height = ButtonHeight;


            // 우측 시스템 버튼 생성(Right Panel)
            BkSpace.Width = ButtonWidth;
            BkSpace.Height = ButtonHeight;
            SpecialButton.Width = ButtonWidth;
            SpecialButton.Height = ButtonHeight;
            Enter.Width = ButtonWidth;
            Enter.Height = ButtonHeight;
            OK.Width = ButtonWidth;
            OK.Height = ButtonHeight;


            // 영어 버튼 생성(English Pannel)
            for (int i = 0; i < qwerty.Length; i++)
            {
                englishPanel.Children.Add(new Button { Content = qwerty[i], Tag = "Alpha", Width = ButtonWidth, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
                if (qwerty[i] == 'L')
                    englishPanel.Children.Add(new Button { Content = "Korean", Tag = "System", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });
                if (qwerty[i] == 'V')
                    englishPanel.Children.Add(new Button { Content = ' ', Tag = "System", Width = ButtonWidth * 2, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
            }
            englishPanel.Children.Add(new Button { Content = '.', Tag = "Special", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });

            // 영어 소문자 버튼 생성(Small English Pannel)
            for (int i = 0; i < small.Length; i++)
            {
                smallEnglishPanel.Children.Add(new Button { Content = small[i], Tag = "Small", Width = ButtonWidth, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
                if (small[i] == 'l')
                    smallEnglishPanel.Children.Add(new Button { Content = "Korean", Tag = "System", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });
                if (small[i] == 'v')
                    smallEnglishPanel.Children.Add(new Button { Content = ' ', Tag = "System", Width = ButtonWidth * 2, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
            }
            smallEnglishPanel.Children.Add(new Button { Content = '.', Tag = "Special", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });


            // 한글 버튼 생성(Korean Pannel)
            for (int i = 0; i < korean.Length; i++)
            {
                koreanPanel.Children.Add(new Button { Content = korean[i], Tag = "Korean", Width = ButtonWidth, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
                if (korean[i] == 'ㅣ')
                    koreanPanel.Children.Add(new Button { Content = "English", Tag = "System", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });
                if (korean[i] == 'ㅍ')
                    koreanPanel.Children.Add(new Button { Content = ' ', Tag = "System", Width = ButtonWidth * 2, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
            }
            koreanPanel.Children.Add(new Button { Content = '.', Tag = "Special", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });

            // 한글 버튼 생성(ssang Pannel)
            for (int i = 0; i < ssang.Length; i++)
            {
                ssangPanel.Children.Add(new Button { Content = ssang[i], Tag = "Ssang", Width = ButtonWidth, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
            }


            // 특수문자 버튼 생성(Speical Pannel)
            for (int i = 0; i < special.Length; i++)
            {
                specialPanel.Children.Add(new Button { Content = special[i], Tag = "Special", Width = ButtonWidth, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
                if (special[i] == '"')
                    specialPanel.Children.Add(new Button { Content = ' ', Tag = "Special", Width = ButtonWidth * 2, Height = ButtonHeight, Focusable = false, Background = System.Windows.Media.Brushes.White });
            }
            specialPanel.Children.Add(new Button { Content = '.', Tag = "Special", Width = ButtonWidth, Height = ButtonHeight, Focusable = false });



            // 한영 전환 버튼
            changeButton.Width = ButtonWidth;
            changeButton.Height = ButtonHeight;
            changeButton.Margin = new Thickness(ButtonWidth * 9, ButtonHeight * 4, 0, 0);

            // 스페이스 버튼
            Space.Width = ButtonWidth * 2;
            Space.Height = ButtonHeight;
            Space.Margin = new Thickness(ButtonWidth * 4, ButtonHeight * 5, 0, 0);

            // 각 버튼과 Button Click method 연결
            foreach (Button button in topPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in leftPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in rightPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in koreanPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in ssangPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in englishPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in smallEnglishPanel.Children)
                button.Click += Button_Click;
            foreach (Button button in specialPanel.Children)
                button.Click += Button_Click;

            textBox.Focus(); // TextBox에 포커스 맞춤
            ShowKoreanPanel(); // 한글 패널만 보이기 (초기값: 한글)
        }
        #endregion

        #region button-click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // id, pw 적기전에 비우기
            if(Internet.isLogin && (textBox.Text == "ID 를 입력하세요" || textBox.Text == "PW 를 입력하세요"))
                textBox.Text = "";
            
            var button = sender as Button; // 각 버튼의 데이터를 button 변수로 가져옴
            string content = button.Content.ToString();

            // 버튼 위에 띄어지는 조그만 텍스트 박스 위치 설정 = 버튼 위치로 이동
            Point point = button.TransformToAncestor(this).Transform(new Point(0, 0));
            label.Margin = new Thickness(point.X, point.Y, 0, 0);

            // OK 버튼 클릭시 textBox의 내용을 Clipboard에 복사하고 키보드 종료
            if (button.Name == "OK")
            {
                if (!Internet.isLogin)
                {
                    Clipboard.SetText(textBox.Text);
                    this.Close();
                }
                else
                {
                    if(!isPW)
                    {
                        Internet.login_ID = textBox.Text;
                        isPW = true;
                        textBox.Text = "PW 를 입력하세요";
                    }
                    else
                    {
                        Internet.login_PW = textBox.Text;
                        isPW = false;
                        this.Close();
                    }

                }
                
            }
            else if (button.Name == "Speech")
            {
                if (isStart)
                {
                    Speech.Content = FindResource("Play");
                }
                else
                {
                    Speech.Content = FindResource("Stop");
                }
                Stt();
            }
            else if (button.Name == "Shift")
            {
                //특수문자 창이 활성화 되지 않았을 경우(한글 또는 영어일 경우)에만 활성화
                if (specialPanel.Visibility == Visibility.Hidden) 
                {
                    // Shift 상태 변경
                    isShifted = !isShifted;

                    if (isShifted == true)
                    {
                        Shift.Content = FindResource("Shifted");
                        if (changeButton.Content.ToString() == "한")
                            ShowEnglishPanel();
                        else
                            ssangPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Shift.Content = FindResource("Shift");
                        if (changeButton.Content.ToString() == "한")
                            ShowSmallEnglishPanel();
                        else
                            ShowKoreanPanel();
                    }
                }
            }
            else if (button.Name == "SpecialButton")
            {
                ShowSpecialPanel();
                if (changeButton.Content.ToString() == "한")
                    changeButton.Content = "영";
                else
                    changeButton.Content = "한";
                changeButton.Margin = new Thickness(ButtonWidth * 9, ButtonHeight, 0, 0);
            }
            else if (button.Name == "Space")
            {
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Space), 0, KEYEVENTF_KEYDOWN, 0);
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Space), 0, KEYEVENTF_KEYUP, 0);
            }
            else if(button.Name == "BkSpace")
            {
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Back), 0, KEYEVENTF_KEYDOWN, 0);
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Back), 0, KEYEVENTF_KEYUP, 0);
            }
            else if (button.Name == "Enter")
            {
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Enter), 0, KEYEVENTF_KEYDOWN, 0);
                keybd_event(Convert.ToByte(System.Windows.Forms.Keys.Enter), 0, KEYEVENTF_KEYUP, 0);
            }
            else if (button.Tag.ToString() == "Ssang")
            {
                InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Native;
                keybd_event(0x10, 0, 0, 0);
                keybd_event((byte)SsangToAlpha(content[0]), 0, KEYEVENTF_KEYDOWN, 0);
                keybd_event((byte)SsangToAlpha(content[0]), 0, KEYEVENTF_KEYUP, 0);
                keybd_event(0x10, 0, KEYEVENTF_KEYUP, 0);
            }
            // 클릭한 버튼이 한글일 경우
            else if (button.Tag.ToString() == "Korean")
            {
                InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Native;
                keybd_event((byte)KoreanToAlpha(content[0]), 0, KEYEVENTF_KEYDOWN, 0);
                keybd_event((byte)KoreanToAlpha(content[0]), 0, KEYEVENTF_KEYUP, 0);
            }
            // 영어, 숫자, 특수문자인 경우
            else
            {
                InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
                textBox.Text += button.Content.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
        #endregion

        #region korean to english
        // 한글 위치에 해당하는 한글로 변환 ex. 'ㅁ' -> 'A'
        public static char KoreanToAlpha(char ch)
        {
            int index = korean.IndexOf(ch);

            return qwerty[index];
        }

        // 한글 위치에 해당하는 한글로 변환 ex. 'ㅁ' -> 'A'
        public static char SsangToAlpha(char ch)
        {
            int index = ssang.IndexOf(ch);

            return qwerty[index];
        }

        // 한영 전환 버튼 클릭시
        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            string content = changeButton.Content.ToString();

            if (content == "영")
            {
                ShowSmallEnglishPanel();
                changeButton.Content = "한";
                changeButton.Margin = new Thickness(ButtonWidth * 9, ButtonHeight * 4, 0, 0);
            }
            else
            {
                ShowKoreanPanel();
                changeButton.Content = "영";
                changeButton.Margin = new Thickness(ButtonWidth * 9, ButtonHeight * 4, 0, 0);
            }
        }
        #endregion

        #region Speech to text
        private void Stt()
        {
            System.IO.Directory.CreateDirectory(path);

            if(isStart)
            {
                isStart = false;
                start_record();
            }
            else
            {
                isStart = true;
                end_record();
                convert();
                send();
            }
        }

        private void start_record()
        {
            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            mciSendString("record recsound", "", 0, 0);
        }

        private void end_record()
        {
            mciSendString(@"save recsound " + path + rawFile, "", 0, 0);
            mciSendString("close recsound ", "", 0, 0);
        }

        private void convert()
        {
            using (var reader = new WaveFileReader(path + rawFile))
            {
                var newFormat = new NAudio.Wave.WaveFormat(16000, 16, 1);
                using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                {
                    WaveFileWriter.CreateWaveFile(path + wavFile, conversionStream);
                }
            }

            if (!File.Exists(path + wavFile))
            {
                Console.WriteLine("wav file no!");
            }
            else
            {
                using (FileStream sourceStream = new FileStream(path + wavFile, FileMode.Open))
                {
                    WAVReader audioSource = new WAVReader(path + wavFile, sourceStream);

                    AudioBuffer buff = new AudioBuffer(audioSource, 0x10000);
                    FlakeWriter flakeWriter = new FlakeWriter(path + flacFile, audioSource.PCM);

                    flakeWriter.CompressionLevel = 8;
                    while (audioSource.Read(buff, -1) != 0)
                    {
                        flakeWriter.Write(buff);
                    }

                    flakeWriter.Close();
                    audioSource.Close();
                }
            }
        }

        private void send()
        {
            // request
            using (FileStream fileStream = File.OpenRead(flac_path))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.SetLength(fileStream.Length);
                    fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);
                    byte[] BA_AudioFile = memoryStream.GetBuffer();
                    HttpWebRequest _HWR_SpeechToText = null;
                    _HWR_SpeechToText =
                                (HttpWebRequest)HttpWebRequest.Create(
                                    "https://www.google.com/speech-api/v2/recognize?output=json&lang=ko-KR&key=AIzaSyCzsbEmnTv36-aWE5ThgGTnNXuJF-AeLcs");
                    _HWR_SpeechToText.Credentials = CredentialCache.DefaultCredentials;
                    _HWR_SpeechToText.Method = "POST";
                    _HWR_SpeechToText.ContentType = "audio/x-flac; rate=16000";
                    _HWR_SpeechToText.ContentLength = BA_AudioFile.Length;
                    using (Stream stream = _HWR_SpeechToText.GetRequestStream())
                    {
                        stream.Write(BA_AudioFile, 0, BA_AudioFile.Length);
                        stream.Close();

                        HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();

                        // response 
                        if (HWR_Response.StatusCode == HttpStatusCode.OK)
                        {
                            StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                            
                            var result = SR_Response.ReadToEnd();
                            var jsons = result.Split('\n');
                            Console.WriteLine(result);
                            json_parsing(jsons);
                        }
                    }
                }
            }
        }

        private void json_parsing(string[] jsons)
        {
            foreach (var root in jsons)
            {
                dynamic jsonObject = JsonConvert.DeserializeObject(root);
                if (jsonObject == null || jsonObject.result.Count <= 0)
                    continue;

                string json = jsonObject.result[0].alternative.ToString();
                var json_array = JArray.Parse(json);

                int i = 0;
                int max = 0;
                var max_confidence = 0;
                foreach (var a in json_array)
                {
                    if (i == 0)
                    {
                        max = i;
                        max_confidence = jsonObject.result[0].alternative[0].confidence;
                    }
                    else if (jsonObject.result[0].alternative[i].confidence >= max_confidence)
                    {
                        max = i;
                        max_confidence = jsonObject.result[0].alternative[i].confidence;
                    }
                    i++;
                }
                speech_output = jsonObject.result[0].alternative[max].transcript;
                textBox.Text += speech_output;
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
        #endregion

        #region Mini Textbox
        // 텍스트 박스의 값이 변경되면 버튼 위에 띄어지는 조그만 박스의 값도 업데이트
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // id, pw 적기전에 비우기
            if (Internet.isLogin && (textBox.Text == "ID 를 입력하세요" || textBox.Text == "PW 를 입력하세요"))
                label.Text = "";
            else
            {
                label.Visibility = Visibility.Visible;
                var sentence = textBox.Text.Split(' ');
                sentence = sentence[sentence.Length - 1].Split('\n');
                label.Text = sentence[sentence.Length - 1];
                if (label.Text == "") // 텍스트 박스의 값이 없으면 조그만 박스도 사라짐
                {
                    label.Visibility = Visibility.Hidden;
                }
            }
            fakeLabel.Content = textBox.Text;
        }
        #endregion

        private void ShowEnglishPanel()
        {
            specialPanel.Visibility = Visibility.Hidden;
            smallEnglishPanel.Visibility = Visibility.Hidden;
            koreanPanel.Visibility = Visibility.Hidden;
            ssangPanel.Visibility = Visibility.Hidden;
            englishPanel.Visibility = Visibility.Visible;      
        }

        private void ShowSmallEnglishPanel()
        {
            isShifted = false;
            specialPanel.Visibility = Visibility.Hidden;
            koreanPanel.Visibility = Visibility.Hidden;
            englishPanel.Visibility = Visibility.Hidden;
            ssangPanel.Visibility = Visibility.Hidden;
            smallEnglishPanel.Visibility = Visibility.Visible;
        }

        private void ShowKoreanPanel()
        {
            isShifted = false;
            specialPanel.Visibility = Visibility.Hidden;
            englishPanel.Visibility = Visibility.Hidden;
            smallEnglishPanel.Visibility = Visibility.Hidden;
            ssangPanel.Visibility = Visibility.Hidden;
            koreanPanel.Visibility = Visibility.Visible;
        }

        private void ShowSpecialPanel()
        {
            isShifted = false;
            englishPanel.Visibility = Visibility.Hidden;
            smallEnglishPanel.Visibility = Visibility.Hidden;
            koreanPanel.Visibility = Visibility.Hidden;
            ssangPanel.Visibility = Visibility.Hidden;
            specialPanel.Visibility = Visibility.Visible;
        }
    }
}
