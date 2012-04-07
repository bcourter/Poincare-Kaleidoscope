using System;
using System.Diagnostics;
using System.Collections.Generic;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using Poincare.Geometry;

namespace Poincare.Application {
	public class MouseControl {
		bool isDragging;
		bool isDraggingAngle;
		Complex mousePos, initialMousePos;

		public PoincareWindow PoincareWindow { get; private set; }
		
		public MouseControl(PoincareWindow poincareWindow) {
			PoincareWindow = poincareWindow;
			
			PoincareWindow.Mouse.ButtonDown += Mouse_ButtonDown;
			PoincareWindow.Mouse.ButtonUp += Mouse_ButtonUp;
		}
			
		private void Mouse_ButtonDown(object sender, MouseButtonEventArgs ea) {
			isDragging = true;
			mousePos = MousePos;
			if (mousePos.ModulusSquared > 0.98)
				isDraggingAngle = true;

			initialMousePos = mousePos;
		}

		private void Mouse_ButtonUp(object sender, MouseButtonEventArgs ea) {
			isDragging = false;
			isDraggingAngle = false;
		}
		
		public void Sample() {
			if (isDragging) {
				mousePos = MousePos;
				if (isDraggingAngle)
					PoincareWindow.AngleOffset = MousePos.Argument - initialMousePos.Argument;
				else {
					if (mousePos.ModulusSquared > 0.98)
						mousePos = Complex.CreatePolar(0.98, mousePos.Argument);

					PoincareWindow.Offset = mousePos - initialMousePos;
				}
			}
			
			initialMousePos = mousePos;
		}
		
		public Complex MousePos {
			get { 
				int width = PoincareWindow.Width;
				int height = PoincareWindow.Height;
					
				return 2 * new Complex(PoincareWindow.Mouse.X, width - PoincareWindow.Mouse.Y) / width - Complex.I * (width - height) / width - new Complex(1, 1);
			}
		}

	}
}