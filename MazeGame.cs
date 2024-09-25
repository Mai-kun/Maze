internal class MazeGame
{
    private const int WidthMaze = 41;
    private const int HeightMaze = 21;
    private const int ExitX = WidthMaze - 2;
    private const int ExitY = HeightMaze - 2;
    private const int VisibilityRadius = 10;

    private const char WallCell = '█';
    private const char RoadCell = ' ';
    private const char ExitCell = 'E';
    private const char PlayerCell = '@';
    private const char FogCell = '?';
    private const char PathCell = '*';

    private readonly char[,] maze = new char[HeightMaze, WidthMaze];
    private int playerX = 1;
    private int playerY = 1;

    /// <summary>
    /// Start the game
    /// </summary>
    public void Start()
    {
        GenerateMaze();
        ConsoleKeyInfo keyInfo;
        while (playerX != ExitX || playerY != ExitY)
        {
            PrintMaze();
            Console.WriteLine("Цель: Найти выход из лабиринта.");
            Console.WriteLine("Используйте W, A, S, D для передвижения.");
            Console.WriteLine("Q - выйти. E - показать путь к выходу");

            keyInfo = Console.ReadKey();
            MovePlayer(keyInfo.Key);

            if (keyInfo.Key == ConsoleKey.E)
            {
                var path = FindPath(playerX, playerY, ExitX, ExitY);
                ShowPath(path);
            }

            if (keyInfo.Key == ConsoleKey.Q)
            {
                return;
            }

            Console.Clear();
        };

        Console.Clear();
        PrintMaze();
        Console.WriteLine("Поздравляем! Вы нашли выход!");
    }

    private void GenerateMaze()
    {
        for (var y = 0; y < HeightMaze; y++)
        {
            for (var x = 0; x < WidthMaze; x++)
            {
                maze[y, x] = WallCell;
            }
        }

        maze[playerY, playerX] = RoadCell;
        CarveMaze(playerX, playerY);

        maze[ExitY, ExitX] = ExitCell;
    }

    private void CarveMaze(int posX, int posY)
    {
        var random = new Random();

        int[] directionX = { 0, 0, -2, 2 };
        int[] directionY = { -2, 2, 0, 0 };

        for (var i = 0; i < 4; i++)
        {
            var r = random.Next(4);
            var tempX = directionX[i];
            var tempY = directionY[i];
            directionX[i] = directionX[r];
            directionY[i] = directionY[r];
            directionX[r] = tempX;
            directionY[r] = tempY;
        }

        for (var i = 0; i < 4; i++)
        {
            var newPosX = posX + directionX[i];
            var newPosY = posY + directionY[i];

            if (newPosX > 0 && newPosX < WidthMaze &&
                newPosY > 0 && newPosY < HeightMaze &&
                maze[newPosY, newPosX] == WallCell)
            {
                maze[newPosY, newPosX] = RoadCell;
                maze[posY + directionY[i] / 2, posX + directionX[i] / 2] = RoadCell;
                CarveMaze(newPosX, newPosY);
            }
        }
    }

    private void PrintMaze()
    {
        for (var y = 0; y < HeightMaze; y++)
        {
            for (var x = 0; x < WidthMaze; x++)
            {
                var distance = Math.Abs(playerX - x) + Math.Abs(playerY - y);

                if (distance <= VisibilityRadius && IsVisible(x, y))
                {
                    if (x == playerX && y == playerY)
                    {
                        Console.Write(PlayerCell);
                    }
                    else
                    {
                        Console.Write(maze[y, x]);
                    }
                }
                else
                {
                    Console.Write(FogCell);
                }
            }
            Console.WriteLine();
        }
    }

    private bool IsVisible(int x, int y)
    {
        const int DiagonalVisibility = 1;

        var distanceX = Math.Abs(x - playerX);
        var distanceY = Math.Abs(y - playerY);

        if (distanceX > DiagonalVisibility && distanceY > DiagonalVisibility)
        {
            return false;
        }

        var sx = playerX < x ? 1 : -1;
        var sy = playerY < y ? 1 : -1;
        var diagonalDistance = distanceX - distanceY;

        var currentX = playerX;
        var currentY = playerY;

        while (true)
        {
            if (currentX == x && currentY == y)
            {
                return true;
            }

            if (maze[currentY, currentX] == WallCell)
            {
                return false;
            }

            var e2 = VisibilityRadius * diagonalDistance;
            if (e2 > -distanceY)
            {
                diagonalDistance -= distanceY;
                currentX += sx;
            }
            if (e2 < distanceX)
            {
                diagonalDistance += distanceX;
                currentY += sy;
            }
        }
    }

    private void MovePlayer(ConsoleKey key)
    {
        var newX = playerX;
        var newY = playerY;

        switch (key)
        {
            case ConsoleKey.W:
                newY--;
                break;
            case ConsoleKey.S:
                newY++;
                break;
            case ConsoleKey.A:
                newX--;
                break;
            case ConsoleKey.D:
                newX++;
                break;
        }

        if (maze[newY, newX] != WallCell)
        {
            playerX = newX;
            playerY = newY;
        }
    }

    private List<(int, int)>? FindPath(int startX, int startY, int goalX, int goalY)
    {
        var queue = new Queue<(int, int)>();
        var visited = new bool[HeightMaze, WidthMaze];
        var previous = new (int, int)[HeightMaze, WidthMaze];

        queue.Enqueue((startX, startY));
        visited[startY, startX] = true;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            (var x, var y) = queue.Dequeue();

            if (x == goalX && y == goalY)
            {
                var path = new List<(int, int)>();
                while (x != startX || y != startY)
                {
                    path.Add((x, y));
                    (x, y) = previous[y, x];
                }
                path.Add((startX, startY));
                path.Reverse();
                return path;
            }

            for (var i = 0; i < 4; i++)
            {
                var nx = x + dx[i];
                var ny = y + dy[i];

                if (nx >= 0 && nx < WidthMaze && ny >= 0 && ny < HeightMaze && !visited[ny, nx] && maze[ny, nx] != WallCell)
                {
                    queue.Enqueue((nx, ny));
                    visited[ny, nx] = true;
                    previous[ny, nx] = (x, y);
                }
            }
        }

        return null;
    }

    private static void ShowPath(List<(int, int)> path)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        foreach ((var x, var y) in path)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(PathCell);
        }
        Console.SetCursorPosition(0, HeightMaze + 2);
        Console.ResetColor();
        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
        Console.ReadKey();
    }

}
