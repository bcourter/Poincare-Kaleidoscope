using System;
using System.Diagnostics;
using System.Collections.Generic;

//using System.Linq;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

using Poincare.Geometry;

namespace Poincare.Application {
	public class PoincareWindow : GameWindow {
		static GraphicsMode graphicsMode;
		const int windowDefaultSize = 800;
		Disc disc;
		Bitmap bitmap;
		Random random = new Random();
		double time = System.DateTime.Now.Ticks * 1E-7;
		double oldTime = System.DateTime.Now.Ticks * 1E-7;
		double startTime = System.DateTime.Now.Ticks * 1E-7;
		int drawCount = 0;
		double resetTime = 0;
		double resetDuration = 60;
		JoystickControl joystickControl = null;
		MouseControl mouseControl = null;
		KeyboardControl keyboardControl = null;
		int p = 5, q = 5 ;
		int imageIndex = 0;

		public Complex Offset { get; set; }

		public double AngleOffset { get; set; }

		public bool IsMoving { get; set; }

		public bool IsRandomizing { get; set; }
		
		public static List<string> ImageFiles{ get; set; }
		
		public double ImageSpeed { get; set; }

		public double ImageOffset { get; set; }

		public bool IsInverting { get; set; }
	
		/// <summary>Creates a window with the specified title.</summary>
		public PoincareWindow()
			: base(windowDefaultSize, windowDefaultSize, graphicsMode, "Poincare'") {
			VSync = VSyncMode.On;
	
			Offset = Complex.Zero;
			AngleOffset = 0;
			
			IsMoving = false;
			IsRandomizing = false;
			
			ImageSpeed = 111;
			ImageOffset = 0;
			IsInverting = false;
		}

		/// <summary>Load resources here.</summary>
		/// <param name="e">Not used.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);		
			
			keyboardControl = new KeyboardControl(this);
			mouseControl = new MouseControl(this);
			
			if (Joysticks.Count == 1)
				joystickControl = new JoystickControl(Joysticks[0], this);
			
			Reset(P, Q, imageIndex);
		}

		public void MakeLimitRotation() {
			AngleOffset = Offset.Modulus * 2;
		}

		public void Randomize() {
			P = random.Next(5) + 3;
			Q = random.Next(10 - P);
			imageIndex = random.Next(ImageFiles.Count);
			
			Reset();
		}
		
		public void ToggleFullscreen() {
			if (WindowState == WindowState.Fullscreen) {
				WindowState = WindowState.Normal;
				Width = windowDefaultSize;
				Height = windowDefaultSize;
				OnResize(null);
				return;
			}
			
			WindowState = WindowState.Fullscreen;
		}

		protected override void OnUnload(EventArgs e) {
			disc.Dispose();
		}

		/// <summary>
		/// Called when your window is resized. Set your viewport here. It is also
		/// a good place to set up your projection matrix (which probably changes
		/// along when the aspect ratio of your window).
		/// </summary>
		/// <param name="e">Not used.</param>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

