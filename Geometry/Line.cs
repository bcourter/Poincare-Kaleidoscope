using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
	public class Line : CircLine {
		public Line(Complex b, double c)
			: base(0, b, c) {
			Debug.Assert(b != Complex.Zero);
			Debug.Assert(!double.IsNaN(c));
		}

		public static Line Create(Complex point, double angle) {

			return Create(point, point - Complex.CreatePolar(1, angle));
		}

		public static Line Create(Complex p1, Complex p2) {
			double dx = p2.Re - p1.Re;
			double dy = p2.Im - p1.Im;

			return Create(-dy, dx, dx * p1.Im - dy * p1.Re);
		}

		/// <summary>
		/// Creates a linear circle a * x + b * y + c = 0 from reals a, b, and c.
		/// </summary>
		/// <returns>
		/// The linear circle.
		/// </returns>
		/// <param name='a'>
		/// a.
		/// </param>
		/// <param name='b'>
		/// b.
		/// </param>
		/// <param name='c'>
		/// c.
		/// </param>
		public static Line Create(double a, double b, double c) {
			return new Line(new Complex(a, b) / 2, c);
		}

		public override Complex Evaluate(double t) {
			// TBD scale t for lines so +/-pi -> infinity
			double aa = 2 * b.Re;
			double bb = 2 * b.Im;
			if (Accuracy.LengthIsZero(aa))
				return new Complex(t, c / bb);

			if (Accuracy.LengthIsZero(bb))
				return new Complex(c / aa, t);

			//	double x = c / (aa * bb * bb * (1 / (aa * aa) + 1 / (bb * bb)));
			//	return  new ComplexPolar(t, LineAngle) + new Complex(x, (c - aa * x) / bb);

			Complex p0 = new Complex(c / aa, 0);
			Complex p1 = new Complex(0, c / bb);
			if (p1 == Complex.Zero)
				p1 = Complex.CreatePolar(1, Angle);

			return p0 + (p1 - p0).Normalized * t;
			//return (p0 + p1) / 2 + (p1 - p0).Normalized * t;
		}

		public override CircLine Translate(Complex translation) {
			return Create(Origin + translation, Angle);
		}

		public override CircLine Scale(Complex scale) {
			return Create(Origin * scale, Angle + scale.Argument);
		}

		public override Interval MinorInterval(double param0, double param1) {
			double min = Math.Min(param0, param1);
			double max = Math.Max(param0, param1);

			return new Interval(min, max);
		}

		public override Evaluation Project(Complex p) {
			double nearParam = Complex.Dot(p - Origin, Direction);
			Complex nearPoint = Evaluate(nearParam);
			return new Evaluation(nearPoint, nearParam);
		}

		public override List<Intersection> Intersect(CircLine other) {
			List<Intersection > intersections = new List<Intersection>();

			if (other is Circle) {
				Circle otherC = (Circle)other;
				intersections = otherC.Intersect(this);
				if (intersections == null)
					intersections = otherC.Intersect(this);

				return intersections.Select(i => new CircLine.Intersection(i.Point, i.ParamB, i.ParamA)).ToList();
			}

			Line line = (Line)other;

			Complex denominator = b.Conjugate * line.b - b * line.b.Conjugate;
			if (denominator == Complex.Zero)
				return null;

			Complex z = -(b * line.c - line.b * c) / denominator;
			intersections.Add(new Intersection(z, Project(z).Param, line.Project(z).Param));

			return intersections;
		}

		public override bool IsNormalTo(CircLine circLine) {
			if (circLine is Line)
				return Math.Abs(Angle - ((Line)circLine).Angle) % (2 * Math.PI) == Math.PI;

			Complex center = ((Circle)circLine).Center;
			return this.Project(center).Point == center;
		}

		#region properties
		public Complex Origin { get { return Evaluate(0); } }

		public double Angle { get { return Math.PI - Math.Atan2(b.Re, b.Im); } }
		//	public double Angle { get {return Math.Atan2(b.Im, b.Re); } }

		public Complex Direction { get { return Evaluate(1) - Evaluate(0); } }

		public override CircLine Conjugate { get { return new Line(b.Conjugate, c); } }

		//		public  override CircLine Inverse { get { return CircLine.Create(c, -b.Conjugate, 0).Normalized; } }
		public override CircLine Inverse {
			get {
				if (Accuracy.LengthIsZero(c / 1000))
					return new Line(b.Conjugate, 0);
				//	return this.Conjugate; // TBD do we really need conjugate -- fixed without bug, but appeared broken without it

				return Circle.Create(
					b.Conjugate / c, // similarly, this was b.conjugate, but the paper says this... http://www.cefns.nau.edu/~schulz/moe.pdf
					b.Modulus / c
				);
			}
		}

		public override CircLine Normalized { get { return new Line(b / c, 1); } }

		public override Mobius AsInversion {
			get {
				//	if (Accuracy.LengthIsZero(c))
				//		return new Mobius(new ComplexPolar(1, 2 * Angle), Complex.Zero, Complex.Zero, Complex.One);

				return new Mobius(b, Complex.One * c, Complex.Zero, -b.Conjugate);
			}
		}

		public override Complex[] Polyline {
			get {
				return new Complex[] { this.Evaluate(-Math.PI), this.Evaluate(Math.PI) };
			}
		}

		#endregion
	}

}
