using System;
using System.Diagnostics;
using System.Collections.Generic;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using Poincare.Geometry;

namespace Poincare.Application {
	public class KeyboardControl {
		public PoincareWindow PoincareWindow { get; private set; }
		
		public KeyboardControl(PoincareWindow poincareWindow) {
			PoincareWindow = poincareWindow;
	
			PoincareWindow.Keyboard.KeyDown += Keyboard_KeyDown;
		}

		private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e) {
			bool isShift = PoincareWindow.Keyboard[Key.LShift] || PoincareWindow.Keyboard[Key.RShift];

			switch (e.Key) {
			case Key.P:
				PoincareWindow.P += isShift ? -1 : 1;
				PoincareWindow.Reset();
				break;

			case Key.Q:
				PoincareWindow.Q += isShift ? -1 : 1;
				PoincareWindow.Reset();
				break;

			case Key.N:
				PoincareWindow.ImageIndex = (PoincareWindow.ImageIndex + (isShift ? PoincareWindow.ImageFiles.Count - 1 : 1)) % PoincareWindow.ImageFiles.Count;
				PoincareWindow.Reset();
				break;

			case Key.Z:
				PoincareWindow.Randomize();
				break;

			case Key.R:
				PoincareWindow.Offset = Complex.Zero;
				PoincareWindow.AngleOffset = 0;
				PoincareWindow.Reset();
				break;

			case Key.L:
				PoincareWindow.MakeLimitRotation();
				break;

			case Key.I:
				PoincareWindow.IsInverting = !PoincareWindow.IsInverting;
				PoincareWindow.Reset();
				break;

			case Key.M:
				PoincareWindow.IsMoving = !PoincareWindow.IsMoving;
				break;

			case Key.F:
				PoincareWindow.ToggleFullscreen();
				break;

			case Key.Escape:
				PoincareWindow.Exit();
				break;
			
			case Key.Tab:
				break;
			}
		}
		
	}
}