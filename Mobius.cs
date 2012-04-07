using System;
using System.Diagnostics;
using OpenTK;

namespace Poincare.Geometry {	
	public struct Mobius {
		public readonly Complex A, B, C, D;

		public Mobius(Complex a, Complex b, Complex c, Complex d) {
		//	Debug.Assert(a * d - b * c != Complex.Zero, "Singular Mobius");
			if (a * d - b * c == Complex.Zero) {
				this = Mobius.Identity;
			}
#if true
			this.A = a;
			this.B = b;
			this.C = c;
			this.D = d;
#else	
			Complex k = 1 / (a * d - b * c).Sqrt;
			this.A = a * k;
			this.B = b * k;
			this.C = c * k;
			this.D = d * k;
#endif
		}
		
		// Visual Complex Analysis p320
		public static Mobius CreateDiscAutomorphism(Complex a, double phi) {
			return
				Mobius.CreateRotation(phi) *
				new Mobius(Complex.One, -a, a.Conjugate, -Complex.One);
		}
		
		public static Mobius CreateDiscTranslation(Complex a, Complex b) {
			return
				Mobius.CreateDiscAutomorphism(b, 0) * 
				Mobius.CreateDiscAutomorphism(a, 0).Inverse;
		}
		
		public static Mobius CreateTranslation(Complex tranlsation) {
			return new Mobius(Complex.One, tranlsation, Complex.Zero, Complex.One);	
		}
				
		public static Mobius CreateRotation(double phi) {
			return new Mobius(Complex.CreatePolar(1, phi), Complex.Zero, Complex.Zero, Complex.One);	
		}
				
#region operators
		public static Mobius operator *(Mobius m2, Mobius m1) {
			return 	new Mobius(
				m2.A * m1.A + m2.B * m1.C,
				m2.A * m1.B + m2.B * m1.D,
				m2.C * m1.A + m2.D * m1.C,
				m2.C * m1.B + m2.D * m1.D
			);
		}

		public static Complex operator *(Mobius m, Complex z) {
			return (m.A * z + m.B) / (m.C * z + m.D);
		}
		public static Complex[] operator *(Mobius m, Complex[] z) {
			Complex[] result = new Complex[z.Length];
			for (int i = 0; i < z.Length; i++)
				result[i] = m * z[i];

			return result;
		}
			
		public static Mobius operator *(Mobius m, double s) {
			return 	new Mobius(m.A * s, m.B * s, m.C, m.D);
		}
			
		public static Mobius operator *(double s, Mobius m) {
			return 	m * s;
		}
		
#endregion
		
#region properties
		// statics
		public static Mobius Identity {
			get {
				return new Mobius(Complex.One, Complex.Zero, Complex.Zero, Complex.One);	
			}
		}
				
#if false
		public bool IsSingular { get { return A * D - B * C == Complex.Zero; } }
		
		public Mobius Normalized {
			get {
				if (IsSingular)
					throw new InvalidOperationException("Singular Mobius Transformation");
				
				Complex k = 1 / (A * D - B * C).Sqrt;
				return new Mobius(k * A, k * B, k * C, k * D);
			}
		}
#endif

		public Mobius Inverse {
			get {
				return new Mobius(D, -B, -C, A);
			}
		}

		public Mobius Conjugate {
			get {
				return new Mobius(A.Conjugate, B.Conjugate, C.Conjugate, D.Conjugate);
			}
		}

		public Mobius Transpose {
			get {
				return new Mobius(A, C, B, D);
			}
		}

		public Mobius ConjugateTranspose {
			get {
				return new Mobius(A.Conjugate, C.Conjugate, B.Conjugate, D.Conjugate);
			}
		}

#endregion
	}
}

