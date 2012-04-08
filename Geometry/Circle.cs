using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
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

}
