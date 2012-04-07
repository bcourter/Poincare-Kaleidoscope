using System;
using System.Diagnostics;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {	
	public struct TriangleMesh {
		Complex[] points;
		Complex[] pointTexCoords;
		Complex center;
		Complex centerTexCoord;
		
		public TriangleMesh(Complex[] points) {
			this.points = points;
			center = Complex.Zero;
			foreach (Complex point in points)
				center += point;
			
			center /= points.Length;
			pointTexCoords = points;
			centerTexCoord = center;
		}
		
		public TriangleMesh(Complex[] points, Complex center) {
			this.points = points;
			this.center = center;
			pointTexCoords = points;
			centerTexCoord = center;
		}
		
		public TriangleMesh(Complex[] points, Complex center, Complex[] pointTexCoords, Complex centerTexCoord) {
			this.points = points;
			this.center = center;
			this.pointTexCoords = pointTexCoords;
			this.centerTexCoord = centerTexCoord;
		}
		
		public static TriangleMesh operator *(Mobius m, TriangleMesh polygon) {
			Complex[] points = polygon.points;
			Complex[] transformedPoints = new Complex[points.Length];
			for (int i = 0; i < points.Length; i++)
				transformedPoints[i] = m * points[i];
				
			return new TriangleMesh(transformedPoints, m * polygon.center, polygon.pointTexCoords, polygon.centerTexCoord); 
		}
		
		public void DrawGL(Color4 color, int texture) {
			double t = 1E-7d * System.DateTime.Now.Ticks;
			Complex texOffset = new Complex(0.5 + 0.5 * Math.Cos(t / 20), 0.5 + 0.5 * Math.Sin(3 * t / 50));
			GL.Color4(color);
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
	
#if false
			GL.Begin(BeginMode.TriangleFan);
			GL.Color4(new Color4(1f, 1f, 1f, 0.3f));
			GL.TexCoord3((centerTexCoord + texOffset).Vector3d);
			GL.Vertex3(center.Vector3d);
			for (int i = 0; i < points.Length; i++) {	
				//					GL.Color4(Extensions.Color4FromHSV((float)(2 * Math.PI * i / points.Count), 1f, 1f, 1f));
				GL.TexCoord3((pointTexCoords[i] + texOffset).Vector3d);
				GL.Vertex3(points[i].Vector3d);  
			}
			GL.End();	
#else
			
			/*         08
			 *       09  07
			 *     10  14  06
			 *   11  12  13  05
			 * 00  01  02  03  04
			 * 
			 *     04      
			 *   05  03      02
			 * 00  01  02  00  01
			 */
			
			const int smallCount = 3;
			const int mediumCount = 6;
			const int largeCount = 15;
			const double mediumRadius = 0.5;
			const double smallRadius = 0.8;
		
#if false
			if (points.Length == largeCount) {
				if (center.ModulusSquared > mediumRadius) {
				//	if (!IsClockwise(points[0], points[2], points[4])) {
						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(0, texOffset);
						GLVertex(1, texOffset);
						GLVertex(2, texOffset);
						GL.End();	

						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(2, texOffset);
						GLVertex(3, texOffset);
						GLVertex(4, texOffset);
						GL.End();	
				//	}
					
				//	if (!IsClockwise(points[4], points[6], points[8])) {
						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(4, texOffset);
						GLVertex(5, texOffset);
						GLVertex(6, texOffset);
						GL.End();	

						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(6, texOffset);
						GLVertex(7, texOffset);
						GLVertex(8, texOffset);
						GL.End();	
				//	}
					
				//	if (!IsClockwise(points[8], points[10], points[0])) {
						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(8, texOffset);
						GLVertex(9, texOffset);
						GLVertex(10, texOffset);
						GL.End();	

						GL.Begin(BeginMode.TriangleStrip);
						GLVertex(10, texOffset);
						GLVertex(11, texOffset);
						GLVertex(0, texOffset);
						GL.End();	
				//	}
					
					points = new Complex[] {
						points[0],
						points[2],
						points[4],
						points[6],
						points[8],
						points[10]
					};

					pointTexCoords = new Complex[] {
						pointTexCoords[0],
						pointTexCoords[2],
						pointTexCoords[4],
						pointTexCoords[6],
						pointTexCoords[8],
						pointTexCoords[10],
					};
				}
			}
			
			if (points.Length == mediumCount) {
				if (center.ModulusSquared > smallRadius) {
					GL.Begin(BeginMode.TriangleStrip);
					GLVertex(0, texOffset);
					GLVertex(1, texOffset);
					GLVertex(2, texOffset);
					GL.End();	

					GL.Begin(BeginMode.TriangleStrip);
					GLVertex(2, texOffset);
					GLVertex(3, texOffset);
					GLVertex(4, texOffset);
					GL.End();	

					GL.Begin(BeginMode.TriangleStrip);
					GLVertex(4, texOffset);
					GLVertex(5, texOffset);
					GLVertex(0, texOffset);
					GL.End();	

					points = new Complex[] {
						points[0],
						points[2],
						points[4]
					};
							
					pointTexCoords = new Complex[] {
						pointTexCoords[0],
						pointTexCoords[2],
						pointTexCoords[4]
					};
				}
			}
			
			if (points.Length == smallCount) {
				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(0, texOffset);
				GLVertex(1, texOffset);
				GLVertex(2, texOffset);
				GL.End();	
				
				return;
			}
			
			if (points.Length == mediumCount) {
				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(0, texOffset);
				GLVertex(5, texOffset);
				GLVertex(1, texOffset);
				GLVertex(3, texOffset);
				GLVertex(2, texOffset);
				GL.End();	
			
				GL.Begin(BeginMode.TriangleStrip);
				GLVertex(5, texOffset);
				GLVertex(4, texOffset);
				GLVertex(3, texOffset);
				GL.End();	
	
				return;
			}
#endif
			Debug.Assert(points.Length == largeCount);
			
			GL.Begin(BeginMode.TriangleStrip);
			GLVertex(0, texOffset);
			GLVertex(11, texOffset);
			GLVertex(1, texOffset);
			GLVertex(12, texOffset);
			GLVertex(2, texOffset);
			GLVertex(13, texOffset);
			GLVertex(3, texOffset);
			GLVertex(5, texOffset);
			GLVertex(4, texOffset);
			GL.End();	

			GL.Begin(BeginMode.TriangleStrip);
			GLVertex(11, texOffset);
			GLVertex(10, texOffset);
			GLVertex(12, texOffset);
			GLVertex(14, texOffset);
			GLVertex(13, texOffset);
			GLVertex(6, texOffset);
			GLVertex(5, texOffset);
			GL.End();	
			
			GL.Begin(BeginMode.TriangleStrip);
			GLVertex(10, texOffset);
			GLVertex(9, texOffset);
			GLVertex(14, texOffset);
			GLVertex(7, texOffset);
			GLVertex(6, texOffset);
			GL.End();	
			
			GL.Begin(BeginMode.TriangleStrip);
			GLVertex(9, texOffset);
			GLVertex(8, texOffset);
			GLVertex(7, texOffset);
			GL.End();	
#endif
			
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Blend);
			
		}
		
		private bool IsClockwise(Complex p0, Complex p1, Complex p2) {
			double angle1 = Math.Atan2((p2.Im - p0.Im), (p2.Re - p0.Re));
			double angle2 = Math.Atan2((p1.Im - p0.Im), (p1.Re - p0.Re));
			
			return angle1 - angle2 > 0;
		}
		
		private Complex IntersectOrCenter(Complex p0, Complex p1, Complex p2, Complex p3, Complex p4) {
			double angle1 = Math.Atan2((p1.Im - p0.Im), (p1.Re - p0.Re));
			double angle2 = Math.Atan2((p4.Im - p3.Im), (p4.Re - p3.Re));
			
			if (Accuracy.AngleIsZero((angle1 - angle2) % Math.PI))
				return p2;
				
			Complex p = Line.Create(p0, p1).Intersect(Line.Create(p4, p3))[0].Point;
			return p;
		}
		
		private void GLVertex(int i, Complex texOffset) {
			GL.TexCoord3((pointTexCoords[i] + texOffset).Vector3d);
			GL.Vertex3(points[i].Vector3d);  
		}
		
		public Complex[] Points { get { return points; } }
		
		public TriangleMesh Conjugate {
			get {
				Complex[] conjugatePoints = new Complex[points.Length];
				for (int i = 0; i < points.Length; i++)
					conjugatePoints[i] = points[i].Conjugate;
				
				return new TriangleMesh(conjugatePoints, center.Conjugate, pointTexCoords, centerTexCoord); 
			}
		}
			
	}
}

