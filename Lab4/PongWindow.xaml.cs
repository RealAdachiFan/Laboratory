using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls; 

namespace PongGame
{
    // Паттерн: Команда (с поправкой на nullable)
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => true;
        public event EventHandler? CanExecuteChanged;
        public void Execute(object? parameter) => _execute(parameter);
    }

    public partial class PongWindow : Window
    {
        private Game? _game;            
        private DispatcherTimer? _timer; 

        public ICommand StartCommand => new RelayCommand(StartGame);
        public ICommand ResetCommand => new RelayCommand(ResetGame);
        public ICommand ExitCommand => new RelayCommand(ExitGame);

        public PongWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitGame();
        }

        private void InitGame()
        {
            _game = new Game();

            _game.Score.OnScoreChanged += (player, ai) =>
            {
                ScoreText.Text = $"Игрок: {player} | Компьютер: {ai}";
            };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += GameLoop;
        }

        private void GameLoop(object? sender, EventArgs e) 
        {
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
                _game?.PlayerPaddle.MoveUp();
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
                _game?.PlayerPaddle.MoveDown(20);

            if (_game != null && _timer != null)
            {
                _game.Update();
                Render();
            }
        }

        private void StartGame(object? obj) 
        {
            _timer?.Start();
        }

        private void ResetGame(object? obj)
        {
            _timer?.Stop();
            _game?.Reset();
            Render();
        }

        private void ExitGame(object? obj) 
        {
            Application.Current.Shutdown();
        }

        private void Render()
        {
            if (_game == null) return;
            
            
            double scaleX = GameCanvas.Width / 80;
            double scaleY = GameCanvas.Height / 20;

            Canvas.SetLeft(Ball, _game.Ball.Position.X * scaleX);
            Canvas.SetTop(Ball, _game.Ball.Position.Y * scaleY);

            Canvas.SetLeft(PlayerPaddle, _game.PlayerPaddle.Position.X * scaleX);
            Canvas.SetTop(PlayerPaddle, _game.PlayerPaddle.Position.Y * scaleY);

            Canvas.SetLeft(AiPaddle, _game.AiPaddle.Position.X * scaleX);
            Canvas.SetTop(AiPaddle, _game.AiPaddle.Position.Y * scaleY);
        }
    }
}