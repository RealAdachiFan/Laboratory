using System;
using System.IO;
using System.Threading.Tasks;

namespace PongGame
{
    public class GameService
    {
        private string _filePath = "highscore.dat"; // Файл для сохранения рекорда

        // 1. Асинхронная работа с файлами (I/O Bound)
        // Используем await, чтобы не блокировать интерфейс во время записи на диск
        public async Task SaveHighScoreAsync(int score)
        {
            // Сохраняем в файл только если новый счет выше текущего
            int currentBest = await LoadHighScoreAsync();
            if (score > currentBest)
            {
                await File.WriteAllTextAsync(_filePath, score.ToString());
            }
        }

        public async Task<int> LoadHighScoreAsync()
        {
            if (!File.Exists(_filePath)) return 0;
            string content = await File.ReadAllTextAsync(_filePath);
            return int.TryParse(content, out int score) ? score : 0;
        }

        // 2. Асинхронные вычисления (CPU Bound)
        // Используем Task.Run, чтобы вынести тяжелый расчет в отдельный поток
        public async Task<int> CalculateOptimalDefenseAsync(PlayerBall ball, int fieldHeight)
        {
            return await Task.Run(() =>
            {
                // Симуляция сложного математического расчета (например, метод Монте-Карло)
                // Здесь мы просто нагружаем поток, чтобы показать пример
                int result = 0;
                for (int i = 0; i < 5000000; i++) 
                {
                    // Эмуляция вычислений
                    result += ball.Position.Y * ball.Velocity.X; 
                }
                
                // Возвращаем результат (в данном примере просто Y позицию мяча)
                return ball.Position.Y;
            });
        }
    }
}