			//jetblbc	Height = Width;
			float aspect = (float)Height / Width;
			Matrix4 projection = Matrix4.CreateOrthographic(2, 2 * aspect, -1f, 1f);
			projection *= Matrix4.CreateRotationY((float)Math.PI);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref projection);
		}

		/// <summary>
		/// Called when it is time to setup the next frame. Add you game logic here.
		/// </summary>
		/// <param name="e">Contains timing information for framerate independent logic.</param>
		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);
		}

		public void Reset() {
			Reset(P, Q, imageIndex);
		}

		private void Reset(int p, int q, int pictureIndex) {
			if (disc != null)
				disc.Dispose();

			bitmap = new Bitmap(ImageFiles[pictureIndex]);
			disc = new Disc(new FundamentalRegion(p, q), bitmap, IsInverting);
	
			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.PolygonSmooth);
			//		GL.Enable(EnableCap.DepthTest);
			Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);
			
			drawCount = 0;
			startTime = time;
		}

		/// <summary>
		/// Called when it is time to render the next frame. Add your rendering code here.
		/// </summary>
		/// <param name="e">Contains timing information.</param>
		protected override void OnRenderFrame(FrameEventArgs e) {
			GC.Collect();
			//	GC.WaitForPendingFinalizers();
			base.OnRenderFrame(e);
			
			oldTime = time;
			time = System.DateTime.Now.Ticks * 1E-7;
			Console.WriteLine(string.Format("Frame:{0:F5} Avg:{1:F5}", time - oldTime, (time - startTime) / ++drawCount));
			
			if (IsRandomizing && time - resetTime > resetDuration) {
				Randomize();
				resetTime = time;
			}
			
			if (joystickControl != null)
				joystickControl.Sample();
			
			mouseControl.Sample();

			Mobius movement =
				Mobius.CreateRotation(AngleOffset) *
				Mobius.CreateDiscTranslation(Complex.Zero, Offset);
			
			if (IsMoving)
				movement = Mobius.CreateDiscTranslation(Complex.Zero, Complex.CreatePolar(0.01 * Math.Sin(2 * Math.PI * time / 50), 2 * Math.PI * time / 30)) * movement;
			
			ImageOffset += ImageSpeed;
			
			GL.ClearColor(Color4.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			disc.DrawGL(movement, ImageOffset);
			
			//	PolarDemo(time);
			//	LoopDemo(mousePos.Re);

			SwapBuffers();
		}

		void LoopDemo(double t) {
			int pointCount = 100;
			Complex[] gamma = new Complex[100];
			Complex[] gammaF = new Complex[100];

			List<Complex > zeros = new List<Complex>();
			zeros.Add(new Complex(0.1, 0.1));
			zeros.Add(new Complex(-0.3, 0.3));
			zeros.Add(new Complex(-0.5, -0.5));

			for (int i = 0; i < pointCount; i++) {
				gamma[i] = Complex.CreatePolar(t, (double)i / pointCount * Math.PI * 2);
				gammaF[i] = Complex.One;
				
				foreach (Complex zero in zeros)
					gammaF[i] *= gamma[i] - zero;
			}

			Circle.Create(Complex.Zero, 0.02).DrawGL(Color4.Gray);
			foreach (Complex zero in zeros)
				Circle.Create(zero, 0.02).DrawGL(Color4.White);


			GL.Begin(BeginMode.LineLoop);
			GL.Color4(Color.Blue);
			for (int i = 0; i < pointCount; i++)
				GL.Vertex3(gamma[i].Vector3d);
			GL.End();

			GL.Begin(BeginMode.LineLoop);
			GL.Color4(Color.Red);
			for (int i = 0; i < pointCount; i++)
				GL.Vertex3(gammaF[i].Vector3d);
			GL.End();

			Complex aa = Complex.One * 3;
			Complex bb = -(2 * zeros[0] + 2 * zeros[1] + 2 * zeros[2]);
			Complex cc = zeros[0] * zeros[1] + zeros[1] * zeros[2] + zeros[2] * zeros[0];

			Complex r1 = (-bb + (bb * bb - 4 * aa * cc).Sqrt) / 2 / aa;
			Complex r2 = (-bb - (bb * bb - 4 * aa * cc).Sqrt) / 2 / aa;
			Circle.Create(r1, 0.01).DrawGL(Color4.Green);

			Circle.Create(r2, 0.01).DrawGL(Color4.Green);
		}

		void PolarDemo(double time) {
			//	CircLine inversionCircle = Circle.Create(new Complex(-1, -0.5), 1);
			CircLine inversionCircle = Circle.Create(new Complex(0, 0), 1);
			Mobius inversion = inversionCircle.AsInversion;
			inversionCircle.DrawGL(Color4.Gray);

			int circleCount = 6;
			Complex center = Complex.CreatePolar(0.3, time);
			for (int i = 0; i < circleCount; i++) {
				CircLine radialLine = Line.Create(center, Math.PI * i / circleCount);
				radialLine.DrawGL(Color4.Green);
				CircLine radialLineImage = inversion * radialLine.Conjugate;
				radialLineImage.DrawGL(Color4.GreenYellow);
				CircLine radialLineBack = inversion * radialLineImage.Conjugate;
				radialLineBack.DrawGL(Color4.Aqua);

				//CircLine circumferencialLine = Circle.Create(center, (double) (i + 1) / circleCount);
				//circumferencialLine.DrawGL(Color4.DarkRed);
				//CircLine circumferencialLineImage = inversion * circumferencialLine.Conjugate;
				//circumferencialLineImage.DrawGL(Color4.Red);

			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			ImageFiles = new List<string>();
			string imageFileDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Resources");
			ImageFiles.AddRange(GetImages(imageFileDir));

			//		graphicsMode = new GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, graphicsModeSamples);
			graphicsMode = new GraphicsMode();
			
			if (args.Length > 0) {
				imageFileDir = args[0];
				if (imageFileDir != string.Empty)
					ImageFiles.AddRange(GetImages(imageFileDir));
			}

			// The 'using' idiom guarantees proper resource cleanup.
			// We request 30 UpdateFrame events per second, and unlimited
			// RenderFrame events (as fast as the computer can handle).
			using (PoincareWindow game = new PoincareWindow()) {
				game.Run(30.0);
			}
		}

		private static List<string> GetImages(string directory) {
			var files = new List<string>();
			files.AddRange(System.IO.Directory.GetFiles(directory, "*.jpg"));
			files.AddRange(System.IO.Directory.GetFiles(directory, "*.png"));
			files.AddRange(System.IO.Directory.GetFiles(directory, "*.bmp"));
			return files;
		}
	
		public int P { 
			get { return p;	}
			set {
				p = value;
				while ((p - 2) * (q - 2) <= 4)
					p++;
			}
		}
			
		public int Q { 
			get { return q;	}
			set {
				q = value;
				while ((p - 2) * (q - 2) <= 4)
					q++;
				
				//	q = Math.Max(q, 4);
			}
		}

		public int ImageIndex { 
			get { return imageIndex;	}
			set {
				imageIndex = value;
				while (imageIndex < 0)
					imageIndex += ImageFiles.Count;
				
				imageIndex %= ImageFiles.Count;
			}
		}
	}
}