using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Poincare.Geometry;

namespace Poincare.PoincareDisc {
	public class Edge {
		Face face;
		CircLine circLine;
		Complex start, end;

		public Face AdjacentFace { get; private set; }

		public Edge(Face face, CircLine circLine, Complex start, Complex end) {
			this.face = face;
			this.circLine = circLine;
			this.start = start;
			this.end = end;
		}

		public static Edge operator *(Mobius m, Edge edge) {
			return new Edge(edge.face, m * edge.circLine, m * edge.start, m * edge.end);
		}

		public Edge Conjugate {
			get {
				return new Edge(
					face,
					circLine.Conjugate,
					end.Conjugate,
					start.Conjugate
				);
			}
		}

		public void DrawGL(Color4 color) {
			TrimmedCircLine.DrawGL(color);
		}

		public Face Face { get { return face; } }

		public CircLine CircLine { get { return circLine; } }

		public Complex Start { get { return start; } }

		public Complex End { get { return end; } }

		public bool IsConvex {
			get {
				Circle circle = circLine as Circle;
				if (circle == null)
					return false;

				double a1 = (end - start).Argument;
				double a2 = (circle.Center - start).Argument;
				return (a1 - a2 + 4 * Math.PI) % (2 * Math.PI) < Math.PI;
			}
		}

		public TrimmedCircLine TrimmedCircLine {
			get {
				return new TrimmedCircLine(circLine, start, end);
			}
		}

	}

}

