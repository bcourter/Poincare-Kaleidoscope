using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {	
	// TBD currently not efficient
	public class TrimmedCircLine {
		CircLine circLine;
		Interval bounds;
		
		public TrimmedCircLine(CircLine circLine, Interval bounds) {
			this.circLine = circLine;
			this.bounds = bounds;
		}
		
		public TrimmedCircLine(CircLine circLine, Complex start, Complex end) {
			this.circLine = circLine;
			this.bounds = circLine.MinorInterval(CircLine.Project(start).Param, CircLine.Project(end).Param);
		}
		
		public TrimmedCircLine(Complex start, Complex end) {
			this.circLine = Line.Create(start, end);
			this.bounds = circLine.MinorInterval(CircLine.Project(start).Param, CircLine.Project(end).Param);
		}
		
		public void DrawGL(Color4 color) {
			if (Accuracy.LengthIsZero(bounds.Span))
				return;
			
			GL.Begin(BeginMode.LineStrip);
			GL.Color4(color);
			
			IList<Complex> points = Polygon;
			for (int i = 0; i < points.Count; i++) 				
				GL.Vertex3(points[i].Vector3d);  
			
			GL.End();
		}
	
		public CircLine CircLine { get { return circLine; } }

		public Interval Bounds { get { return bounds; } }
		
		public IList<Complex> Polygon {
			get {
				if (Accuracy.LengthIsZero(bounds.Span))
					throw new InvalidOperationException("Span is zero");
					
				var Points = new List<Complex>(); 
				
				int numPoints = 31;
				if (circLine is Line)
					numPoints = 1;
				
				for (int i = 0; i <= numPoints; i++) 				
					Points.Add(circLine.Evaluate(bounds.Start + bounds.Span * (double)i / numPoints));  
				
				return Points;
			}
		}
		
	}
}

