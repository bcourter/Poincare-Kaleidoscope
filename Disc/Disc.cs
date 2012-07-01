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
		double drawTimeTarget = 0.04; //0.04;

		Face[] result = new Face[2000];
		int resultLength;
//		ComplexCollection faceCenters = new ComplexCollection(10);
		List<Complex> faceCenters = new List<Complex>(2000);
		Queue<Face > faceQueue = new Queue<Face>(2000);
		
		public Disc(FundamentalRegion region, Bitmap bitmap, bool isInverting) {
			this.fundamentalRegion = region;
			this.isInverting = isInverting;
			
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
			double time = System.DateTime.Now.Ticks * 1E-7;
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

			IList<Face> faces = GetFaces(currentFace);
		//	Console.Write(string.Format("{0:F5} ", System.DateTime.Now.Ticks * 1E-7-time));
			
			Color4 color = new Color4(1f, 1f, 1f, 0.5f);
	//		foreach (Face face in faces) {
		//		face.DrawGL(color, texture, textureInverse, isInverting, isInverted, texOffset);
		
			for(int i = 0; i < resultLength; i++) {
				result[i].DrawGL(color, texture, textureInverse, isInverting, isInverted, texOffset);
			}

			DrawBlendedHorizon(backgroundColor);
			
			drawTime = System.DateTime.Now.Ticks * 1E-7 - time;

#if true // adjust timing
			double diff = drawTimeTarget - drawTime;
			if (drawCount < 16 && Math.Abs(diff) > 0.004) {
				circleLimit *= 1 + diff / 2;
				circleLimit = Math.Min(circleLimit, 0.99);
			}
			
			circleLimitAlphaBand = (circleLimitAlphaBand + (1 - (1 - circleLimit) * 2)) / 2;
#endif
			totalDraw += drawTime;
			drawCount++;

			

//			double sleepTime = drawTimeTarget - (System.DateTime.Now.Ticks * 1E-7 - time) + 0.01;
//			if (sleepTime > 0)
//				System.Threading.Thread.Sleep((int) (sleepTime * 1000));

			//		Console.WriteLine(string.Format("Frame:{0:F5} Avg:{1:F5}", drawTime, totalDraw / drawCount));
		}

		public IList<Face> GetFaces(Face seedFace) {
			faceQueue.Clear();
			resultLength = 0;

			faceQueue.Enqueue(seedFace);
			faceCenters.Clear();
			faceCenters.Add(seedFace.Center);
//			result.Add(seedFace);
			result[resultLength++] = seedFace;

			double time = System.DateTime.Now.Ticks * 1E-7;
			
	//			GC.Collect();
			while (faceQueue.Count > 0) {
				Face face = faceQueue.Dequeue();
				for (int i = 0; i < face.Edges.Length; i++) {
					Edge edge = face.Edges[i];

					if (edge.IsConvex)
						continue;

					Circle circle = edge.CircLine as Circle;
					if (circle != null && circle.RadiusSquared < 1E-4)
						continue;

					Face image = edge.CircLine.AsInversion * face.Conjugate;
					
					if (image.Center.ModulusSquared > circleLimit)
						continue;

					if (faceCenters.Contains(image.Center))
//					if (faceCenters.ContainsValue(image.Center))
						continue;

					faceQueue.Enqueue(image);
					result[resultLength++] = image;
			
					//		result.Add(image);
					faceCenters.Add(image.Center);
	
					if (System.DateTime.Now.Ticks * 1E-7 - time > drawTimeTarget * 1.5)
						break;
//		 		if (resultLength <= 102)
//					Console.Write(string.Format("{0:F5} ", System.DateTime.Now.Ticks * 1E-7 - time));
			}
			
		//			Console.Write(string.Format("{0:F5} ", System.DateTime.Now.Ticks * 1E-7 - time));
				
				
//				if (System.DateTime.Now.Ticks * 1E-7 - time > drawTimeTarget)
//					break;
			}

			return null;
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

