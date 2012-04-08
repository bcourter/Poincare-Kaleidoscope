using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Poincare.Geometry;

namespace Poincare.PoincareDisc {
	public class Disc {
		FundamentalRegion fundamentalRegion;
		Face currentFace, initialFace;
		int texture, textureInverse;
		Color4 backgroundColor;
		bool isInverting;
		int drawCount;
		double totalDraw;
		double circleLimit = 0.985;
		double circleLimitAlphaBand = 0;
		double drawTimeTarget = 0.04;
		
		// CircLine[] sectorBoundaries;
		// ComplexCollection sectorCollisionCenters = new ComplexCollection(10);
		
		public Disc(FundamentalRegion region, Bitmap bitmap, bool isInverting) {
			this.fundamentalRegion = region;
			this.isInverting = isInverting;
			//	sectorBoundaries = new CircLine[fundamentalRegion.P];
			
			currentFace = new Face(fundamentalRegion);  // TBD fix extra face bug when centered
			initialFace = currentFace;

			// texture
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			texture = CreateTexture(bitmap);

			int r = 0, g = 0, b = 0;
			int size = bitmap.Width * bitmap.Height;
			int skip = 16;
			size = 0;
			for (int i = 0; i < bitmap.Width; i += skip) {
				for (int j = 0; j < bitmap.Height; j += skip) {
					Color color = bitmap.GetPixel(i, j);
					r += color.R;
					g += color.G;
					b += color.B;
					size++;
				}
			}

			r /= size;
			g /= size;
			b /= size;
			backgroundColor = Color.FromArgb(r, g, b);

			drawCount = 1;
			totalDraw = 0;
		}

		public void Dispose() {
			GL.DeleteTextures(1, ref texture);
			GL.DeleteTextures(1, ref textureInverse);
			Console.WriteLine(string.Format("P: {0}, Q: {1}, Avg :{2:F5}", fundamentalRegion.P, fundamentalRegion.Q, totalDraw / ++drawCount));
		}

		private int CreateTexture(Bitmap bitmap) {
			int texture;
			GL.GenTextures(1, out texture);
			GL.BindTexture(TextureTarget.Texture2D, texture);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
						 ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
						 OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			bitmap.UnlockBits(data);
			return texture;
		}

		public void DrawGL(Mobius movement, double textureTime) {
			double drawTime = 0;
			double beginDraw = System.DateTime.Now.Ticks * 1E-7;
			currentFace = movement * currentFace;
			bool isInverted = false;
			
			Circle.Unit.DrawFill(backgroundColor);

			Edge flipEdge;
			do {
				flipEdge = null;
				Mobius flipTrans = Mobius.Identity;
				foreach (Edge edge in currentFace.Edges) {
					if (edge.IsConvex) {
						flipEdge = edge;
						//		edge.CircLine.DrawGL(Color4.Firebrick);
						break;
					}
				}

				Face image = currentFace;
				if (flipEdge != null) {
					flipTrans = flipTrans * flipEdge.CircLine.AsInversion;
					image = flipTrans * currentFace.Conjugate;

					currentFace = image;
				}

			} while (flipEdge != null);

			// curentFace seems to accumulate rounoff error; create a new one from it's new position
			Mobius toCenter = Mobius.CreateDiscAutomorphism(currentFace.Center, 0);
			double angle = (toCenter * currentFace.Vertices[0]).Argument;
			Mobius seedFaceTrans = Mobius.CreateDiscAutomorphism(currentFace.Center, 0) *
					Mobius.CreateRotation(angle - initialFace.Vertices[0].Argument);
			
			currentFace = seedFaceTrans * initialFace;
			Complex texOffset = new Complex(0.5 + 0.5 * Math.Cos(textureTime / 20), 0.5 + 0.5 * Math.Sin(3 * textureTime / 50));

//			for (int i = 0; i < fundamentalRegion.P; i++) {
//				sectorBoundaries[i] = seedFaceTrans * Mobius.CreateRotation(2 * Math.PI * i / fundamentalRegion.P) * fundamentalRegion.L2;
// //				if (i == 0 || i == fundamentalRegion.P - 1)
//				//	sectorBoundaries[i].DrawGL(Color4.Pink);
//			}

			//		currentFace.DrawGL(new Color4(1f, 1f, 1f, 1f), texture, textureInverse, isInverting, isInverted, texOffset);
			
#if true
			IList<Face>[] faces = new IList<Face>[1];
			faces[0] = GetFaces(currentFace, 0);
			
#elif false
			IList<Face>[] faces = new IList<Face>[fundamentalRegion.P];
			
			System.Threading.Tasks.ParallelOptions options = new System.Threading.Tasks.ParallelOptions();
			options.MaxDegreeOfParallelism = 4;// fundamentalRegion.P; // -1 is for unlimited. 1 is for sequential.

			System.Threading.Tasks.Parallel.For(0, fundamentalRegion.P, options, i =>
				faces[i] = GetFaces(currentFace.Edges[i].CircLine.AsInversion * currentFace.Conjugate, i)
			);
#else
			for (int i = 0; i < fundamentalRegion.P; i++) {
				faces[i] = GetFaces(currentFace.Edges[i].CircLine.AsInversion * currentFace.Conjugate, i);
			}
#endif

			Color4 color = new Color4(1f, 1f, 1f, 0.5f);

			foreach (IList<Face> faceList in faces) {
				foreach (Face face in faceList) 
					face.DrawGL(color, texture, textureInverse, isInverting, isInverted, texOffset);
			}

			DrawBlendedHorizon(backgroundColor);
			// sectorCollisionCenters.Clear();
			
			drawTime = System.DateTime.Now.Ticks * 1E-7 - beginDraw;

#if true // adjust timing
			double diff = drawTimeTarget - drawTime;
			if (drawCount < 16 && Math.Abs(diff) > 0.004) {
				circleLimit *= 1 + diff / 2;
				circleLimit = Math.Min(circleLimit, 0.99);
			}
			
			circleLimitAlphaBand = (circleLimitAlphaBand + (1 - (1 - circleLimit) * 2)) / 2;
#endif
			totalDraw += drawTime;

			//		Console.WriteLine(string.Format("Frame:{0:F5} Avg:{1:F5}", drawTime, totalDraw / drawCount));
			++drawCount;
		}

