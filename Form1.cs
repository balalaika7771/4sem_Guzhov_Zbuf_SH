using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GRAF4_ZBUF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
			InitializeComponent();
			pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
			timer1.Interval = 10;
			timer1.Start();
		}

		TCube3D cube = new TCube3D("C:\\Qt\\cube.obj");//для создания кубика нам нужно дать ему путь к файлу из которого он будет читать координаты своих вершин
		ZBuffer zb = new ZBuffer(1000, 1000);
		double teta = 0, phi = 0, zer = 0;   //углы для поворота
		int RRRX = 100, RRRY = 100;//перемещение кубика по экрану 
		double zoom = 1;//увеличение кубика
		private void timer1_Tick(object sender, EventArgs e)
		{
			Graphics g = Graphics.FromImage(pictureBox1.Image);
			SolidBrush b = new SolidBrush(Color.White);
			g.FillRectangle(b, 0, 0, pictureBox1.Width, pictureBox1.Height);//каждую итерацию эта команда закрашивает окно полностью, чтобы нарисовать новый квадрат и чтобы получалась анимация
			
			cube.view_transformation(phi, teta, zer, RRRX, RRRY, zoom);
			cube.tobuf(zb);

			for (int j = 0; j < zb.sY; j+=2) {
				for (int i = 0; i < zb.sX; i += 2)
					if (zb.buff[i][j].color .Color!= Color.White)
					{
						g.DrawLine(zb.buff[i][j].color, i,j,i+1,j+1);

					}

			
		}
			zb.Clear();
			pictureBox1.Invalidate();
		}

      
		void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == (char)Keys.Up)//тут просто реакция на нажатие клавиш
			{
				teta += (Math.PI / (360));
			}
			if (e.KeyValue == (char)Keys.Down)
			{
				teta -= (Math.PI / (360));
			}

			if (e.KeyValue == (char)Keys.Left)
			{
				phi -= (Math.PI / (360));
			}
			if (e.KeyValue == (char)Keys.Right)
			{
				phi += (Math.PI / (360));
			}

			if (e.KeyValue == (char)Keys.M)
			{
				zer += (Math.PI / (360));
			}
			if (e.KeyValue == (char)Keys.N)
			{
				zer -= (Math.PI / (360));
			}


			if (e.KeyValue == (char)Keys.W)
			{
				RRRX += 10;
			}
			if (e.KeyValue == (char)Keys.S)
			{
				RRRX -= 10;
			}

			if (e.KeyValue == (char)Keys.D)
			{
				RRRY += 10;
			}
			if (e.KeyValue == (char)Keys.A)
			{
				RRRY -= 10;
			}


			if (e.KeyValue == (char)Keys.Z)
			{
				zoom += 0.1;
			}
			if (e.KeyValue == (char)Keys.X)
			{
				zoom -= 0.1;
			}
		}

	}

	struct Cel
	{
		public double z;
		public Pen color;
	};
	class ZBuffer
	{

		public Cel[][] buff; 
		public int sX, sY; // Размер Z-Буфера
		public ZBuffer(int x, int y)
        {
			sX = x; sY = y;
			buff = new Cel[sX][];
			for (int ax = 0; ax < sX; ax++)
            {
				buff[ax] = new Cel[sY];
				for(int ay = 0; ay < sY; ay++)
                {
					buff[ax][ay].z = 10000;
					buff[ax][ay].color = new Pen(Color.White);
				}
            }
		}
       
		public void PutTriangle(Triangle t)
        {
			int ymax, ymin, ysc, e1, e, i;
			int[] x = new int[3]; int [] y = new int[3];
			//Заносим x,y из t в массивы для последующей работы с ними
			for (i = 0; i < 3; i++)
			{
				x[i] = Convert.ToInt32(t.p[i].x); y[i] = Convert.ToInt32(t.p[i].y);
			}
			//Определяем максимальный и минимальный y
			ymax = ymin = y[0];
			if (ymax < y[1]) { ymax = y[1]; } else { if (ymin > y[1]) { ymin = y[1]; } }
			if (ymax < y[2]) { ymax = y[2]; } else { if (ymin > y[2]) { ymin = y[2]; } }
			ymin = (ymin < 0) ? 0 : ymin;
			ymax = (ymax < sY) ? ymax : sY;
			bool ne;
			int x1 = 0, x2= 0, xsc1 = 0, xsc2 = 0;
			double z1 = 0, z2 = 0, tc = 0, z = 0;
			//Следующий участок кода перебирает все строки сцены
			//и определяет глубину каждого пикселя
			//для соответствующего треугольника
			for (ysc = ymin; ysc < ymax; ysc++)
			{
				ne = false;
				for ( e = 0; e < 3; e++)
				{
					e1 = e + 1;
					if (e1 == 3) { e1 = 0; }
					if (y[e] < y[e1])
					{
						if (y[e1] <= ysc || ysc < y[e]) continue;
					}
					else if (y[e] > y[e1])
					{
						if (y[e1] > ysc || ysc >= y[e]) continue;
					}
					else continue;
					tc = Convert.ToDouble(y[e] - ysc) / (y[e] - y[e1]);
					if (ne)
					{
						x2 = x[e] + Convert.ToInt32(tc * (x[e1] - x[e]));
						z2 = t.p[e].z + tc * (t.p[e1].z - t.p[e].z);
					}
					else
					{
						x1 = x[e] + Convert.ToInt32(tc * (x[e1] - x[e]));
						z1 = t.p[e].z + tc * (t.p[e1].z - t.p[e].z);
						ne = true;
					}

				}
				if (x2 < x1) { e = x1; x1 = x2; x2 = e; tc = z1; z1 = z2; z2 = tc; }
				xsc1 = (x1 < 0) ? 0 : x1;
				xsc2 = (x2 < sX) ? x2 : sX;
				for (int xsc = xsc1; xsc < xsc2; xsc++)
				{
					tc = Convert.ToDouble(x1 - xsc) / (x1 - x2);
					z = z1 + tc * (z2 - z1);
					//Если полученная глубина пиксела меньше той,
					//что находится в Z-Буфере - заменяем храняшуюся на новую.
					if (z < (buff[ysc][xsc].z))
					{
						buff[ysc][xsc].color = t.color;
						buff[ysc][xsc].z = z;
					}
				}
			}

		}
		public void Clear()
        {
			for (int ax = 0; ax < sX; ax++)
			{
				for (int ay = 0; ay < sY; ay++)
				{
					buff[ax][ay].z = 10000;
					buff[ax][ay].color.Color = Color.White;
				}
			}


		}
	};


	class Triangle
	{
		public Pen color;
		public TPoint3D [] p = new TPoint3D[3];
		public Triangle(TPoint3D p1, TPoint3D p2, TPoint3D p3, Pen c)
		{
			p[0] = p1; p[1] = p2; p[2] = p3;
			color = c;
		}
	};
	class TPoint3D
	{
		public double x, y, z;

		public TPoint3D(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public void setx(double value) { x = value; }
		public void sety(double value) { y = value; }
		public void setz(double value) { z = value; }
		public void set(double value1, double value2, double value3)
		{
			x = value1; y = value2; z = value3;
		}

		public double getx() { return x; }
		public double gety() { return y; }
		public double getz() { return z; }

	};

	class TPoint
	{
		private double x, y;

		public void setx(double value) { x = value; }
		public void sety(double value) { y = value; }
		public double getx() { return x; }
		public double gety() { return y; }
	};

	class TCube3D
	{
		//в w из cube.obj записываем координаты вершин
		//мы знаем в каком порядке мы из записывали и в каком они тогда будум в w
		//поэтому в trian из cube.obj мы записываем НОМЕРА вершин которые храняться в w по которым можно нарисовать кубик 
		public List<TPoint3D> w = new List<TPoint3D>();  //мировые координаты
		//public List<TPoint> v = new List<TPoint>();  //видиовые координаты 
		public List<int> trian = new List<int>();
		//далее мы каждую вершину из w умножаем на матрицы поворотв, мастшабирования, перемещения и получаем
		//получаные точки записываем в WN и проэцируем на плоскость z записывая уже двумерные точки в v
		//pointw это центр кубика, он нужен для проверки робертса

		public List<Color> penz = new List<Color>();
		//public TPoint3D pointw;
		public List<TPoint3D> WN = new List<TPoint3D>();

		public TCube3D(string file_name)
		{
			String str;
			StreamReader sr = new StreamReader(file_name);
			str = sr.ReadLine();
			while (str != null)
			{
				if (str.Length >= 1)
				{
					if (str[0] == 'v')//если v то далее будем читать вершину кубика
					{
						int x, y, z;


						x = Int16.Parse(sr.ReadLine());
						y = Int16.Parse(sr.ReadLine());
						z = Int16.Parse(sr.ReadLine());
						TPoint3D point = new TPoint3D(x, y, z);
						w.Add(point);
						WN.Add(point);
					}
					if (str[0] == 'k')//если k то далее будем читать номера вершин по которым нужно нарисовать треугольник 
					{
						int p1, p2, p3;
						p1 = Int16.Parse(sr.ReadLine());
						p2 = Int16.Parse(sr.ReadLine());
						p3 = Int16.Parse(sr.ReadLine());

						trian.Add(p1);
						trian.Add(p2);
						trian.Add(p3);

					}
				}
				str = sr.ReadLine();
			}
			//close the file
			sr.Close();
			Random random = new Random();
			for(int i = 0; i < trian.Count / 3; i++)
            {
				penz.Add(Color.FromArgb(random.Next(250), random.Next(250), random.Next(250)));

            }
			
		}

		public void view_transformation(double LX, double LY, double LZ, double RRRX, double RRRY, double zoom)
		{
			//каждый кадр мы берём кубик который считали из файла и изменяем его по этим параметрам 
			double KX1, KY1, KZ1;
			double KX2, KY2, KZ2;
			double KX3, KY3, KZ3;
			for (int i = 0; i < w.Count; i++)
			{//берём по очереди все вершины кубика

				// умножаем на матрицу поворота X
				KX1 = w[i].getx();
				KY1 = w[i].gety() * Math.Cos(LX) + w[i].getz() * Math.Sin(LX);
				KZ1 = -w[i].gety() * Math.Sin(LX) + w[i].getz() * Math.Cos(LX);

				// умножаем на матрицу поворота Y
				KX2 = KX1 * Math.Cos(LY) - KZ1 * Math.Sin(LY);
				KY2 = KY1;
				KZ2 = KX1 * Math.Sin(LY) + KZ1 * Math.Cos(LY);

				// умножаем на матрицу поворота Z
				KX3 = KX2 * Math.Cos(LZ) + KY2 * Math.Sin(LZ);
				KY3 = -KX2 * Math.Sin(LZ) + KY2 * Math.Cos(LZ);
				KZ3 = KZ2;

				KX3 += RRRX;// перемещаем кубик в просранстве 
				KY3 += RRRY;

				KX3 *= zoom;// изменяем размер
				KY3 *= zoom;
				KZ3 *= zoom;

				TPoint3D NN = new TPoint3D(KX3, KY3, KZ3);
				WN[i] = NN;
			
			}

		}
	
		public void tobuf(ZBuffer zb)
        {
			int j = 0;
			for (int i = 0; i < trian.Count; i = i + 3)
			{
				TPoint3D p1 =  new TPoint3D(WN[trian[i]].getx(), WN[trian[i]].gety(), WN[trian[i]].getz());
				TPoint3D p2 = new TPoint3D(WN[trian[i+1]].getx(), WN[trian[i+1]].gety(), WN[trian[i+1]].getz());
				TPoint3D p3 = new TPoint3D(WN[trian[i+2]].getx(), WN[trian[i+2]].gety(), WN[trian[i+2]].getz());

				Pen o = new Pen(penz[j]);
				Triangle t1 = new Triangle(p1, p2, p3,o);
				zb.PutTriangle(t1);
				j++;
			}
		}
	};
}
