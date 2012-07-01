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
        JoystickMapping mapping = System.Environment.OSVersion.Platform == PlatformID.Unix ? JoystickMapping.UbuntuMapping : JoystickMapping.WindowsMapping;
		
		public JoystickDevice Joystick { get; private set; }

		public PoincareWindow PoincareWindow { get; private set; }
		
		public JoystickControl(JoystickDevice joystick, PoincareWindow poincareWindow) {
			Joystick = joystick;
			PoincareWindow = poincareWindow;
			
			Joystick.ButtonDown += Joystick_ButtonDown;
			Joystick.ButtonUp += Joystick_ButtonUp;
		}
				
		private void Joystick_ButtonDown(object sender, JoystickButtonEventArgs e) {

			if (e.Button == mapping.ButtonBrake) // Trigger
				isBraking = true;
				
			if (e.Button == mapping.ButtonLimitRotation) // Grip
				isLimit = true;

            if (e.Button == mapping.ButtonPreviousImage) { // Thumb bottom left
                PoincareWindow.ImageIndex--;
                PoincareWindow.Reset();
            }

            if (e.Button == mapping.ButtonNextImage) { // Thumb bottom right
                PoincareWindow.ImageIndex++;
                PoincareWindow.Reset();
            }

            if (e.Button == mapping.ButtonInvert) { // Thumb top left
                PoincareWindow.IsInverting = !PoincareWindow.IsInverting;
                PoincareWindow.Reset();
            }
            if (e.Button == mapping.ButtonRandomize) {// Thumb top right
                PoincareWindow.Randomize();
            }

			if (e.Button == mapping.ButtonAutoMove) // Pad 7
				PoincareWindow.IsMoving = !PoincareWindow.IsMoving;
				
			if (e.Button == mapping.ButtonAutoRandomize) // Pad 8
				PoincareWindow.IsRandomizing = !PoincareWindow.IsRandomizing;

            if (e.Button == mapping.ButtonRecenter) { // Pad 9
                PoincareWindow.Offset = Complex.Zero;
                PoincareWindow.AngleOffset = 0;
                PoincareWindow.Reset();
            }

            if (e.Button == mapping.ButtonReset) { // Pad 10
                PoincareWindow.Offset = Complex.Zero;
                PoincareWindow.AngleOffset = 0;
                PoincareWindow.P = 5;
                PoincareWindow.Q = 5;
                PoincareWindow.ImageIndex = 0;
                PoincareWindow.Reset();
            }

            if (e.Button == mapping.ButtonPreviousImage2) { // Pad 11
                PoincareWindow.ImageIndex--;
                PoincareWindow.Reset();
            }

            if (e.Button == mapping.ButtonNextImage2) { // Pad 12
                PoincareWindow.ImageIndex++;
                PoincareWindow.Reset();
            }

		}
		
		private void Joystick_ButtonUp(object sender, JoystickButtonEventArgs e) {
			if (e.Button == mapping.ButtonBrake) // Trigger
				isBraking = false;
	
			if (e.Button == mapping.ButtonLimitRotation) // Grip
				isLimit = false;

		}
		
		public void Sample(double timing) {
			// obsolete but necessary
			#pragma warning disable 0612
			PoincareWindow.InputDriver.Poll(); 
			#pragma warning restore 0612
			
			double scale = 0.01 * timing; // 0.001
			double limit = 0.15;
			PoincareWindow.Offset += new Complex(Joystick.Axis[mapping.AxisX] * scale, Joystick.Axis[mapping.AxisY] * scale);
			if (PoincareWindow.Offset.ModulusSquared > limit * limit)
				PoincareWindow.Offset = PoincareWindow.Offset.Normalized * limit;
			
			PoincareWindow.AngleOffset -= Joystick.Axis[mapping.AxisRotation] * scale;
			if (Math.Abs(PoincareWindow.AngleOffset) > limit)
				PoincareWindow.AngleOffset = Math.Sign(PoincareWindow.AngleOffset) * limit;
			
			PoincareWindow.ImageSpeed = Math.Pow(Joystick.Axis[mapping.AxisImageSpeed], 2) / 5 * Math.Sign(Joystick.Axis[mapping.AxisImageSpeed]);
			
			if (Joystick.Axis[mapping.AxisP] == 0 && Joystick.Axis[mapping.AxisQ] == 0)
				disablePQ = false;
			
			if (Joystick.Axis[mapping.AxisP] != 0 && !disablePQ) {
				PoincareWindow.P += (int)Joystick.Axis[mapping.AxisP];
				disablePQ = true;
				PoincareWindow.Reset();
			}
			
			if (Joystick.Axis[mapping.AxisQ] != 0 && !disablePQ) {
				PoincareWindow.Q += (int)Joystick.Axis[mapping.AxisQ];
				disablePQ = true;
				PoincareWindow.Reset();
			}
			
			if (isLimit) {
				double ratio = 0.8; //0.8;
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

	public class JoystickMapping {
		public JoystickButton ButtonBrake { get; set; }
		public JoystickButton ButtonLimitRotation { get; set; }
		public JoystickButton ButtonPreviousImage { get; set; }
		public JoystickButton ButtonNextImage { get; set; }
		public JoystickButton ButtonInvert { get; set; }
		public JoystickButton ButtonRandomize { get; set; }
		public JoystickButton ButtonAutoMove { get; set; }
		public JoystickButton ButtonAutoRandomize { get; set; }
		public JoystickButton ButtonRecenter { get; set; }
		public JoystickButton ButtonReset { get; set; }
		public JoystickButton ButtonPreviousImage2 { get; set; }
		public JoystickButton ButtonNextImage2 { get; set; }

		public int AxisX { get; set; }
		public int AxisY { get; set; }
		public int AxisRotation { get; set; }
		public int AxisImageSpeed { get; set; }
		public int AxisP { get; set; }
		public int AxisQ { get; set; }

		private JoystickMapping() {
		}

        public static JoystickMapping UbuntuMapping {
            get {
                JoystickMapping mapping = new JoystickMapping();
                mapping.ButtonBrake = JoystickButton.Button0;
                mapping.ButtonLimitRotation = JoystickButton.Button1;
                mapping.ButtonPreviousImage = JoystickButton.Button2;
                mapping.ButtonNextImage = JoystickButton.Button3;
                mapping.ButtonInvert = JoystickButton.Button4;
                mapping.ButtonRandomize = JoystickButton.Button5;
                mapping.ButtonAutoMove = JoystickButton.Button6;
                mapping.ButtonAutoRandomize = JoystickButton.Button7;
                mapping.ButtonRecenter = JoystickButton.Button8;
                mapping.ButtonReset = JoystickButton.Button9;
                mapping.ButtonPreviousImage2 = JoystickButton.Button10;
                mapping.ButtonNextImage2 = JoystickButton.Button11;

                mapping.AxisX = 0;
                mapping.AxisY = 1;
                mapping.AxisRotation = 2;
                mapping.AxisImageSpeed = 3;
                mapping.AxisP = 4;
                mapping.AxisQ = 5;

                return mapping;
            }
        }

        public static JoystickMapping WindowsMapping {
            get {
                JoystickMapping mapping = new JoystickMapping();
                mapping.ButtonBrake = JoystickButton.Button0;
                mapping.ButtonLimitRotation = JoystickButton.Button1;
                mapping.ButtonPreviousImage = JoystickButton.Button2;
                mapping.ButtonNextImage = JoystickButton.Button3;
                mapping.ButtonInvert = JoystickButton.Button4;
                mapping.ButtonRandomize = JoystickButton.Button5;
                mapping.ButtonAutoMove = JoystickButton.Button6;
                mapping.ButtonAutoRandomize = JoystickButton.Button7;
                mapping.ButtonRecenter = JoystickButton.Button8;
                mapping.ButtonReset = JoystickButton.Button9;
                mapping.ButtonPreviousImage2 = JoystickButton.Button10;
                mapping.ButtonNextImage2 = JoystickButton.Button11;

                mapping.AxisX = 0;
                mapping.AxisY = 1;
                mapping.AxisRotation = 3;
                mapping.AxisImageSpeed = 2;
                mapping.AxisP = 4;
                mapping.AxisQ = 5;

                return mapping;
            }
        }    

	}           
}                                                     
