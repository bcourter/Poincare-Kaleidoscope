using System;

namespace Poincare.Geometry {
	public interface ITransformable {
		ITransformable Transform(Mobius trans);
		ITransformable GetConjugate();
	}
}

