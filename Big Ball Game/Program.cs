using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

struct PointF
{
    public float X { get; set; }
    public float Y { get; set; }

    public PointF(float x, float y)
    {
        X = x;
        Y = y;
    }
}

abstract class Ball
{
    public float Radius { get; set; }
    public PointF Position { get; set; }
    public float Dx { get; set; }
    public float Dy { get; set; }

    public Ball(float radius, PointF position, float dx, float dy)
    {
        Radius = radius;
        Position = position;
        Dx = dx;
        Dy = dy;
    }

    public virtual void UpdatePosition(int width, int height)
    {
        Position = new PointF(Position.X + Dx, Position.Y + Dy);

        if (Position.X - Radius < 0 || Position.X + Radius > width)
        {
            Dx = -Dx;
        }
        if (Position.Y - Radius < 0 || Position.Y + Radius > height)
        {
            Dy = -Dy;
        }
    }

    public virtual string GetBallType()
    {
        return "Ball";
    }

    public override string ToString()
    {
        return $"{GetBallType()}: Position ({Position.X}, {Position.Y}), Radius {Radius}";
    }
}

class RegularBall : Ball
{
    public RegularBall(float radius, PointF position, float dx, float dy)
        : base(radius, position, dx, dy) { }

    public override string GetBallType()
    {
        return "Regular Ball";
    }
}

class MonsterBall : Ball
{
    public MonsterBall(float radius, PointF position)
        : base(radius, position, 0, 0) { }

    public void AdjustPosition(int width, int height)
    {
        Position = new PointF(
            Math.Max(Radius, Math.Min(width - Radius, Position.X)),
            Math.Max(Radius, Math.Min(height - Radius, Position.Y))
        );
    }

    public override string GetBallType()
    {
        return "Monster Ball";
    }
}

class RepellentBall : Ball
{
    public RepellentBall(float radius, PointF position, float dx, float dy)
        : base(radius, position, dx, dy) { }

    public override string GetBallType()
    {
        return "Repellent Ball";
    }
}

class Canvas
{
    private int width;
    private int height;
    private List<Ball> balls;

    public Canvas(int width, int height, int regularBalls, int monsterBalls, int repellentBalls)
    {
        this.width = width;
        this.height = height;
        balls = new List<Ball>();
        Random random = new Random();

        InitializeBalls<RegularBall>(regularBalls, random);
        InitializeBalls<MonsterBall>(monsterBalls, random);
        InitializeBalls<RepellentBall>(repellentBalls, random);
    }

    private void InitializeBalls<T>(int count, Random random) where T : Ball
    {
        for (int i = 0; i < count; i++)
        {
            if (typeof(T) == typeof(RegularBall))
            {
                balls.Add(new RegularBall(
                    random.Next(5, 15),
                    new PointF(random.Next(0, width), random.Next(0, height)),
                    (float)(random.NextDouble() * 2 - 1),
                    (float)(random.NextDouble() * 2 - 1)
                ));
            }
            else if (typeof(T) == typeof(MonsterBall))
            {
                balls.Add(new MonsterBall(
                    random.Next(10, 20),
                    new PointF(random.Next(0, width), random.Next(0, height))
                ));
            }
            else if (typeof(T) == typeof(RepellentBall))
            {
                balls.Add(new RepellentBall(
                    random.Next(5, 15),
                    new PointF(random.Next(0, width), random.Next(0, height)),
                    (float)(random.NextDouble() * 2 - 1),
                    (float)(random.NextDouble() * 2 - 1)
                ));
            }
        }
    }

    public void Turn()
    {
        foreach (var ball in balls)
        {
            ball.UpdatePosition(width, height);
        }

        // Ensure monster balls are within bounds and not overlapping
        foreach (var ball in balls.OfType<MonsterBall>())
        {
            ball.AdjustPosition(width, height);
        }

        CheckCollisions();

        balls.RemoveAll(b => b.Radius <= 0);
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                if (IsCollision(balls[i], balls[j]))
                {
                    HandleCollision(balls[i], balls[j]);
                }
            }
        }
    }

    private bool IsCollision(Ball b1, Ball b2)
    {
        float dx = b1.Position.X - b2.Position.X;
        float dy = b1.Position.Y - b2.Position.Y;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
        return distance < b1.Radius + b2.Radius;
    }

    private void HandleCollision(Ball b1, Ball b2)
    {
        if (b1 is RegularBall && b2 is RegularBall)
        {
            AbsorbRegularBalls((RegularBall)b1, (RegularBall)b2);
        }
        else if ((b1 is RegularBall && b2 is MonsterBall) || (b1 is MonsterBall && b2 is RegularBall))
        {
            AbsorbByMonsterBall(b1, b2);
        }
        else if ((b1 is RegularBall && b2 is RepellentBall) || (b1 is RepellentBall && b2 is RegularBall))
        {
            RepelRegularBall(b1, b2);
        }
        else if (b1 is RepellentBall && b2 is RepellentBall)
        {
            SwapRepellentBalls((RepellentBall)b1, (RepellentBall)b2);
        }
        else if ((b1 is RepellentBall && b2 is MonsterBall) || (b1 is MonsterBall && b2 is RepellentBall))
        {
            HalveRepellentBall(b1, b2);
        }
    }

    private void AbsorbRegularBalls(RegularBall b1, RegularBall b2)
    {
        if (b1.Radius > b2.Radius)
        {
            AbsorbBall(b1, b2);
        }
        else
        {
            AbsorbBall(b2, b1);
        }
    }

    private void AbsorbByMonsterBall(Ball b1, Ball b2)
    {
        MonsterBall monster = b1 is MonsterBall ? (MonsterBall)b1 : (MonsterBall)b2;
        RegularBall regular = b1 is RegularBall ? (RegularBall)b1 : (RegularBall)b2;
        monster.Radius += regular.Radius;
        regular.Radius = 0;
    }

    private void RepelRegularBall(Ball b1, Ball b2)
    {
        RegularBall regular = b1 is RegularBall ? (RegularBall)b1 : (RegularBall)b2;
        regular.Dx = -regular.Dx;
        regular.Dy = -regular.Dy;
    }

    private void SwapRepellentBalls(RepellentBall b1, RepellentBall b2)
    {
        var tempPosition = b1.Position;
        b1.Position = b2.Position;
        b2.Position = tempPosition;
    }

    private void HalveRepellentBall(Ball b1, Ball b2)
    {
        RepellentBall repellent = b1 is RepellentBall ? (RepellentBall)b1 : (RepellentBall)b2;
        repellent.Radius /= 2;
    }

    private void AbsorbBall(Ball larger, Ball smaller)
    {
        larger.Radius += smaller.Radius;
        smaller.Radius = 0;
    }

    public bool IsFinished()
    {
        return !balls.Any(b => b is RegularBall);
    }

    public void Display()
    {
        Console.Clear();
        foreach (var ball in balls)
        {
            Console.WriteLine(ball.ToString());
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        int width = 800;
        int height = 600;
        int regularBalls = 10;
        int monsterBalls = 2;
        int repellentBalls = 3;

        Canvas canvas = new Canvas(width, height, regularBalls, monsterBalls, repellentBalls);

        while (!canvas.IsFinished())
        {
            canvas.Turn();
            canvas.Display();
            Thread.Sleep(400);
        }

        Console.WriteLine("Simulation finished.");
    }
}