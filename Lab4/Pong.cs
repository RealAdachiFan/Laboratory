using System;

namespace PongGame
{
    // ==========================================
    // ПАТТЕРН: СТРАТЕГИЯ (Strategy)
    // ==========================================
    // Интерфейс для поведения ИИ
    public interface IAiStrategy
    {
        void UpdatePosition(AiPaddle paddle, PlayerBall ball, int fieldHeight);
    }

    // Конкретная стратегия: ИИ просто следует за мячом
    public class FollowBallStrategy : IAiStrategy
    {
        public void UpdatePosition(AiPaddle paddle, PlayerBall ball, int fieldHeight)
        {
            // Центр ракетки
            float centerPaddle = paddle.Position.Y + (paddle.Height / 2.0f);
            int ballY = ball.Position.Y;

            // Двигаемся, если центр ракетки не совпадает с мячом
            if (centerPaddle < ballY)
                paddle.MoveDown(fieldHeight);
            else if (centerPaddle > ballY)
                paddle.MoveUp();
        }
    }

    // ==========================================
    // БАЗОВЫЕ КЛАССЫ (Ваш код с минимальными правками)
    // ==========================================
    public class GamePoint { public int X { get; set; } public int Y { get; set; } public GamePoint(int x, int y) { X = x; Y = y; } }
    public class GameVector { public int X { get; set; } public int Y { get; set; } public GameVector(int x, int y) { X = x; Y = y; } }

    public class PlayerBall
    {
        public GamePoint Position { get; set; }
        public GameVector Velocity { get; set; }

        public PlayerBall(GamePoint position, GameVector velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public void Move()
        {
            Position.X += Velocity.X;
            Position.Y += Velocity.Y;
        }

        public void BounceHorizontal() => Velocity.X *= -1;
        public void BounceVertical() => Velocity.Y *= -1;

        // Метод для сброса мяча
        public void Reset(int width, int height)
        {
            Position.X = width / 2;
            Position.Y = height / 2;
            // Случайное направление старта
            Velocity.X = (new Random().Next(2) == 0 ? 2 : -2); 
            Velocity.Y = (new Random().Next(2) == 0 ? 2 : -2);
        }
    }

    public class PlayerPaddle
    {
        public GamePoint Position { get; set; }
        public int Height { get; } = 10;
        public int Width { get; } = 3;
        public int Speed { get; } = 5;

        public PlayerPaddle(GamePoint position) { Position = position; }

        public void MoveUp() { Position.Y = Math.Max(0, Position.Y - Speed); }
        public void MoveDown(int fieldHeight) { Position.Y = Math.Min(fieldHeight - Height, Position.Y + Speed); }
    }

    // Ракетка ИИ теперь использует Стратегию
    public class AiPaddle : PlayerPaddle
    {
        public IAiStrategy Strategy { get; set; }

        public AiPaddle(GamePoint position, IAiStrategy strategy) : base(position)
        {
            Strategy = strategy;
        }

        public void Update(PlayerBall ball, int fieldHeight)
        {
            // Делегируем решение стратегии
            Strategy.UpdatePosition(this, ball, fieldHeight);
        }
    }

    // ==========================================
    // ПАТТЕРН: НАБЛЮДАТЕЛЬ (Observer) - Субъект
    // ==========================================
public class Score
{
    // Исправлено: добавлен знак вопроса '?'
    public event Action<int, int>? OnScoreChanged; 

    public int PlayerScore { get; private set; }
    public int AiScore { get; private set; }

    public void AddPlayerScore()
    {
        PlayerScore++;
        NotifyObservers();
    }

    public void AddAiScore()
    {
        AiScore++;
        NotifyObservers();
    }
    
    // (Добавьте методы Reset и NotifyObservers, если их еще нет)
    public void Reset() { PlayerScore = 0; AiScore = 0; NotifyObservers(); }

    private void NotifyObservers()
    {
        OnScoreChanged?.Invoke(PlayerScore, AiScore);
    }
}

    // ==========================================
    // ГЛАВНЫЙ КЛАСС ИГРЫ
    // ==========================================
    public class Game
    {
        private PlayerBall _ball;
        public PlayerBall Ball => _ball;
        public PlayerPaddle PlayerPaddle { get; }
        public AiPaddle AiPaddle { get; }
        public Score Score { get; } = new Score();

        private readonly int _fieldWidth = 80;
        private readonly int _fieldHeight = 20;

        public Game()
        {
            // Инициализация с передачей стратегии
            var aiStrategy = new FollowBallStrategy();

            _ball = new PlayerBall(new GamePoint(_fieldWidth / 2, _fieldHeight / 2), new GameVector(2, 2));
            PlayerPaddle = new PlayerPaddle(new GamePoint(2, _fieldHeight / 2 - 5));
            AiPaddle = new AiPaddle(new GamePoint(_fieldWidth - 5, _fieldHeight / 2 - 5), aiStrategy);
        }

        public void Update()
        {
            Ball.Move();
            
            // Используем стратегию для движения ИИ
            AiPaddle.Update(Ball, _fieldHeight);
            
            CheckCollisions();
        }

        public void Reset()
        {
            _ball.Reset(_fieldWidth, _fieldHeight);
            Score.Reset();
        }

        private void CheckCollisions()
        {
            // Отскок от стен
            if (Ball.Position.Y <= 0 || Ball.Position.Y >= _fieldHeight)
                Ball.BounceVertical();

            // Отскок от ракеток
            if (IsColliding(PlayerPaddle) || IsColliding(AiPaddle))
                Ball.BounceHorizontal();

            // Гол
            if (Ball.Position.X <= 0)
            {
                Score.AddAiScore();
                Ball.Reset(_fieldWidth, _fieldHeight);
            }
            else if (Ball.Position.X >= _fieldWidth)
            {
                Score.AddPlayerScore();
                Ball.Reset(_fieldWidth, _fieldHeight);
            }
        }

        private bool IsColliding(PlayerPaddle paddle)
        {
            return Ball.Position.X >= paddle.Position.X && 
                   Ball.Position.X <= paddle.Position.X + paddle.Width &&
                   Ball.Position.Y >= paddle.Position.Y &&
                   Ball.Position.Y <= paddle.Position.Y + paddle.Height;
        }
    }
}