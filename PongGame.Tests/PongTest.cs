using Microsoft.VisualStudio.TestTools.UnitTesting;
using PongGame; 

namespace PongGame.Tests
{
    [TestClass]
    public class PongLogicTests
    {
        // Тест 1: Движение мяча
        [TestMethod]
        public void TestBallMove_UpdatesPositionCorrectly()
        {
            // Подготавливаем мяч
            var ball = new PlayerBall(new GamePoint(10, 10), new GameVector(2, 2));
            
            // Двигаем
            ball.Move();
            
            // Проверяем, что координаты изменились
            Assert.AreEqual(12, ball.Position.X);
            Assert.AreEqual(12, ball.Position.Y);
        }

        // Тест 2: Отскок от стены
        [TestMethod]
        public void TestBallBounceHorizontal_ReversesVelocity()
        {
            var ball = new PlayerBall(new GamePoint(0, 0), new GameVector(2, 2));
            
            ball.BounceHorizontal();
            
            Assert.AreEqual(-2, ball.Velocity.X);
            Assert.AreEqual(2, ball.Velocity.Y);
        }

        // Тест 3: Ограничение движения ракетки
        [TestMethod]
        public void TestPlayerPaddle_MoveUp_DoesNotGoBelowZero()
        {
            var paddle = new PlayerPaddle(new GamePoint(5, 0));
            
            paddle.MoveUp();
            
            Assert.AreEqual(0, paddle.Position.Y); // Должно остаться на 0
        }
        
        // Тест 4: Увеличение счета
        [TestMethod]
        public void TestScore_AddPlayerScore_IncrementsScore()
        {
            var score = new Score();
            
            score.AddPlayerScore();
            
            Assert.AreEqual(1, score.PlayerScore);
            Assert.AreEqual(0, score.AiScore);
        }

        // Тест 5: Стратегия ИИ
        [TestMethod]
        public void TestAiPaddle_StrategyMovesTowardsBall()
        {
            var ai = new AiPaddle(new GamePoint(50, 10), new FollowBallStrategy());
            var ball = new PlayerBall(new GamePoint(40, 2), new GameVector(1, 1));

            ai.Update(ball, 20);
            
            // ИСПРАВЛЕНИЕ: Вместо IsLessThan используем IsTrue с условием сравнения
            Assert.IsTrue(ai.Position.Y < 10, "AI должен двигаться вверх (уменьшать Y)");
        }
    }
}