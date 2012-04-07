using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
	// http://iweb.dl.sourceforge.net/project/poincare/poincare.pdf
	public struct FundamentalRegion {
		int p, q;
		CircLine l1, l2, c;
		double r, d, phi;

		public FundamentalRegion(int p, int q) {
			this.p = p;
			this.q = q;

			double sinP2 = Math.Pow(Math.Sin(Math.PI / p), 2);
			double cosQ2 = Math.Pow(Math.Cos(Math.PI / q), 2);
			r = Math.Sqrt(sinP2 / (cosQ2 - sinP2));
			d = Math.Sqrt(cosQ2 / (cosQ2 - sinP2));
			phi = Math.PI * (0.5 - ((double) 1 / p + (double) 1 / q));

			l1 = Line.Create(Complex.Zero, Complex.One);
			l2 = Line.Create(Complex.Zero, Math.PI / p);
			c = Circle.Create(Complex.One * d, r);
		}

		public TriangleMesh Mesh {
			/*         08
			 *       09  07
			 *     10  14  06
			 *   11  12  13  05
			 * 00  01  02  03  04
			 * */
			get {
				var points = new Complex[15];

				Complex p0 = Complex.Zero;
				Complex p1 = P1;
				Complex p2 = P2;

				int count = 4;
				for (int i = 0; i < count; i++) {
					double t = (double) i / count;
					points[i] = p0 * (1 - t) + p2 * t;
					points[i + count] = Complex.One * d + Complex.CreatePolar(r, Math.PI - phi * t);
					points[i + 2 * count] = p1 * (1 - t) + Complex.Zero * t;
				}

				points[12] = (p0 + (p1 + p2) / 2) / 2;
				points[13] = (p2 + (p0 + p1) / 2) / 2;
				points[14] = (p1 + (p2 + p0) / 2) / 2;

				return new TriangleMesh(points.ToArray());
			}
		}

		public void DrawGL() {
			l1.DrawGL(Color4.Red);
			List<CircLine.Intersection> intersections = l2.Intersect(Circle.Unit);
			Interval interval = l2.MinorInterval(intersections[0].ParamA, intersections[1].ParamA);
			TrimmedCircLine trimmedCircLine = new TrimmedCircLine(l2, interval);
			trimmedCircLine.DrawGL(Color4.Green);
			c.DrawGL(Color4.Blue);
		}

		public int P { get { return p; } }

		public int Q { get { return q; } }

		public CircLine L1 { get { return l1; } }

		public CircLine L2 { get { return l2; } }

		public CircLine C { get { return c; } }

		public Complex P1 { get { return Complex.One * d + Complex.CreatePolar(r, Math.PI - phi); } }

		public Complex P2 { get { return Complex.One * (d - r); } }



	}
}

