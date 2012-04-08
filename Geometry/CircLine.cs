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

}
