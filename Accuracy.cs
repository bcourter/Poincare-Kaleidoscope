using System;

namespace Poincare.Geometry {
	public static class Accuracy {
		public const double LinearTolerance = 1E-9;
		public const double AngularTolerance = 1E-3;
		public const double LinearToleranceSquared = LinearTolerance * LinearTolerance;
		public const double MaxLength = 100;
		
		public static bool LengthEquals(double a, double b) {
			return Math.Abs(a - b) < LinearTolerance;
		}
		
		public static bool LengthIsZero(double a) {
			return Math.Abs(a) < LinearTolerance;
		}

		public static bool AngleEquals(double a, double b) {
			return Math.Abs(a - b) < AngularTolerance;
		}
		
		public static bool AngleIsZero(double a) {
			return Math.Abs(a) < AngularTolerance;
		}

	}
}

