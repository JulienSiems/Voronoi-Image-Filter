/*
 * Created by SharpDevelop.
 * User: Burhan
 * Date: 11/05/2014
 * Time: 01:02 ص
 * 
 * C# Program by Burhan Joukhadar
 * Permission to use, copy, modify, and distribute this software for any
 * purpose without fee is hereby granted, provided that this entire notice
 * is included in all copies of any software which is or includes a copy
 * or modification of this software and in all copies of the supporting
 * documentation for such software.
 * THIS SOFTWARE IS BEING PROVIDED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
 * WARRANTY.  IN PARTICULAR, NEITHER THE AUTHORS NOR AT&T MAKE ANY
 * REPRESENTATION OR WARRANTY OF ANY KIND CONCERNING THE MERCHANTABILITY
 * OF THIS SOFTWARE OR ITS FITNESS FOR ANY PARTICULAR PURPOSE.
 * 
 * This is a program that draws Voronoi Diagram using Fortune's Algorithm
 * This program is to evaluate the and view the resulting voronoi diagram
 * Also it gives an example of how to use the voronoi object, it's not optimized actually it's rushed.
 */

using System;
using System.Collections.Generic;
using CSPoint = System.Drawing.Point; // "Point" بسبب وجود تضارب في اسم النوع
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Voronoi2
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		Bitmap bitmap;
		Bitmap background;
		Graphics g;
		Random seeder;
		Voronoi voroObject;
		static int siteCount = 20;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			seeder = new Random();
			pb.AutoSize = true;
			bitmap = new Bitmap (512,512);
			
			background = new Bitmap ( 512, 512 );
			Graphics g2 = Graphics.FromImage ( background );
			g2.Clear (Color.White);
			g2 = null;
			
			g = Graphics.FromImage (bitmap);
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.Clear (Color.White);
			pb.Image = bitmap;
			this.AutoSize = true;
			
			voroObject = new Voronoi ( 0.1 );
		}
		
		void spreadPoints()
		{
			g.Clear ( Color.White );
			
			List<PointF> sites = new List<PointF>();
			int seed = seeder.Next();
			Random rand = new Random ( seed );
			
			richTextBox1.Text += "\nSEED: " + seed;
			
			for ( int i = 0; i < siteCount; i++ )
			{
				sites.Add ( new PointF ( (float)(rand.NextDouble() * 512), (float)(rand.NextDouble() * 512) ) );
			}
			
			// رسم المواقع
			for (int i = 0; i < sites.Count; i++)
			{
				g.FillEllipse ( Brushes.Blue, sites[i].X-1.5f, sites[i].Y-1.5f, 3, 3 );
			}
			
			List<GraphEdge> ge;
			ge = MakeVoronoiGraph ( sites, bitmap.Width, bitmap.Height );
			
			// رسم أضلاع فورونوي
			for ( int i = 0; i < ge.Count; i++ )
			{
				try
				{
					CSPoint p1 = new CSPoint( (int)ge[i].x1, (int)ge[i].y1 );
					CSPoint p2 = new CSPoint( (int)ge[i].x2, (int)ge[i].y2 );
					g.DrawLine (Pens.Black, p1.X, p1.Y, p2.X, p2.Y );
				}catch{
					string s = "\nP " + i + ": " + ge[i].x1 + ", " + ge[i].y1 + " || " + ge[i].x2 + ", " + ge[i].y2;
					richTextBox1.Text += s;
				}
			}
			pb.Image = bitmap;
		}
		
		List<GraphEdge> MakeVoronoiGraph ( List<PointF> sites, int width, int height )
		{
			double[] xVal = new double[sites.Count];
			double[] yVal = new double[sites.Count];
			for ( int i = 0; i < sites.Count; i++ )
			{
				xVal[i] = sites[i].X;
				yVal[i] = sites[i].Y;
			}
			return voroObject.generateVoronoi ( xVal, yVal, 0, width, 0, height );
			
		}
		
		
		void Button1Click(object sender, EventArgs e)
		{
			this.richTextBox1.Text += "\n******* NEW TEST *******";
			spreadPoints();
			//background = Clone32BPPBitmap ( bitmap );
		}
		void NumericUpDown1ValueChanged(object sender, EventArgs e)
		{
			siteCount = (int)(numericUpDown1.Value);
			spreadPoints();
			//background = Clone32BPPBitmap ( bitmap );
		}
		
		void PbMouseMove(object sender, MouseEventArgs e)
		{
			label1.Text = e.X + ", " + e.Y;
		}
	}
}
