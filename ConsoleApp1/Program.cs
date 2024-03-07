using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool isEnd = false;
            int width = 20;
            int height = 20;
            int count = 30;
            Console.Clear();
            Board board = new Board(count, width, height);
            board.Print();

            //Cursor
            int pointX = 0, pointY = 0;
            int prevX = 0, prevY = 0;
            Console.SetCursorPosition(0, 0);
            while (!isEnd)
            {
                //game
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                prevX = pointX;
                prevY = pointY;
                switch(keyInfo.Key)
                {
                    case ConsoleKey.Escape:
                        isEnd = true;
                        continue;
                    case ConsoleKey.F:
                        Console.Beep();
                        break;
                    case ConsoleKey.Spacebar:
                        Console.Beep(1600, 200);
                        board.Click(pointX, pointY);
                        break;
                    case ConsoleKey.LeftArrow:
                        pointX--;
                        break;
                    case ConsoleKey.RightArrow:
                        pointX++;
                        break;
                    case ConsoleKey.UpArrow:
                        pointY--;
                        break;
                    case ConsoleKey.DownArrow:
                        pointY++;
                        break;
                    default:
                        break;
                }
                //Fix pre
                board.PrintCell(prevX, prevY);
                //Validate and draw pointer
                if (pointX < 0) pointX = 0;
                if(pointY < 0) pointY = 0;
                if(pointX >= width) pointX = width - 1;
                if(pointY >= height) pointY = height - 1;
                Console.SetCursorPosition(2*pointX, pointY);
                Console.Write("\x1b[48;5;207m  ");
            }

            Console.WriteLine("\x1b[0m");
        }
    }

    internal class Board
    {
        private bool isReady = false;
        readonly char[,] Cells;
        readonly byte[,] Mask;
        readonly int Width;
        readonly int Height;
        readonly int Seed;
        readonly int BombCount;
        readonly Random random;

        static char CrunchChar = '9';

        int FlagCount = 0;

        internal Board(int BombCount, int Seed, int Width, int Height)
        {
            this.BombCount = BombCount;
            this.Seed = Seed;
            this.Width = Width;
            this.Height = Height;
            this.Cells = new char[Width, Height];
            this.Mask = new byte[Width, Height]; for (int i = 0; i < Width; i++) for (int j = 0; j < Height; j++) Mask[i, j] = 0;
            this.random = new Random(Seed);
        }

        internal Board(int BombCount, int Width, int Height) : this(BombCount, (int) (0x0000000ffffffff & DateTime.Now.Ticks), Width, Height)
        {

        }
        internal void Generate(int clickX, int clickY)
        {
            for (int x = 0; x < Width; x++) { 
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = '0';
                }
            }

            if (BombCount >= Width * Height)
                throw new Exception("On generate:   Saper instant explose. BombCount >= Width * Height");

            int placedBombCount = 0;
            unsafe
            {
                while(placedBombCount < BombCount)
                {
                    int x = random.Next(Width);
                    int y = random.Next(Height);

                    if (x == clickX && y == clickY)
                        continue;

                    char* elementAtArray = GetPointerToArrayElement(x, y);

                    if (elementAtArray == null)
                        throw new Exception($"On generate:   Point ({x},{y}) not found");

                    if (*elementAtArray > '9') //BOMB
                        continue;

                    *elementAtArray = 'b';
                    for(int _x = -1; _x <= 1; _x++) {
                        for (int _y = -1; _y <= 1; _y++)
                        {
                            char* neighbor = GetPointerToArrayElement(x+_x, y+_y);
                            if (*neighbor < '9')
                                (*neighbor)++;
                        }
                    }
                    placedBombCount++;
                }
            }
            isReady = true;
        }
        unsafe char* GetPointerToArrayElement(int x, int y)
        {
            fixed (char* crutch = &CrunchChar)
            {
                if (x < 0 || y < 0 || x >= this.Width || y >= this.Height)
                    return crutch;
                fixed (char* Point = &this.Cells[x, y])
                {
                    return Point;
                }
            }
        }
        internal void Click(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height)
                return;
            if (Mask[x, y] != 0)
                return;
            if (!isReady)
                Generate(x, y);
            Open(x, y);
        }

        private void Open(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height)
                return;
            if (Mask[x, y] != 0)
                return;
            char current = Cells[x, y];
            switch(current - 48)
            {
                case 0:
                    Mask[x, y] = 1;
                    for(int i = -1; i <= 1; i++) 
                        for(int j = -1; j <= 1; j++) 
                            Open(x + i, y + j); 
                    break;
                case 50:
                    for (int i = 0; i < Width; i++) for (int j = 0; j < Height; j++) Mask[i, j] = 1;
                    Print();
                    for (int i = 0; i < 3; i++)
                    {
                        Console.Beep(200, 100);
                        Console.Beep(150, 100);
                        Console.Beep(100, 100);
                        Console.Beep(300, 100);
                        Console.Beep(250, 100);
                    }
                    return;
                default:
                    Mask[x, y] = 1;
                    break;
            }
            PrintCell(x, y);
        }

        internal void PrintCell(int x, int y)
        {
            if (x < 0 || x >= Width)
                return;
            if (y < 0 || y >= Height)
                return;
            Console.SetCursorPosition(2*x, y);

            if(this.Mask[x,y] == 0)
            {
                Console.Write("\x1b[48;5;248m__");
                return;
            }
            if (this.Mask[x,y] == 2)
            {
                Console.Write("\x1b[48;5;95m\x1b[38;5;22m F");
                return;
            }


            char num = Cells[x, y];

            int background = 40;
            int foreground = 37;

            switch (num - 48)
            {
                case 50:
                    background = 7;
                    foreground = 30;
                    break;
                case 0:
                    background = 233;
                    foreground = 30;
                    break;
                case 1:
                    background = 21;
                    break;
                case 2:
                    background = 40;
                    break;
                case 3:
                    background = 196;
                    break;
                case 4:
                    background = 4;
                    break;
                case 5:
                    background = 1;
                    break;
                case 6:
                    background = 6;
                    break;
                case 7:
                    background = 235;
                    break;
                case 8:
                    background = 249;
                    break;
            }

            Console.Write($"\x1b[48;5;{background}m\x1b[{foreground}m {Cells[x, y]}");
        }
        internal void Print()
        {
            Console.Clear();
            for (int y = 0;y < this.Height; y++) { 
                for(int x = 0;x < this.Width; x++)
                {
                    PrintCell(x, y);
                }
                //if(y +1 < this.Height)
                   // Console.Write("\x1b[48;5;233m\x1b[37m\n");
            }
        }
    }
}
