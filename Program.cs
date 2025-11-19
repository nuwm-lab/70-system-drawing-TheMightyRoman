using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LabWork16
{
    // Головний клас програми (Точка входу)
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Запускаємо нашу форму
            Application.Run(new MainForm());
        }
    }

    // Клас форми, де відбувається малювання
    public class MainForm : Form
    {
        // --- Параметри Варіанту 16 ---
        // y = (5 * ln(x)) / (x^2 - 1)
        // 1.2 <= x <= 3.8, крок 0.4
        private const double XStart = 1.2;
        private const double XEnd = 3.8;
        private const double Step = 0.4;

        // Відступи від країв вікна для краси
        private readonly int _margin = 60;

        public MainForm()
        {
            this.Text = "Варіант 16: Графік функції";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(400, 300);
            
            // Подвійна буферизація прибирає мерехтіння при зміні розміру
            this.DoubleBuffered = true;
            
            // При зміні розміру вікна - перемальовуємо
            this.Resize += (s, e) => this.Invalidate();
        }

        // Метод, який викликається системою для малювання
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Згладжування

            // 1. Отримуємо координати точок
            List<PointInfo> points = CalculatePoints();

            if (points.Count < 2) return;

            // 2. Знаходимо межі (Min/Max) для масштабування
            double minX = XStart;
            double maxX = XEnd;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var p in points)
            {
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            // Додамо трохи простору зверху і знизу графіка, щоб лінії не липли до країв
            double yPadding = (maxY - minY) * 0.1; 
            minY -= yPadding;
            maxY += yPadding;

            // 3. Розрахунок масштабних коефіцієнтів
            // Ширина і висота області малювання
            float w = this.ClientSize.Width - 2 * _margin;
            float h = this.ClientSize.Height - 2 * _margin;

            // Перевірка на ділення на нуль (якщо вікно занадто мале)
            if (w <= 0 || h <= 0) return;

            // Коефіцієнти: скільки пікселів в одній математичній одиниці
            double scaleX = w / (maxX - minX);
            double scaleY = h / (maxY - minY);

            // Локальна функція: Перетворення координат (Математика -> Екран)
            PointF ToScreen(double x, double y)
            {
                float sx = _margin + (float)((x - minX) * scaleX);
                // Інвертуємо Y, бо на екрані 0 зверху
                float sy = _margin + h - (float)((y - minY) * scaleY);
                return new PointF(sx, sy);
            }

            // 4. Малювання осей координат (рамки)
            using (Pen axisPen = new Pen(Color.Black, 1))
            {
                // Вісь Y (зліва)
                g.DrawLine(axisPen, _margin, _margin, _margin, _margin + h);
                // Вісь X (знизу)
                g.DrawLine(axisPen, _margin, _margin + h, _margin + w, _margin + h);
            }

            // 5. Малювання графіка та точок
            using (Pen linePen = new Pen(Color.Blue, 2))
            using (Brush pointBrush = new SolidBrush(Color.Red))
            using (Font font = new Font("Arial", 8))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                PointF? prevPoint = null;

                foreach (var p in points)
                {
                    PointF currentScreenPoint = ToScreen(p.X, p.Y);

                    // Малюємо лінію від попередньої точки до поточної
                    if (prevPoint != null)
                    {
                        g.DrawLine(linePen, prevPoint.Value, currentScreenPoint);
                    }

                    // Малюємо кружечок (точку)
                    float r = 3; // радіус точки
                    g.FillEllipse(pointBrush, currentScreenPoint.X - r, currentScreenPoint.Y - r, 2 * r, 2 * r);

                    // Підписуємо координати біля точки
                    string label = $"({p.X:F1}; {p.Y:F2})";
                    g.DrawString(label, font, textBrush, currentScreenPoint.X + 5, currentScreenPoint.Y - 15);

                    prevPoint = currentScreenPoint;
                }
            }

            // 6. Підпис осей
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                g.DrawString("X", font, Brushes.Black, _margin + w + 5, _margin + h);
                g.DrawString("Y", font, Brushes.Black, _margin, _margin - 20);
            }
        }

        // Розрахунок точок згідно варіанту
        private List<PointInfo> CalculatePoints()
        {
            var list = new List<PointInfo>();
            
            // Проходимо циклом від 1.2 до 3.8 з кроком 0.4
            // Використовуємо Decimal для точного кроку, щоб уникнути похибок (1.60000001)
            for (decimal xDec = (decimal)XStart; xDec <= (decimal)XEnd; xDec += (decimal)Step)
            {
                double x = (double)xDec;
                double y = CalculateFormula(x);
                list.Add(new PointInfo { X = x, Y = y });
            }
            return list;
        }

        // Формула варіанту 16
        private double CalculateFormula(double x)
        {
            // y = (5 * ln(x)) / (x^2 - 1)
            // Захист від ділення на нуль (хоча в діапазоні [1.2; 3.8] знаменник не стає 0)
            if (Math.Abs(x * x - 1) < 1e-9) return 0;
            
            return (5 * Math.Log(x)) / (x * x - 1);
        }

        // Структура для зберігання пари координат
        private struct PointInfo
        {
            public double X;
            public double Y;
        }
    }
}
