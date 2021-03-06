﻿

using System;
using System.Windows;
//move mouse
using System.Runtime.InteropServices;
//Tobii
using Tobii.EyeX.Framework;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Threading;
using Karna.Magnification;
using System.Diagnostics;

using System.Threading;
using SHDocVw;

using mshtml;


namespace Renewal
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region variable
        public static int internetCount = 0;
        public static bool isInternet = false;

        private mshtml.HTMLDocument doc;
        private string youtube = "www.youtube.com";
        private string facebook = "www.facebook.com";
        #endregion

        #region main
        public MainWindow()
        {
            InitializeComponent();

            // calculate screen and button size
            Width /= 8;

            double ButtonWidth = Width;
            double ButtonHeight = Height / 6;
            Mouse.Width = ButtonWidth;
            Mouse.Height = ButtonHeight;
            Keyboard.Width = ButtonWidth;
            Keyboard.Height = ButtonHeight;
            Internet.Width = ButtonWidth;
            Internet.Height = ButtonHeight; Internet.Width = ButtonWidth;
            Internet.Height = ButtonHeight;
            PgUp.Width = ButtonWidth;
            PgUp.Height = ButtonHeight;
            PgDn.Width = ButtonWidth;
            PgDn.Height = ButtonHeight;
            Setting.Width = ButtonWidth;
            Setting.Height = ButtonHeight;



            Mouse.Width = ButtonWidth * 0.95;
            Mouse.Height = ButtonHeight * 0.95;
            Keyboard.Width = ButtonWidth * 0.95;
            Keyboard.Height = ButtonHeight * 0.95;
            Internet.Width = ButtonWidth * 0.95;
            Internet.Height = ButtonHeight * 0.95;
            PgUp.Width = ButtonWidth * 0.95;
            PgUp.Height = ButtonHeight * 0.95;
            PgDn.Width = ButtonWidth * 0.95;
            PgDn.Height = ButtonHeight * 0.95;
            Setting.Width = ButtonWidth * 0.95;
            Setting.Height = ButtonHeight * 0.95;

            // 툴바 위치 설정
            Left = SystemParameters.WorkArea.Right - Width;
            Top = 0;

            Move_Mouse();

            //키보드 후킹 --> up key를 누르면 마우스 왼쪽 버튼 클릭이 작동
            SetHook();

          //  startMagnifier();
        }
        #endregion

        #region focus
        // 창에 focus 가지 않도록 no activate
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

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

        #region Tobii-mouse-move
        // move mouse
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        // last Point
        private static int lastX = 0;
        private static int lastY = 0;
         
        // err
        private static int errX = 50;
        private static int errY = 50;
         
        //user Gaze Point
        private static int userCoordinateX = 0;
        private static int userCoordinateY = 0;
         
        // sure SetCoordinate is opened.
        public static bool isCoordinate = false;

        // move_mouse event?
        private void Move_Mouse()
        {
            var lightlyFilteredGazeDataStream = ((App)System.Windows.Application.Current)._eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);

                lightlyFilteredGazeDataStream.Next += (s, e) => move_mouse(e.X, e.Y);
           
                
        }

        private void move_mouse(double x, double y)
        {
            if (magnifierWindowLens != null)
            {
                x = x / mgnLens.Magnification + mgnLens.getSourceRectLeft();
                y = y / mgnLens.Magnification + mgnLens.getSourceRectTop();
            }

                if (lastX == 0 && lastY == 0)
            {
                lastX = (int)x;
                lastY = (int)y;
            }

            if (x <= lastX - errX || x >= lastX + errX || y >= lastY + errY || y <= lastY - errY)
            {
                SetCursorPos((int)x + userCoordinateX, (int)y + userCoordinateY);
                lastX = (int)x;
                lastY = (int)y;
            }
            else
            {
                SetCursorPos(lastX + userCoordinateX, lastY + userCoordinateY);
            }
        }

        #endregion

        #region Keyboard Ctrl + v event
        // 키보드 이벤트 API
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        //  키보드 종료 되면서 clipboard에 복사된 텍스트가 ctrl + v 됨
        void Keyboard_Closed(object sender, EventArgs e)
        {
            keybd_event((byte)0x11, 0, 0, 0);
            keybd_event((byte)'V', 0, 0, 0);
            keybd_event((byte)0x11, 0, 0x0002, 0);
            keybd_event((byte)'V', 0, 0x0002, 0);
        }
        #endregion
        
        #region About hooking - keyboard, mouse, cursor cordinate

        //**********************************************


        //키보드 후킹
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "MagShowSystemCursor")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagShowSystemCursor([MarshalAs(UnmanagedType.Bool)]bool fShowCursor);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc = hookProc;

        private static IntPtr hhook = IntPtr.Zero;
        // 전역후킹 변수들

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int LEFTDOWN = 0x0002;
        public const int LEFTUP = 0x0004;
        public const int RIGHTDOWN = 0x0008;
        public const int RIGHTUP = 0x0010;
      //  public const int MOUSEWHEEL = 0x0800;
        //마우스 후킹 변수들 

        public static int mouseEvent_var = (int)mouseEvent.LCLICKED;
        public static int mouseEvent_var_forMagnification;
        public static bool flag_mag_start = false;
      
        public enum mouseEvent
        {
            LCLICKED = 0,
            RCLICKED = 1,
            DOUBLECLICKED = 2,
            DRAGCLICKED = 3,
            RELEASED = 4
        }
        // 마우스 이벤트 변수들

        public void SetHook() // 후킹을 시작
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0); // _porc 함수로 넘어감 = hookPorc로 넘어감
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }

       
        public static Stopwatch SW = new Stopwatch();
        public static magnifier magnifierWindowLens = null;
        public static magnifier magnifierWindowDocked = null;
        public static Magnifier mgnLens = null;
        public static Magnifier mgnDocked = null;
        public static int time = 0 ;


        private static void Timer_Tick()
        {
            flag_mag_start = true;
            if (mouseEvent_var == (int)mouseEvent.RCLICKED)
                mouse_event(RIGHTUP, 0, 0, 0, 0);
            else
                mouse_event(LEFTUP, 0, 0, 0, 0);

            startMagnifier();

        }

        private static void startMagnifier()
        {
                if (magnifierWindowLens==null)
                {
                    magnifierWindowLens = new magnifier(true);
                    magnifierWindowLens.Show();
                    mgnLens = new Karna.Magnification.Magnifier(magnifierWindowLens, true);
                    mgnLens.Magnification = (float)(Properties.Settings.Default.magnificationRate + 2.0) / 2;
                }
               /* else if(magnifierWindowDocked==null)
                {
                    //System.Windows.MessageBox.Show("This is Docked");
                    magnifierWindowDocked = new magnifier(Properties.Settings.Default.isLens);
                    magnifierWindowDocked.Show();
                    mgnDocked = new Magnifier(magnifierWindowDocked, Properties.Settings.Default.isLens);
                    mgnDocked.Magnification = (float)(Properties.Settings.Default.magnificationRate + 2.0) / 2;
                }
                */
        }

        public static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            #region // keydown hooking
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN) 
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode.ToString() == "124") // 38: up key, 81: q key, 124: F13 key(shift+F1)
                                                // http://crynut84.tistory.com/34 참조
                {
                    #region /*stop watch for magnification*/ 
                    
                    if(SW.ElapsedMilliseconds > 200)
                    {
                        SW.Stop();
                        Timer_Tick();
                        SW.Reset();
                    }
                    if (!SW.IsRunning)
                    {
                        SW.Start();
                    }
                    #endregion

                    #region 
                    if (flag_mag_start)
                    {
                        return (IntPtr)1;
                    }
                    else
                    {
                        switch (mouseEvent_var)
                        {
                            case (int)mouseEvent.LCLICKED:
                                mouse_event(LEFTDOWN, 0, 0, 0, 0); // 마우스 왼쪽 클릭 
                                return (IntPtr)1; //return 1: vkCode(up 키) 메세지를 메세지 큐로 전달하지 않음 (=up 키가 작동되지 않음)
                                                  // return CallNextHookEx(hhook, code, (int)wParam, lParam); //: 해당 메세지를 큐로 전달함

                            case (int)mouseEvent.RCLICKED:
                                mouse_event(RIGHTDOWN, 0, 0, 0, 0); // 마우스 오른쪽 클릭
                                return (IntPtr)1;  // return CallNextHookEx(hhook, code, (int)wParam, lParam); //: 해당 메세지를 큐로 전달함


                            case (int)mouseEvent.DOUBLECLICKED:
                                mouse_event(LEFTDOWN, 0, 0, 0, 0); // 마우스 더블 클릭 
                                return (IntPtr)1;

                            case (int)mouseEvent.DRAGCLICKED:
                                mouse_event(LEFTDOWN, 0, 0, 0, 0); // 마우스 드래그 
                                return (IntPtr)1;

                            case (int)mouseEvent.RELEASED:
                                mouse_event(LEFTUP, 0, 0, 0, 0);
                                mouseEvent_var = (int)mouseEvent.LCLICKED;
                                return (IntPtr)1;
                        }
                    }
                    #endregion 

                }
                #region //when user coordinates gaze Point and mouse position. 
                if (isCoordinate)
                {
                    switch (vkCode)
                    {
                        //A key- 왼쪽으로 좌표점 이동시키기
                        case 65:
                            userCoordinateX -= 5;
                            break;
                        //D key - 오른쪽으로 좌표점 이동시키기 
                        case 68:
                            userCoordinateX += 5;
                            break;
                        //W key - 위로 이동시키기
                        case 87:
                            userCoordinateY -= 5;
                            break;
                        //S key - 아래로 이동시키기
                        case 83:
                            userCoordinateY += 5;
                            break;
                        default:
                            break;
                    }
                    return (IntPtr)1;
                }
                else
                    return CallNextHookEx(hhook, code, (int)wParam, lParam);
                #endregion
            }
            #endregion
            #region //keyup hooking
            else if (code >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode.ToString() == "124") // 38: up key, 81: q key, 124: F13 key(shift+F1)
                                                // http://crynut84.tistory.com/34 참조
                {
                    SW.Stop();
                    SW.Reset();

                    #region /*when user makes first click event after magnification is generated*/
                    if (flag_mag_start)
                    { 
                        flag_mag_start = false;
                        return (IntPtr)1;
                    }
                    #endregion
                    else
                    {
                        switch (mouseEvent_var)
                        {
                            case (int)mouseEvent.LCLICKED:
                                mouse_event(LEFTUP, 0, 0, 0, 0);
                                break; // return 1: vkCode(up 키) 메세지를 메세지 큐로 전달하지 않음 (=up 키가 작동되지 않음)
                                                  // return CallNextHookEx(hhook, code, (int)wParam, lParam); : 해당 메세지를 큐로 전달함
                            case (int)mouseEvent.RCLICKED:
                                mouse_event(RIGHTUP, 0, 0, 0, 0);
                                mouseEvent_var = (int)mouseEvent.LCLICKED;
                                break;
                            case (int)mouseEvent.DOUBLECLICKED:
                                mouse_event(LEFTUP, 0, 0, 0, 0);
                                mouse_event(LEFTDOWN, 0, 0, 0, 0);
                                mouse_event(LEFTUP, 0, 0, 0, 0);
                                mouseEvent_var = (int)mouseEvent.LCLICKED;
                                break;
                            case (int)mouseEvent.DRAGCLICKED:
                                mouseEvent_var = (int)mouseEvent.RELEASED;
                                break;
                        }
                        #region /*magnification closing*/
                        if (magnifierWindowLens != null)
                        {
                            magnifierWindowLens.Close();
                            magnifierWindowLens = null;
                            return (IntPtr)1;
                        }
                        else
                            return (IntPtr)1;
                    }
                }
                return CallNextHookEx(hhook, code, (int)wParam, lParam);
            }
            #endregion
            #endregion
            else
                return CallNextHookEx(hhook, code, (int)wParam, lParam);
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            UnHook();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            SetHook();
       
        }
        #endregion

        #region button click event
        // button click event 
        private void Mouse_Click(object sender, RoutedEventArgs e)
        {
            Mouse dlg = new Renewal.Mouse();
            dlg.Show();
        }
        private void Keyboard_Click(object sender, RoutedEventArgs e)
        {
            Keyboard dlg = new Renewal.Keyboard();
            dlg.Closed += new EventHandler(Keyboard_Closed);
            dlg.Show();
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Setting dlg = new Renewal.Setting();
            dlg.Show();
        }
        private void PgUp_Click(object sender, RoutedEventArgs e)
        {
             const int KEYUP = 0x0002;
             keybd_event(0x21, 0, 0,0);  // Page Up key 다운
             keybd_event(0x21, 0, KEYUP, 0);
        }
        private void PgDn_Click(object sender, RoutedEventArgs e)
        {
            const int KEYUP = 0x0002;
            keybd_event(0x22, 0, 0, 0);   // Page Up key 다운
            keybd_event(0x22, 0, KEYUP, 0);
        }
        //sure Internet toolbar is opened
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void Internet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InternetExplorer ie = new InternetExplorer();
                IWebBrowserApp webBrowser = ie;

                webBrowser.Visible = true;
                webBrowser.GoHome();
                IWebBrowserApp wb = (IWebBrowserApp)ie;

                wb.Visible = true;
                wb.GoHome();

                internetCount++;
            }
            catch
            {
                System.Windows.MessageBox.Show("internet connect");
            }

            //인터넷 최대화 단축키
            keybd_event(0x5B, 0, 0, 0); // window key
            keybd_event(0x26, 0, 0, 0); // arrow up key
            keybd_event(0x5B, 0, 0x0002, 0);
            keybd_event(0x26, 0, 0x0002, 0);
            
            if (isInternet == false )
            {
                SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
                IntPtr handle = GetForegroundWindow();
                foreach (SHDocVw.WebBrowser IE in shellWindows)
                {
                    if (IE.HWND.Equals(handle.ToInt32()))
                    {
                        while (IE.Busy == true || IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                            System.Threading.Thread.Sleep(100);
                        try
                        {
                            doc = IE.Document as mshtml.HTMLDocument;
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show("err");
                        }
                    }
                }            
                if (doc != null)
                {
                    Uri uri = new Uri(doc.url);
                    String host = uri.Host;

                    if (host.Contains(youtube))
                    {
                        InternetY dlg = new Renewal.InternetY();
                        dlg.Show();
                        //isInternetY = true;
                    }
                    else if (host.Contains(facebook))
                    {
                    }
                    else // naver, daum, google etc. (default)
                    {
                        Internet dlg = new Renewal.Internet();
                        dlg.Show();
                        isInternet = true;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Internet open error");
                }
            }
        }
        #endregion

        #region 바탕화면 크기 조정?
        // 윈도우 로드, 클로즈 시 Work area 변경
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.Right);
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
            System.Windows.Application.Current.Shutdown(); // 모든 자식과 함께 종료
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
            System.Windows.Application.Current.Shutdown(); // 모든 자식과 함께 종료
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
            System.Windows.Application.Current.Shutdown(); // 모든 자식과 함께 종료
        }
        #endregion
    }
}

