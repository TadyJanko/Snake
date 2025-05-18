using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Snake
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new SnakeGame();
            game.Run();
        }
    }

    class SnakeGame
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly Random _random;
        private readonly Snake _snake;
        private Position _food;
        private int _score;
        private bool _isGameOver;
        private Direction _currentDirection;
        private DateTime _lastFrameTime;

        public SnakeGame()
        {
            _screenWidth = 32;
            _screenHeight = 16;
            _random = new Random();
            _snake = new Snake(new Position(_screenWidth / 2, _screenHeight / 2), ConsoleColor.Red);
            _score = 5;
            _currentDirection = Direction.Right;
            SpawnFood();
        }

        public void Run()
        {
            Console.WindowHeight = _screenHeight;
            Console.WindowWidth = _screenWidth;

            while (!_isGameOver)
            {
                _lastFrameTime = DateTime.Now;
                
                DrawGame();
                HandleInput();
                UpdateGame();

                // Wait for the remaining time to complete the 500ms frame
                var elapsed = DateTime.Now.Subtract(_lastFrameTime).TotalMilliseconds;
                if (elapsed < 500)
                {
                    Thread.Sleep((int)(500 - elapsed));
                }
            }

            ShowGameOver();
        }

        private void DrawGame()
        {
            Console.Clear();
            DrawBorders();
            DrawSnake();
            DrawFood();
        }

        private void DrawBorders()
        {
            for (int i = 0; i < _screenWidth; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write("■");
                Console.SetCursorPosition(i, _screenHeight - 1);
                Console.Write("■");
            }

            for (int i = 0; i < _screenHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("■");
                Console.SetCursorPosition(_screenWidth - 1, i);
                Console.Write("■");
            }
        }

        private void DrawSnake()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var segment in _snake.Body.Skip(1))
            {
                Console.SetCursorPosition(segment.X, segment.Y);
                Console.Write("■");
            }

            Console.ForegroundColor = _snake.HeadColor;
            Console.SetCursorPosition(_snake.Head.X, _snake.Head.Y);
            Console.Write("■");
        }

        private void DrawFood()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(_food.X, _food.Y);
            Console.Write("■");
        }

        private void HandleInput()
        {
            bool buttonPressed = false;
            var inputStartTime = DateTime.Now;

            while (true)
            {
                var currentTime = DateTime.Now;
                if (currentTime.Subtract(inputStartTime).TotalMilliseconds > 500) { break; }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    var newDirection = _currentDirection;

                    switch (key)
                    {
                        case ConsoleKey.UpArrow when _currentDirection != Direction.Down && !buttonPressed:
                            newDirection = Direction.Up;
                            buttonPressed = true;
                            break;
                        case ConsoleKey.DownArrow when _currentDirection != Direction.Up && !buttonPressed:
                            newDirection = Direction.Down;
                            buttonPressed = true;
                            break;
                        case ConsoleKey.LeftArrow when _currentDirection != Direction.Right && !buttonPressed:
                            newDirection = Direction.Left;
                            buttonPressed = true;
                            break;
                        case ConsoleKey.RightArrow when _currentDirection != Direction.Left && !buttonPressed:
                            newDirection = Direction.Right;
                            buttonPressed = true;
                            break;
                    }

                    _currentDirection = newDirection;
                }
            }
        }

        private void UpdateGame()
        {
            var newHead = _snake.Head.Move(_currentDirection);

            if (IsCollision(newHead))
            {
                _isGameOver = true;
                return;
            }

            _snake.Move(newHead);

            if (newHead.Equals(_food))
            {
                _score++;
                SpawnFood();
            }

            while (_snake.Body.Count > _score)
            {
                _snake.RemoveTail();
            }
        }

        private bool IsCollision(Position position)
        {
            return position.X <= 0 || position.X >= _screenWidth - 1 ||
                   position.Y <= 0 || position.Y >= _screenHeight - 1 ||
                   _snake.Body.Any(p => p.Equals(position));
        }

        private void SpawnFood()
        {
            do
            {
                _food = new Position(
                    _random.Next(1, _screenWidth - 2),
                    _random.Next(1, _screenHeight - 2)
                );
            } while (_snake.Body.Any(p => p.Equals(_food)));
        }

        private void ShowGameOver()
        {
            Console.SetCursorPosition(_screenWidth / 5, _screenHeight / 2);
            Console.WriteLine($"Game over, Score: {_score}");
        }
    }

    class Snake
    {
        private readonly List<Position> _body;
        public ConsoleColor HeadColor { get; }

        public Position Head => _body[0];
        public IReadOnlyList<Position> Body => _body;

        public Snake(Position initialPosition, ConsoleColor headColor)
        {
            _body = new List<Position> { initialPosition };
            HeadColor = headColor;
        }

        public void Move(Position newHead)
        {
            _body.Insert(0, newHead);
        }

        public void RemoveTail()
        {
            if (_body.Count > 1)
            {
                _body.RemoveAt(_body.Count - 1);
            }
        }
    }

    struct Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position Move(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Position(X, Y - 1),
                Direction.Down => new Position(X, Y + 1),
                Direction.Left => new Position(X - 1, Y),
                Direction.Right => new Position(X + 1, Y),
                _ => this
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Position other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
//¦