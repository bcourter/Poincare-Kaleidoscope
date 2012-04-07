using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {	
	public struct Complex {
		public double Re;
		public double Im;

		public Complex(double re, double im) {
			Debug.Assert(!double.IsNaN(re), "NaN");
			Debug.Assert(!double.IsNaN(im), "NaN");
			
			this.Re = re;
			this.Im = im;
			
//			Debug.Assert(ModulusSquared < 1 / Accuracy.LinearToleranceSquared);
		}

		public static Complex CreatePolar(double r, double theta) {
			return new Complex(r * Math.Cos(theta), r * Math.Sin(theta));
		}

		public void DrawGL(Color4 color) {
			GL.Begin(BeginMode.Quads);
			GL.Color4(color);
			double halfSize = 0.005;
			GL.Vertex3((this + One * halfSize).Vector3d);
			GL.Vertex3((this + I * halfSize).Vector3d);
			GL.Vertex3((this - One * halfSize).Vector3d);
			GL.Vertex3((this - I * halfSize).Vector3d);
			GL.End();
		}

#region operators
		// complex operators
		public static bool operator ==(Complex a, Complex b) {
			return (a - b).ModulusSquared < Accuracy.LinearToleranceSquared;	
		}
		
		public static bool operator !=(Complex a, Complex b) {
			return !(a == b);
		}
		
		public static Complex operator +(Complex a, Complex b) {
			return new Complex(a.Re + b.Re, a.Im + b.Im);	
		}
		
		public static Complex operator -(Complex a, Complex b) {
			return new Complex(a.Re - b.Re, a.Im - b.Im);	
		}
		
		public static Complex operator -(Complex a) {
			return new Complex(-a.Re, -a.Im);	
		}
		
		public static Complex operator *(Complex a, Complex b) {
			return new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);	
		}
		
		public static Complex operator /(Complex a, Complex b) {
			double denominator = b.Re * b.Re + b.Im * b.Im;
			Debug.Assert(!Accuracy.LengthIsZero(Math.Sqrt(denominator)));
			return new Complex((a.Re * b.Re + a.Im * b.Im) / denominator, (a.Im * b.Re - a.Re * b.Im) / denominator);
		}
		
		// scalars
		public static Complex operator +(double s, Complex c) {
			return new Complex(c.Re + s, c.Im);	
		}
		
		public static Complex operator +(Complex c, double s) {
			return new Complex(c.Re + s, c.Im);	
		}

		public static Complex operator -(double s, Complex c) {
			return new Complex(s - c.Re, c.Im);	
		}
		
		public static Complex operator -(Complex c, double s) {
			return new Complex(c.Re - s, c.Im);	
		}
		
		public static Complex operator *(double s, Complex c) {
			return new Complex(c.Re * s, c.Im * s);	
		}
		
		public static Complex operator *(Complex c, double s) {
			return new Complex(c.Re * s, c.Im * s);	
		}
				
		public static Complex operator /(double s, Complex c) {
			return new Complex(s, 0) / c;	
		}
		
		public static Complex operator /(Complex c, double s) {
			return new Complex(c.Re / s, c.Im / s);	
		}
		
		public static double Dot(Complex a, Complex b) {
			return a.Re * b.Re + a.Im * b.Im;		
		}
		
		public override bool Equals(object obj) {
			return obj is Complex && this == (Complex)obj;
		}
		
		public override int GetHashCode() {
			return Re.GetHashCode() ^ Im.GetHashCode();
		}
		
		public override string ToString() {
			return string.Format("({0}, {1})", Re, Im);
		}
#endregion
		
#region properties
		// statics
		public static Complex Zero { get { return new Complex(0, 0); } }
		
		public static Complex One { get { return new Complex(1, 0); } }
		
		public static Complex I { get { return new Complex(0, 1); } }
		
		// properties
//		public double Re { get { return re; } }
//		
//		public double Im { get { return im; } }
		
		public double Modulus { get { return Math.Sqrt(Re * Re + Im * Im); } }
		
		public double ModulusSquared { get { return Re * Re + Im * Im; } }

		// http://www.flipcode.com/archives/Fast_Approximate_Distance_Functions.shtml
		public double ModulusFastApproximate { get { return Math.Max(Math.Abs(Re), Math.Abs(Im)); } }
		
		public double Argument { get { return Math.Atan2(Im, Re); } }
		
		public Complex Conjugate { get { return new Complex(Re, -Im); } }

		public Complex Normalized {
			get {
				if (Accuracy.LengthIsZero(Modulus))
					throw new ArgumentException("Zero modulus");
				
				return this / Modulus;
			}
		}
		
		public Complex Sqrt {
			get {
				if (Im == 0) {
					if (Re >= 0)
						return Complex.One * Math.Sqrt(Re);
					else
						return Complex.I * Math.Sqrt(-Re);
				}
					
				double modulus = Modulus;
				return new Complex(
					Math.Sqrt((Re + modulus) / 2),
					Math.Sqrt((-Re + modulus) / 2) * Math.Sign(Im)					
				);
			}
		}
		
		public Vector3d Vector3d { get { return new Vector3d(Re, Im, 0); } }
		
#endregion
	}
}

