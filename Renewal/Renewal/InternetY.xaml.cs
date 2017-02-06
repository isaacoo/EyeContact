﻿using mshtml;
using SHDocVw;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Renewal
{
    /// <summary>
    /// Internet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InternetY : Window
    {
        #region variable
        private mshtml.HTMLDocument doc;

        private bool playOn = false;


        private string naver = "www.naver.com";
        private string google = ".google.co.kr";
        private string daum = ".daum.net";
        private string facebook = "www.facebook.com";

        private Uri currentUri;
        private string currentHost;

        DispatcherTimer timer = new DispatcherTimer();

        private Keyboard dlg;
        public static bool isLogin = false;
        public static string login_ID;
        public static string login_PW;
        #endregion

        #region main
        public InternetY()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(changeWindow);
            timer.Start();

            Width = System.Windows.Application.Current.MainWindow.Width;

            Back.Width = Width * 0.95;
            Back.Height = Height / 6 * 0.95;

            Search.Width = Width * 0.95;
            Search.Height = Height / 6 * 0.95;

            Stop_Play.Width = Width * 0.95;
            Stop_Play.Height = Height / 6 * 0.95;

            Fullscreen.Width = Width * 0.95;
            Fullscreen.Height = Height / 6 * 0.95;

            Exit.Width = Width * 0.95;
            Exit.Height = Height / 6 * 0.95;

          //  Login.Width = Width * 0.95;
          //  Login.Height = Height / 6 * 0.95;

        }
        #endregion

        #region focus
        // 창에 focus 가지 않도록 no activate
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        #region initialized
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr ip = SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }
        #endregion

        #region backButton
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private void Back_Click(object sender, RoutedEventArgs e) // 뒤로
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();
            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    if (!IE.Busy)
                    {
                        try
                        {
                            IE.GoBack();
                        }
                        catch
                        {
                            Console.WriteLine("err");
                        }
                    }
                }
            }
        }
        #endregion

        


        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        #region search
        //button click
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            dlg = new Keyboard();
            dlg.Closed += new EventHandler(Keyboard_Closed);
            dlg.Show(); // 키보드 열기
        }
        // search input complete
        void Keyboard_Closed(object sender, EventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                // Document 속성 읽기
                Uri uri = new Uri(doc.url);
                String host = uri.Host;

                if (host.Contains("youtube.com"))
                {
                    //검색어 셋팅
                    IHTMLElement q = doc.getElementsByName("search_query").item("search_query", 0);
                    q.setAttribute("value", System.Windows.Clipboard.GetText());

                    doc.getElementById("search-btn").click();
                }
                else
                {
                    System.Windows.MessageBox.Show("이곳은 유튜브가 아닙니다.");
                }
            }
        }
        #endregion


        #region stop/play
        private void Stop_Play_Click(object sender, RoutedEventArgs e)
        {
            playOn = !playOn;
            if(playOn)
                Stop_Play.Content = FindResource("Play");
            else
                Stop_Play.Content = FindResource("Stop");

            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                mshtml.IHTMLElementCollection elemColl = null;
                elemColl = doc.getElementsByTagName("button") as mshtml.IHTMLElementCollection;

                foreach (mshtml.IHTMLElement elem in elemColl)
                {
                    if (elem.getAttribute("class") != null)
                    {
                        if (elem.className == "ytp-play-button ytp-button")
                        {
                            elem.click();
                            break;
                        }
                    }
                }
            }
        }
                          
        #endregion



        #region FullScreen
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                mshtml.IHTMLElementCollection elemColl = null;
                elemColl = doc.getElementsByTagName("button") as mshtml.IHTMLElementCollection;

                foreach (mshtml.IHTMLElement elem in elemColl)
                {
                    if (elem.getAttribute("class") != null)
                    {
                        if (elem.className == "ytp-fullscreen-button ytp-button")
                        {
                            
                            elem.click();
                            Console.WriteLine("yy");
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        /*
        #region login
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            isLogin = true;
            dlg = new Keyboard();
            dlg.Closed += new EventHandler(Login_Closed);
            dlg.Show(); // 키보드 열기            
        }
        void Login_Closed(object sender, EventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                // Document 속성 읽기
                Uri uri = new Uri(doc.url);
                String host = uri.Host;

                if (host.Contains(naver) || host.Contains(nid_naver))
                {
                    mshtml.IHTMLElementCollection elemColl = null;
                    elemColl = doc.getElementsByTagName("input") as mshtml.IHTMLElementCollection;

                    foreach (mshtml.IHTMLElement elem in elemColl)
                    {
                        if (elem.getAttribute("id") != null)
                        {
                            if (elem.id == "id")
                            {
                                elem.setAttribute("value", login_ID);
                            }
                            else if (elem.id == "pw")
                            {
                                elem.setAttribute("value", login_PW);
                            }
                        }
                        else if (elem.getAttribute("title") != null)
                        {
                            if (elem.title == "로그인")
                                elem.click();
                        }
                    }
                }/*
                else if (host.Contains(daum) || host.Contains(google))
                {
                    IHTMLElement q = doc.getElementsByName("q").item("q", 0);
                    q.setAttribute("value", Clipboard.GetText());

                    IHTMLFormElement form_google = doc.forms.item(Type.Missing, 0);
                    form_google.submit();
                }///*
                else
                {
                    System.Windows.MessageBox.Show("naver google daum 쓰세요");
                }
                isLogin = false;
            }
        }

        #endregion
*/

        #region exit click
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();
            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    IE.Quit();
                    MainWindow.internetCount--;
                }
            }
            if (MainWindow.internetCount <= 0)
            {
                this.Close();
                timer.Stop();
                MainWindow.isInternet = false;
            }
        }
        #endregion

        #region area set
        // 윈도우 로드, 클로즈 시 Work area 변경
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                // Document 속성 읽기
                currentUri = new Uri(doc.url);
                currentHost = currentUri.Host;
            }
            AppBarFunctions.SetAppBar(this, ABEdge.Left);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
        }
        #endregion


        #region changeWindow
        private void changeWindow(object sender, EventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            if (doc != null)
            {
                // Document 속성 읽기
                Uri uri = new Uri(doc.url);
                String host = uri.Host;

                if (host != currentHost)
                {
                    currentHost = host;
                    if (host.Contains(naver))
                    {
                        Internet dlg = new Renewal.Internet();
                        dlg.Show();
                        timer.Stop();
                        this.Close();
                    }
                    else if (host.Contains(facebook))
                    {

                    }
                }
            }
        }
        #endregion

    }

}

