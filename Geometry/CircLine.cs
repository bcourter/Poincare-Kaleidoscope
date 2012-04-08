using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
	// http://www.cefns.nau.edu/~schulz/moe.pdf
	public abstract class CircLine {
		protected double a, c;
		protected Complex b;

		protected CircLine(double a, Complex b, double c) {
			if (a == 0) {
				double scale = 1 / b.ModulusFastApproximate;
				b *= scale;
				c *= scale;
			}


			this.a = a;
			this.b = b;
			this.c = c;
		}

		public static CircLine Create(double a, Complex b, double c) {
			if (Accuracy.LengthIsZero(a))
				return new Line(b, c);

			return new Circle(a, b, c);
		}

		public abstract Complex Evaluate(double t);

		public abstract List<Intersection> Intersect(CircLine other);

		// these sources don't seem to agree, and what I ended up using is still different.  WHY?
		// http://en.wikipedia.org/wiki/Generalised_circle
		// http://www.math.ubc.ca/~cass/research/pdf/Geometry.pdf
		// http://www.math.okstate.edu/~wrightd/INDRA/MobiusonCircles.mpl
		public static CircLine operator *(Mobius m, CircLine circLine) {
#if true 
			Mobius inverse = m.Inverse;
			Mobius hermitian = inverse.Transpose *
				new Mobius(new Complex(circLine.a, 0), circLine.b.Conjugate, circLine.b, new Complex(circLine.c, 0)) *
				inverse.Conjugate;

			return CircLine.Create(hermitian.A.Re, hermitian.C, hermitian.D.Re);
#else // do it by decomposing the mobius -- slower
			Complex a = m.A;
			Complex b = m.B;
			Complex c = m.C;
			Complex d = m.D;

			CircLine toInvert, inverted, scaled;
			if (c == Complex.Zero) {
				scaled = circLine.Scale(a / d);
				return scaled.Translate(b / d);
			}

			toInvert = circLine.Translate(d / c);
			inverted = toInvert.Inverse;
			scaled = inverted.Scale(-(a * d - b * c) / (c * c));
			return scaled.Translate(a / c);
#endif
		}

		public static bool operator ==(CircLine a, CircLine b) {
			if (System.Object.ReferenceEquals(a, b))
				return true;

			if (((object)a == null) || ((object)b == null))
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(CircLine a, CircLine b) {
			return !(a == b);
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;

			return this.Equals(obj as CircLine);
		}

		public bool Equals(CircLine circLine) {
			if (circLine == null)
				return false;

			return Accuracy.LengthEquals(a, circLine.a) && b == circLine.b && Accuracy.LengthEquals(c, circLine.c);
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode();
		}

		public virtual void DrawGL(Color4 color) {
			GL.Begin(BeginMode.LineLoop);
			GL.Color4(color);

			IList<Complex > points = this.Polyline;
			for (int i = 0; i < points.Count; i++)
				GL.Vertex3(points[i].Vector3d);

			GL.End();
		}

		public abstract CircLine Translate(Complex translation);

		public abstract CircLine Scale(Complex scale);

		public abstract Interval MinorInterval(double param0, double param1);

		public abstract Evaluation Project(Complex c);

		public abstract bool IsNormalTo(CircLine circLine);

		public abstract CircLine Conjugate { get; }

		public abstract CircLine Inverse { get; }

		public abstract CircLine Normalized { get; }

		public abstract Mobius AsInversion { get; }

		public abstract Complex[] Polyline { get; }

		public struct Intersection {
			Complex point;
			double paramA, paramB;

			public Intersection(Complex point, double paramA, double paramB) {
				this.point = point;
				this.paramA = paramA;
				this.paramB = paramB;
			}

			public Complex Point { get { return point; } }

			public double ParamA { get { return paramA; } }

			public double ParamB { get { return paramB; } }
		}

		public struct Evaluation {
			Complex point;
			double param;

			public Evaluation(Complex point, double param) {
				this.point = point;
				this.param = param;
			}

			public Complex Point { get { return point; } }

			public double Param { get { return param; } }
		}

		public bool IsPointOnLeft(Complex p) {
			return a * p.ModulusSquared + (b.Conjugate * p + b * p.Conjugate).Re + c + Accuracy.LinearTolerance > 0;
		}
		
		public bool ContainsPoint(Complex p) {
			return Accuracy.LengthIsZero(a * p.ModulusSquared + (b.Conjugate * p + b * p.Conjugate).Re + c);
		}
		
		public bool ArePointsOnSameSide(Complex p1, Complex p2) {
			if (p1 == p2)
				return true;	
			
//			if (ContainsPoint(p1) || ContainsPoint(p2))
//				return true;
			
			return IsPointOnLeft(p1) ^ IsPointOnLeft(p2);
		}
		
	}

	public class Circle : CircLine {
		public Circle(double a, Complex b, double c)
			: base(1, b / a, c / a) {
			Debug.Assert(RadiusSquared > -Accuracy.LinearToleranceSquared);
		}

		public Circle(Complex b, double c)
			: base(1, b, c) {
			Debug.Assert(RadiusSquared > -Accuracy.LinearToleranceSquared);
		}

		public static Circle Create(Complex center, double radius) {
			return new Circle(-center, center.ModulusSquared - radius * radius);
		}

		public static Circle CreateFromRadiusSquared(Complex center, double radiusSquared) {
			return new Circle(-center, center.ModulusSquared - radiusSquared);
		}

		public void DrawFill(Color4 color) {
			GL.Begin(BeginMode.TriangleFan);
			GL.Color4(color);

			IList<Complex > points = this.Polyline;
			for (int i = 0; i < points.Count; i++)
				GL.Vertex3(points[i].Vector3d);

			GL.Vertex3(points[0].Vector3d);
			GL.End();
		}

		public override void DrawGL(Color4 color) {
#if false
			double clipRadius = .5;
			if (RadiusSquared > 10 ) {
				List<Intersection> intersections = Intersect(Circle.Unit);
				if (intersections.Count != 2)
					return;

				Line.Create(intersections[0].Point, intersections[1].Point).DrawGL(Color4.Yellow);

				new TrimmedCircLine(this, MinorInterval(intersections[0].ParamA, intersections[1].ParamA)).DrawGL(color);
				return;
			}
#endif

			GL.Begin(BeginMode.LineLoop);
			GL.Color4(color);
			IList<Complex > points = Polyline;
			for (int i = 0; i < points.Count; i++)
				GL.Vertex3(points[i].Vector3d);

			GL.End();
		}

		public override List<Intersection> Intersect(CircLine other) {
			List<Intersection > intersections = new List<Intersection>();

			if (other is Circle) {
				Circle otherC = (Circle)other;

				Complex p0 = this.Center;
				Complex p1 = otherC.Center;
				double d = (p1 - p0).Modulus;
				double r0 = this.Radius;
				double r1 = otherC.Radius;

				if (d > (r0 + r1)) // outside
					return null;
				if (d < Math.Abs(r0 - r1))
					return intersections;
				if (d == 0)
					return intersections;

				double a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);
				double h = Math.Sqrt(r0 * r0 - a * a);
				Complex p2 = p0 + a * (p1 - p0) / d;

				Complex intersect;
				intersect = new Complex(
						p2.Re + h * (p1.Im - p0.Im) / d,
						p2.Im - h * (p1.Re - p0.Re) / d
					);

				intersections.Add(new Intersection(
					intersect,
					Math.Atan2(p0.Im - intersect.Im, p0.Re - intersect.Re),
					Math.Atan2(p1.Im - intersect.Im, p1.Re - intersect.Re)
				));

				intersect = new Complex(
					p2.Re - h * (p1.Im - p0.Im) / d,
					p2.Im + h * (p1.Re - p0.Re) / d
				);

				intersections.Add(new Intersection(
					intersect,
					Math.Atan2(p0.Im - intersect.Im, p0.Re - intersect.Re),
					Math.Atan2(p1.Im - intersect.Im, p1.Re - intersect.Re)
				));

				return intersections;
			}

			Line line = (Line)other;

			Complex nearPoint = line.Project(Center).Point - line.Origin;

			double dist = (Center - nearPoint).Modulus;
			if (dist - Radius > 0)
				return null;

			Complex p;

			p = Line.Create(nearPoint, line.Angle).Evaluate(Math.Sqrt(RadiusSquared - dist * dist));
			intersections.Add(new Intersection(
				p,
				line.Angle - Math.Asin(dist / Radius),
				line.Project(p).Param
			));

			p = Line.Create(nearPoint, line.Angle).Evaluate(-Math.Sqrt(RadiusSquared - dist * dist));
			intersections.Add(new Intersection(
				p,
				line.Angle + Math.Asin(dist / Radius) + Math.PI,
				line.Project(p).Param
			));

			return intersections;
		}

		public override Interval MinorInterval(double param0, double param1) {
			if (param0 < 0)
				param0 = 2 * Math.PI + (param0 % (2 * Math.PI));
			if (param1 < 0)
				param1 = 2 * Math.PI + (param1 % (2 * Math.PI));
			param0 %= 2 * Math.PI;
			param1 %= 2 * Math.PI;

			double min = Math.Min(param0, param1);
			double max = Math.Max(param0, param1);

			if (max - min < Math.PI)
				return new Interval(min, max);

			return new Interval(max, min + 2 * Math.PI);
		}

		public override Evaluation Project(Complex p) {
			double nearParam = (p - Center).Argument;
			Complex nearPoint = Evaluate(nearParam);
			return new Evaluation(nearPoint, nearParam);
		}

		public override Complex Evaluate(double t) {
			return Center + Complex.CreatePolar(Radius, t);
		}

		public override CircLine Translate(Complex translation) {
			return CreateFromRadiusSquared(Center + translation, RadiusSquared);
		}

		public override CircLine Scale(Complex scale) {
			return Create(Center * scale, Radius * scale.Modulus);
		}

		public override bool IsNormalTo(CircLine circLine) {
			if (circLine is Line)
				return circLine.IsNormalTo(this);

			List<Intersection > intersections = this.Intersect(circLine);
			if (intersections == null || intersections.Count == 0)
				return false;

			Circle other = (Circle)circLine;
			Complex p = intersections[0].Point;

			return Accuracy.AngularTolerance > 
				Math.Abs((p - this.Center).ModulusSquared + (p - other.Center).ModulusSquared - (this.Center - other.Center).ModulusSquared);
		}
	
		#region properties
		public static Circle Unit { get { return Create(Complex.Zero, 1); } }

		public Complex Center { get { return -b / a; } }

		public double RadiusSquared { get { return Center.ModulusSquared - c / a; } }

		public double Radius { get { return Math.Sqrt(RadiusSquared); } }

		public override CircLine Conjugate { get { return new Circle(a, b.Conjugate, c); } }

		public override CircLine Inverse {
			get {
				if (Accuracy.LengthIsZero((Center.ModulusSquared - RadiusSquared))) {
					return new Line(-b.Conjugate, 1);
				}

				return new Circle(Center.ModulusSquared - RadiusSquared, -Center.Conjugate, 1);
			}
		}

		public override CircLine Normalized { get { return new Circle(b / a, c / a); } }

		public override Mobius AsInversion {
			get {
				return new Mobius(Center, RadiusSquared - Complex.One * Center.ModulusSquared, Complex.One, -Center.Conjugate);
			}
		}

		public override Complex[] Polyline {
			get {
				int numPoints = 128;
				Complex[] Points = new Complex[numPoints];

				for (int i = 0; i < numPoints; i++)
					Points[i] = this.Evaluate((double)i / numPoints * 2 * Math.PI);

				return Points;
			}
		}

		#endregion
	}

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
