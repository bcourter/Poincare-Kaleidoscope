using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
	public class Face {
		FundamentalRegion region;
		Complex[] mesh;
		protected int p;
		Edge[] edges;
		bool isFlipped;
		int meshInternalCount = 3;
		Complex center;
		Complex[] vertices, edgeCenters;
		Complex[][] halfEdgePoints, spinePoints, dualEdgePoints, interiorPoints;

		/*
		 *                                 ----/  vertex
		 *                            ----    /
		 *            spine       ----       /  
		 *                    ----          |  halfEdge
		 *                ----   interior   |
		 *            ----                  |
		 * center --------------------------- edgeCenter
		 *                  dualEdge
		 *                  
		 *         08
		 *       09  07
		 *     10  14  06
		 *   11  12  13  05
		 * 00  01  02  03  04
		 * 
		 */

		public Face(FundamentalRegion region) {
			this.region = region;
			this.center = Complex.Zero;
			p = region.P;
			isFlipped = false;

			Mobius increment = Mobius.CreateRotation(2 * Math.PI / p);
			Complex midvertex = region.P1;
			Edge edge = new Edge(
				this,
				region.C,
				midvertex,
				increment.Inverse * midvertex
			);

			mesh = region.Mesh.Points;
			edges = new Edge[p];

			vertices = new Complex[p];
			edgeCenters = new Complex[p];

			halfEdgePoints = new Complex[2 * p][];
			spinePoints = new Complex[p][];
			dualEdgePoints = new Complex[p][];
			interiorPoints = new Complex[2 * p][];

			Mobius rotation = Mobius.Identity;
			for (int i = 0; i < p; i++) {
				edges[i] = rotation * edge;

				dualEdgePoints[i] = new Complex[meshInternalCount];
				dualEdgePoints[i][0] = rotation * mesh[1];
				dualEdgePoints[i][1] = rotation * mesh[2];
				dualEdgePoints[i][2] = rotation * mesh[3];

				edgeCenters[i] = rotation * mesh[4];

				halfEdgePoints[i] = new Complex[meshInternalCount];
				halfEdgePoints[i][0] = rotation * mesh[5];
				halfEdgePoints[i][1] = rotation * mesh[6];
				halfEdgePoints[i][2] = rotation * mesh[7];

				halfEdgePoints[i + p] = new Complex[meshInternalCount];
				halfEdgePoints[i + p][0] = rotation * mesh[5].Conjugate;
				halfEdgePoints[i + p][1] = rotation * mesh[6].Conjugate;
				halfEdgePoints[i + p][2] = rotation * mesh[7].Conjugate;

				vertices[i] = rotation * mesh[8];

				spinePoints[i] = new Complex[meshInternalCount];
				spinePoints[i][0] = rotation * mesh[9];
				spinePoints[i][1] = rotation * mesh[10];
				spinePoints[i][2] = rotation * mesh[11];

				interiorPoints[i] = new Complex[meshInternalCount];
				interiorPoints[i][0] = rotation * mesh[12];
				interiorPoints[i][1] = rotation * mesh[13];
				interiorPoints[i][2] = rotation * mesh[14];

				interiorPoints[i + p] = new Complex[meshInternalCount];
				interiorPoints[i + p][0] = rotation * mesh[12].Conjugate;
				interiorPoints[i + p][1] = rotation * mesh[13].Conjugate;
				interiorPoints[i + p][2] = rotation * mesh[14].Conjugate;

				rotation = rotation * increment;
			}
		}

		Face(FundamentalRegion region, Edge[] edges, Complex center, Complex[] vertices, Complex[] edgeCenters, Complex[][] halfEdges, Complex[][] spines, Complex[][] dualEdges, Complex[][] interiors, bool isFlipped) {
			this.region = region;
			this.edges = edges;
			this.center = center;
			this.vertices = vertices;
			this.edgeCenters = edgeCenters;
			this.halfEdgePoints = halfEdges;
			this.spinePoints = spines;
			this.dualEdgePoints = dualEdges;
			this.interiorPoints = interiors;
			this.isFlipped = isFlipped;
			p = region.P;
			mesh = region.Mesh.Points;
		}

		public static Face operator *(Mobius m, Face face) {
			int p = face.p;
			Edge[] edges = new Edge[p];

			Complex center = m * face.center;
			Complex[] vertices = m * face.vertices;
			Complex[] edgeCenters = m * face.edgeCenters;

			Complex[] [] halfEdges = new Complex[2 * p][];
			Complex[] [] spines = new Complex[p][];
			Complex[] [] dualEdges = new Complex[p][];
			Complex[] [] interiors = new Complex[2 * p][];

#if false
			System.Threading.Tasks.Parallel.For(0, p, i => {
				newEdges[i] = m * face.edges[i];
				newPolygons[i] = m * face.polygons[i];
				newPolygons[i + p] = m * face.polygons[i + p];
			});
#else
			for (int i = 0; i < p; i++) {
				edges[i] = m * face.edges[i];
				halfEdges[i] = m * face.halfEdgePoints[i];
				halfEdges[i + p] = m * face.halfEdgePoints[i + p];
				spines[i] = m * face.spinePoints[i];
				dualEdges[i] = m * face.dualEdgePoints[i];
				interiors[i] = m * face.interiorPoints[i];
				interiors[i + p] = m * face.interiorPoints[i + p];
			}
#endif

			return new Face(face.region, edges, center, vertices, edgeCenters, halfEdges, spines, dualEdges, interiors, face.isFlipped);
		}

		public Face Conjugate {
			get {
				Edge[] edges = new Edge[p];

				Complex center = this.center.Conjugate;

				Complex[] vertices = new Complex[p];
				Complex[] edgeCenters = new Complex[p];

				Complex[] [] halfEdges = new Complex[2 * p][];
				Complex[] [] spines = new Complex[p][];
				Complex[] [] dualEdges = new Complex[p][];
				Complex[] [] interiors = new Complex[2 * p][];

				for (int i = 0; i < p; i++) {
					//	int ii = p - i - 1;
					vertices[i] = this.vertices[i].Conjugate;
					edgeCenters[i] = this.edgeCenters[i].Conjugate;

					edges[i] = this.edges[i].Conjugate;
					halfEdges[i] = this.halfEdgePoints[i].Conjugate();
					halfEdges[i + p] = this.halfEdgePoints[i + p].Conjugate();
					spines[i] = this.spinePoints[i].Conjugate();
					dualEdges[i] = this.dualEdgePoints[i].Conjugate();
					interiors[i] = this.interiorPoints[i].Conjugate();
					interiors[i + p] = this.interiorPoints[i + p].Conjugate();
				}

				return new Face(region, edges, center, vertices, edgeCenters, halfEdges, spines, dualEdges, interiors, !isFlipped);
			}
		}

		public void DrawGL(Color4 color, int texture, int textureBack, bool isInverting, bool isInverted, Complex texOffset) {
			GL.Color4(color);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Enable(EnableCap.ColorLogicOp);
			GL.LogicOp(LogicOp.Copy);
			
			GL.BindTexture(TextureTarget.Texture2D, texture);
			//		AverageColor = Color4.Black;
			
			for (int i = 0; i < p; i++) {

				if (isInverting) {
					if (isInverted ^ isFlipped) 
						GL.LogicOp(LogicOp.CopyInverted);
					else
						GL.LogicOp(LogicOp.Copy);
				}

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(center, 0, texOffset);
				GLVertex(spinePoints[i][2], 11, texOffset);
				GLVertex(dualEdgePoints[i][0], 1, texOffset);
				GLVertex(interiorPoints[i][0], 12, texOffset);
				GLVertex(dualEdgePoints[i][1], 2, texOffset);
				GLVertex(interiorPoints[i][1], 13, texOffset);
				GLVertex(dualEdgePoints[i][2], 3, texOffset);
				GLVertex(halfEdgePoints[i][0], 5, texOffset);
				GLVertex(edgeCenters[i], 4, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[i][2], 11, texOffset);
				GLVertex(spinePoints[i][1], 10, texOffset);
				GLVertex(interiorPoints[i][0], 12, texOffset);
				GLVertex(interiorPoints[i][2], 14, texOffset);
				GLVertex(interiorPoints[i][1], 13, texOffset);
				GLVertex(halfEdgePoints[i][1], 6, texOffset);
				GLVertex(halfEdgePoints[i][0], 5, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[i][1], 10, texOffset);
				GLVertex(spinePoints[i][0], 9, texOffset);
				GLVertex(interiorPoints[i][2], 14, texOffset);
				GLVertex(halfEdgePoints[i][2], 7, texOffset);
				GLVertex(halfEdgePoints[i][1], 6, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[i][0], 9, texOffset);
				GLVertex(vertices[i], 8, texOffset);
				GLVertex(halfEdgePoints[i][2], 7, texOffset);
				GL.End();

				if (isInverting) {
					if (isInverted ^ isFlipped) 
						GL.LogicOp(LogicOp.Copy);
					else
						GL.LogicOp(LogicOp.CopyInverted);
				}

				int ii = (i + p - 1) % p;
				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(center, 0, texOffset);
				GLVertex(spinePoints[ii][2], 11, texOffset);
				GLVertex(dualEdgePoints[i][0], 1, texOffset);
				GLVertex(interiorPoints[i + p][0], 12, texOffset);
				GLVertex(dualEdgePoints[i][1], 2, texOffset);
				GLVertex(interiorPoints[i + p][1], 13, texOffset);
				GLVertex(dualEdgePoints[i][2], 3, texOffset);
				GLVertex(halfEdgePoints[i + p][0], 5, texOffset);
				GLVertex(edgeCenters[i], 4, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[ii][2], 11, texOffset);
				GLVertex(spinePoints[ii][1], 10, texOffset);
				GLVertex(interiorPoints[i + p][0], 12, texOffset);
				GLVertex(interiorPoints[i + p][2], 14, texOffset);
				GLVertex(interiorPoints[i + p][1], 13, texOffset);
				GLVertex(halfEdgePoints[i + p][1], 6, texOffset);
				GLVertex(halfEdgePoints[i + p][0], 5, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[ii][1], 10, texOffset);
				GLVertex(spinePoints[ii][0], 9, texOffset);
				GLVertex(interiorPoints[i + p][2], 14, texOffset);
				GLVertex(halfEdgePoints[i + p][2], 7, texOffset);
				GLVertex(halfEdgePoints[i + p][1], 6, texOffset);
				GL.End();

				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(spinePoints[ii][0], 9, texOffset);
				GLVertex(vertices[ii], 8, texOffset);
				GLVertex(halfEdgePoints[i + p][2], 7, texOffset);
				GL.End();
			}

			GL.Disable(EnableCap.Texture2D);
			
//			GL.LogicOp(LogicOp.Invert);
//			foreach (Edge edge in edges) 
//				edge.DrawGL(color);
			
			GL.Disable(EnableCap.ColorLogicOp);
			GL.Disable(EnableCap.Blend);
			
		}

		private void GLVertex(Complex p, int i, Complex texOffset) {
			GL.TexCoord3((mesh[i] + texOffset).Vector3d);
			GL.Vertex3(p.Vector3d);
		}
		
		public Complex Center { get { return center; } }

		public Edge[] Edges { get { return edges; } }
		
		public Complex[] Vertices { get { return vertices; } }

		public bool IsFlipped { get { return isFlipped; } }
	}
	
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

