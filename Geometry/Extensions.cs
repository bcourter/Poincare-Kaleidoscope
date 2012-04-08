using System;

namespace Poincare.Geometry {
	public static class Extensions {
		public static Complex[] Conjugate(this Complex[] z) {
			Complex[] result = new Complex[z.Length];
			for (int i = 0; i < z.Length; i++)
				result[i] = z[i].Conjugate;

			return result;
		}
	}
}