		public IList<Face> GetFaces(Face seedFace, int sector) {
			List<Face > result = new List<Face>();
			ComplexCollection faceCenters = new ComplexCollection(10);
			Queue<Face > faceQueue = new Queue<Face>();

			faceQueue.Enqueue(seedFace);
			faceCenters.Clear();
			faceCenters.Add(seedFace.Center);
			result.Add(seedFace);

			while (faceQueue.Count > 0) {
				Face face = faceQueue.Dequeue();
				for (int i = 0; i < face.Edges.Length; i++) {
					Edge edge = face.Edges[i];

					if (edge.IsConvex)
						continue;

					Circle circle = face.Edges[i].CircLine as Circle;
					if (circle == null)
						continue;

					if (circle.RadiusSquared < 1E-4)
						continue;

					Face image = edge.CircLine.AsInversion * face.Conjugate;
					
					//	Circle.Create(Complex.Zero, circleLimit).DrawGL(Color4.Red);
					if (image.Center.ModulusSquared > circleLimit)
						continue;
					
					//	if (image.Center.ModulusSquared < face.Center.ModulusSquared)
					//		continue;

					//	if (DiscSector.GetSector(image.Center) != sector)
					//	 	continue;
					
//					int sectorOther = (sector + fundamentalRegion.P - 1) % fundamentalRegion.P;
//					int sectorThis = sector;						
//					if (
//						(sectorBoundaries[sectorThis].ContainsPoint(image.Center)) ||
//						(sectorBoundaries[sectorOther].ContainsPoint(image.Center)) 
//					) {
//					//	image.Center.DrawGL(Color4.Teal);
//						faceQueue.Enqueue(image);
//						faceCenters.Add(image.Center);
//						
//						if (sectorCollisionCenters.ContainsValue(image.Center)) 
//							continue;
//						
//						lock (sectorCollisionCenters)
//							sectorCollisionCenters.Add(image.Center);
//						
//						result.Add(image);
//						continue;
//					}

//					if ((
//						(sectorBoundaries[sectorThis].ArePointsOnSameSide(image.Center, seedFace.Center)) ||
//						(sectorBoundaries[sectorOther].ArePointsOnSameSide(image.Center, seedFace.Center))
//					)) 
//						continue;

					if (faceCenters.ContainsValue(image.Center))
						continue;

					faceQueue.Enqueue(image);
					result.Add(image);
					faceCenters.Add(image.Center);
				}
			}
			
			return result;
		}

		private void DrawBlendedHorizon(Color4 color) {
		
			Complex[] outerPoints = Circle.Create(Complex.Zero, 0.999).Polyline;
			Complex[] middlePoints = Circle.Create(Complex.Zero, (circleLimit + 1) / 2).Polyline;
			Complex[] innerPoints = Circle.Create(Complex.Zero, circleLimitAlphaBand).Polyline;
			Color4 innerColor = new Color4(color.R, color.G, color.B, 0f);
				
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Begin(BeginMode.TriangleStripAdjacency);

			GL.Color4(color);
			int count = outerPoints.Length;
			for (int i = 0; i <= count; i++) {
				GL.Vertex3(outerPoints[i % count].Vector3d);
				GL.Vertex3(outerPoints[(i + 1) % count].Vector3d);

				GL.Vertex3(middlePoints[i % count].Vector3d);
				GL.Vertex3(middlePoints[(i + 1) % count].Vector3d);
			}

			for (int i = 0; i <= count; i++) {
				GL.Color4(color);
				GL.Vertex3(middlePoints[i % count].Vector3d);
				GL.Vertex3(middlePoints[(i + 1) % count].Vector3d);

				GL.Color4(innerColor);
				GL.Vertex3(innerPoints[i % count].Vector3d);
				GL.Vertex3(innerPoints[(i + 1) % count].Vector3d);
			}

			GL.End();
			GL.Disable(EnableCap.Blend);
			
			Circle.Unit.DrawGL(new Color4(1 - color.R, 1 - color.G, 1 - color.B, 1f));
			
		}

	}

}

