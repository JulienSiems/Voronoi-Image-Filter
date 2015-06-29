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
using System.Windows.Forms;

namespace Voronoi2
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
