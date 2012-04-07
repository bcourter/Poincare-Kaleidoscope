using System;
using System.Collections.Generic;

namespace Poincare.Geometry {	
	public class Interval {
		double start, end;
		
		public Interval(double start, double end) {
			this.start = start;
			this.end = end;
		}
		
		public double Start { get { return start; } }

		public double End { get { return end; } }
		
		public double Span { get { return end - start; } }
	}
}

