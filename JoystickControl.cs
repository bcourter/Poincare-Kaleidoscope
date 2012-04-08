using System;
using System.Diagnostics;
using System.Collections.Generic;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using Poincare.Geometry;

namespace Poincare.Application {
	public class JoystickControl {
		bool isLimit = false;
		bool isBraking = false;
		bool disablePQ = false;
		
		public JoystickDevice Joystick { get; private set; }

		public PoincareWindow PoincareWindow { get; private set; }
		
		public JoystickControl(JoystickDevice joystick, PoincareWindow poincareWindow) {
			Joystick = joystick;
			PoincareWindow = poincareWindow;
			
			Joystick.ButtonDown += Joystick_ButtonDown;
			Joystick.ButtonUp += Joystick_ButtonUp;
		}
				
		private void Joystick_ButtonDown(object sender, JoystickButtonEventArgs e) {
			switch (e.Button) {
			case JoystickButton.Button0: // Trigger
				isLimit = true;
				break;
				
			case JoystickButton.Button1: // Grip
				isBraking = true;
				break;
				
			case JoystickButton.Button2: // Thumb bottom left
				PoincareWindow.ImageIndex--;
				PoincareWindow.Reset();
				break;
			case JoystickButton.Button3: // Thumb bottom right
				PoincareWindow.ImageIndex++;
				PoincareWindow.Reset();
				break;

			case JoystickButton.Button4: // Thumb top left
				PoincareWindow.IsInverting = ! PoincareWindow.IsInverting;
				PoincareWindow.Reset();
				break;
			case JoystickButton.Button5: // Thumb top right
				PoincareWindow.Randomize();
				break;

			case JoystickButton.Button6: // Pad 7
				PoincareWindow.IsMoving = !PoincareWindow.IsMoving;
				break;
				
			case JoystickButton.Button7: // Pad 8
				PoincareWindow.IsRandomizing = !PoincareWindow.IsRandomizing;
				break;
				
			case JoystickButton.Button8: // Pad 9
				PoincareWindow.Offset = Complex.Zero;
				PoincareWindow.AngleOffset = 0;
				PoincareWindow.Reset();
				break;
				
			case JoystickButton.Button9: // Pad 10
				PoincareWindow.Offset = Complex.Zero;
				PoincareWindow.AngleOffset = 0;
				PoincareWindow.P = 5;
				PoincareWindow.Q = 5;
				PoincareWindow.ImageIndex = 0;
				PoincareWindow.Reset();
				break;
				
			case JoystickButton.Button10: // Pad 11
				PoincareWindow.ImageIndex--;
				PoincareWindow.Reset();
				break;
				
			case JoystickButton.Button11: // Pad 12
				PoincareWindow.ImageIndex++;
				PoincareWindow.Reset();
				break;
			}
		}
		
		private void Joystick_ButtonUp(object sender, JoystickButtonEventArgs e) {
			switch (e.Button) {
			case JoystickButton.Button0: // Trigger
				isLimit = false;
				break;
	
			case JoystickButton.Button1: // Grip
				isBraking = false;
				break;
			}
		}
		
		public void Sample() {
			#pragma warning disable 0612
			// obsolete
			PoincareWindow.InputDriver.Poll(); 
			#pragma warning restore 0612

#if false
			string op = string.Empty;
			for (int i = 0; i < Joystick.Axis.Count; i++) 
				op += string.Format("{0} ", Joystick.Axis[i]);
			for (int i = 0; i < Joystick.Button.Count; i++) 
				op += string.Format("{0} ", Joystick.Button[i]);
			Console.WriteLine(op);
#endif
			
			double scale = 0.001;
			double limit = 0.15;
			PoincareWindow.Offset += new Complex(Joystick.Axis[0] * scale, Joystick.Axis[1] * scale);
			if (PoincareWindow.Offset.ModulusSquared > limit * limit)
				PoincareWindow.Offset = PoincareWindow.Offset.Normalized * limit;
			
			PoincareWindow.AngleOffset -= Joystick.Axis[2] * scale;
			if (Math.Abs(PoincareWindow.AngleOffset) > limit)
				PoincareWindow.AngleOffset = Math.Sign(PoincareWindow.AngleOffset) * limit;
			
			PoincareWindow.ImageSpeed = Math.Pow(Joystick.Axis[3], 2) / 5 * Math.Sign(Joystick.Axis[3]);
			
			if (Joystick.Axis[4] == 0 && Joystick.Axis[5] == 0)
				disablePQ = false;
			
			if (Joystick.Axis[4] != 0 && !disablePQ) {
				PoincareWindow.P += (int)Joystick.Axis[4];
				disablePQ = true;
				PoincareWindow.Reset();
			}
			
			if (Joystick.Axis[5] != 0 && !disablePQ) {
				PoincareWindow.Q += (int)Joystick.Axis[5];
				disablePQ = true;
				PoincareWindow.Reset();
			}
			
			if (isLimit) {
				double ratio = 0.8;
				double oldAngleOffset = PoincareWindow.AngleOffset;
				PoincareWindow.MakeLimitRotation();
				PoincareWindow.AngleOffset = (PoincareWindow.AngleOffset * (1 - ratio) + oldAngleOffset * ratio);
			}
			
			if (isBraking) {
				PoincareWindow.Offset *= 0.9;
				PoincareWindow.AngleOffset *= 0.9;
			}
		}
	}
}