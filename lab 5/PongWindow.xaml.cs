using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace PongGame
{
    // ==========================================
    // Паттерн: Команда (Command)
    // ==========================================
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
        
        // Сервис для асинхронных операций (файлы, вычисления)
        private GameService _service = new GameService();

        // Команды для кнопок
        public ICommand StartCommand => new RelayCommand(async (obj) => await StartGameAsync());
        public ICommand ResetCommand => new RelayCommand(async (obj) => await ResetGameAsync());
        public ICommand ExitCommand => new RelayCommand(async (obj) => await ExitGameAsync());

        public PongWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitGame();
            
            // Загружаем рекорд при старте
            LoadHighScoreToUI();
        }

        private void InitGame()
        {
            _game = new Game();

            // Подписка на изменения счета (Наблюдатель)
            _game.Score.OnScoreChanged += UpdateScoreUi;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += GameLoop;
        }

        // Асинхронная загрузка рекорда
        private async void LoadHighScoreToUI()
        {
            try
            {
                int highScore = await _service.LoadHighScoreAsync();
                // Обновляем текст счета, добавляя рекорд
                ScoreText.Text = $"Игрок: 0 | Компьютер: 0 | Рекорд: {highScore}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки рекорда: {ex.Message}");
            }
        }

        // Метод обновления UI при изменении счета
        // Исправляет ошибку: "The name 'UpdateScoreUi' does not exist"
        private void UpdateScoreUi(int playerScore, int aiScore)
        {
            // Получаем текущий рекорд из текста (или просто 0 для простоты)
            // В более сложном приложении лучше хранить рекорд в переменной
            string currentText = ScoreText.Text;
            int bestScore = 0;
            if (currentText.Contains("Рекорд:"))
            {
                var parts = currentText.Split('|');
                var recordPart = parts.LastOrDefault()?.Split(':');
                if (recordPart != null && int.TryParse(recordPart[1], out var best))
                    bestScore = best;
            }

            if (playerScore > bestScore)
            {
                // Еслибили рекорд
                ScoreText.Text = $"Игрок: {playerScore} | Компьютер: {aiScore} | Рекорд: {playerScore}";
                // Можно сохранить новый рекорд здесь
                _ = _service.SaveHighScoreAsync(playerScore);
            }
            else
            {
                ScoreText.Text = $"Игрок: {playerScore} | Компьютер: {aiScore} | Рекорд: {bestScore}";
            }
        }

        // ==========================================
        // Асинхронные методы (Commands)
        // ==========================================

        private async Task StartGameAsync()
        {
            if (_game == null || _timer == null) return;

            // Пример асинхронной работы: "ИИ думает перед стартом"
            // Это не блокирует интерфейс
            int target = await _service.CalculateOptimalDefenseAsync(_game.Ball, 20);
            
            _timer.Start();
        }

        private async Task ResetGameAsync()
        {
            _timer?.Stop();
            _game?.Reset();
            Render(); // Перерисовать поле после сброса
        }

        private async Task ExitGameAsync()
        {
            // Сохраняем рекорд перед выходом
            await _service.SaveHighScoreAsync(_game?.Score.PlayerScore ?? 0);
            Application.Current.Shutdown();
        }

        // Игровой цикл
        private void GameLoop(object? sender, EventArgs e)
        {
            if (_game == null) return;

            // Управление игроком
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
                _game.PlayerPaddle.MoveUp();
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
                _game.PlayerPaddle.MoveDown(20); // 20 - высота поля

            // Логика игры
            _game.Update();

            // Отрисовка
            Render();
        }

        // Метод отрисовки (перевод координат игры в координаты экрана)
        // Исправляет ошибку: "The name 'Render' does not exist"
        private void Render()
        {
            if (_game == null) return;

            double scaleX = GameCanvas.Width / 80;  // 80 - логическая ширина поля
            double scaleY = GameCanvas.Height / 20; // 20 - логическая высота поля

            // Мяч
            Canvas.SetLeft(Ball, _game.Ball.Position.X * scaleX);
            Canvas.SetTop(Ball, _game.Ball.Position.Y * scaleY);

            // Ракетка Игрока
            Canvas.SetLeft(PlayerPaddle, _game.PlayerPaddle.Position.X * scaleX);
            Canvas.SetTop(PlayerPaddle, _game.PlayerPaddle.Position.Y * scaleY);

            // Ракетка ИИ
            Canvas.SetLeft(AiPaddle, _game.AiPaddle.Position.X * scaleX);
            Canvas.SetTop(AiPaddle, _game.AiPaddle.Position.Y * scaleY);
        }
    }
}