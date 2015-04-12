using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Clipboard = System.Windows.Forms.Clipboard;

namespace ChatWheel
{
    public partial class ChatOverlay : Window
    {
        private readonly Settings _settings;

        private readonly string[] buttonColors =
        {
            "#39A7F2",
            "#648E06",
            "#FE8B1E",
            "#C4771B",
            "#EA5600",
            "#6736C6",
            "#3E5066",
            "#39A7F2",
            "#648E06",
            "#FE8B1E"
        };

        private readonly DispatcherTimer hoverTimer = new DispatcherTimer();
        private Point _chatWheelCenterLocation;
        private PiePiece _lastChosenPie;
        private Point _previousValidPos;

        public ChatOverlay(Settings settings)
        {
            _settings = settings;
            InitializeComponent();

            var screen = Screen.PrimaryScreen.WorkingArea;
            Left = screen.Width/2 - Width/2;
            Top = screen.Height/2 - Height/2;

            Canvas.SetLeft(elpsChatWheel, Width/2 - elpsChatWheel.Width/2);
            Canvas.SetTop(elpsChatWheel, Height/2 - elpsChatWheel.Height/2);
            _chatWheelCenterLocation = new Point(Canvas.GetLeft(elpsChatWheel) + elpsChatWheel.Width/2,
                Canvas.GetTop(elpsChatWheel) + elpsChatWheel.Height/2);


            hoverTimer.Tick += hoverTimer_Tick;
            hoverTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            Visibility = Visibility.Hidden;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            User32.SetWindowExTransparent(new WindowInteropHelper(this).Handle);
        }

        /// <summary>
        ///     Handles keypresses, determines what element is being highlighted
        ///     and send the selected element to the game window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            if (!Keyboard.GetKeyStates((Key) _settings.HotKey).HasFlag(KeyStates.Down))
            {
                if (Visibility != Visibility.Hidden)
                {
                    //Selection has ended. Now acting
                    Visibility = Visibility.Hidden;
                    if (_lastChosenPie == null) return;
                    _lastChosenPie.ReactToMouseLeave();
                    SendKeys.SendWait("{ENTER}");
                    Clipboard.SetText(_lastChosenPie.FullText);
                    SendKeys.SendWait("^v");
                    SendKeys.SendWait("{ENTER}");
                }
                return;
            }
            if (Visibility == Visibility.Hidden)
            {
                Visibility = Visibility.Visible;
                User32.SetCursorPosition(Left + _chatWheelCenterLocation.X, Top + _chatWheelCenterLocation.Y);
            }

            //TODO: Clean up this area 
            var mousePos = User32.GetMousePosition();
            var mouseOffseted = mousePos;
            mouseOffseted.Offset(Left*-1, Top*-1);
            Console.WriteLine(mouseOffseted);
            if (_previousValidPos.X == 0)
                _previousValidPos = _chatWheelCenterLocation;

            //Recenter if too far away
            var distance = Utils.Distance2D(_chatWheelCenterLocation, mouseOffseted);
            if (distance > 90)
            {
                User32.SetCursorPosition((int) _previousValidPos.X, (int) _previousValidPos.Y);
            }
            else
            {
                _previousValidPos = mousePos;
            }

            //Ignore nodes if mouse is in the center of the circle
            if (distance < 30)
            {
                if (_lastChosenPie != null)
                    _lastChosenPie.ReactToMouseLeave();
                _lastChosenPie = null;
                return;
            }
            //Todo: optimize by generating a list of piepieces
            foreach (var obj in chatWheelCanvas.Children)
            {
                if ((obj.GetType() != typeof (PiePiece))) continue;
                var pie = obj as PiePiece;
                if (pie.IsAngleOnControl(
                    Utils.FindAngleBetweenPoints
                        (new Point(_chatWheelCenterLocation.X, 0),
                            _chatWheelCenterLocation, mouseOffseted)))
                {
                    pie.ReactToMouseEnter();
                    _lastChosenPie = pie;
                }
                else
                {
                    pie.ReactToMouseLeave();
                }
            }
        }

        /// <summary>
        ///     Recreates and redraws all pie pieces
        ///     This could be greatly optimized.
        /// </summary>
        public void UpdateChatWheel()
        {
            hoverTimer.Stop();
            chatWheelCanvas.Children.RemoveRange(1, chatWheelCanvas.Children.Count - 1);
            for (var i = 0; i < _settings.PhrasesAmount; i++)
            {
                var piece = new PiePiece();

                piece.FillColor =
                    new SolidColorBrush((Color) ColorConverter.ConvertFromString(buttonColors[i]));
                piece.CenterX = _chatWheelCenterLocation.X;
                piece.CenterY = _chatWheelCenterLocation.Y;
                piece.RotationAngle = 360.0/_settings.PhrasesAmount*i;
                piece.WedgeAngle = 360.0/_settings.PhrasesAmount - 2;
                piece.Radius = 110;
                piece.InnerRadius = 50;
                piece.PieceValue = i;
                piece.FullText = _settings.Phrases[i].FullPhrase;
                piece.ShortText = _settings.Phrases[i].ShortPhrase;
                chatWheelCanvas.Children.Add(piece);
            }
            hoverTimer.Start();
        }
    }
}