using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Poincare.Geometry {
	public static class Extensions {

// r,g,b values are from 0 to 1
// h = [0,360], s = [0,1], v = [0,1]
//		if s == 0, then h = -1 (undefined)
//public static void RGBtoHSV( float r, float g, float b, float *h, float *s, float *v )
//{
//	float min, max, delta;
//	min = MIN( r, g, b );
//	max = MAX( r, g, b );
//	*v = max;				// v
//	delta = max - min;
//	if( max != 0 )
//		*s = delta / max;		// s
//	else {
//		// r = g = b = 0		// s = 0, v is undefined
//		*s = 0;
//		*h = -1;
//		return;
//	}
//	if( r == max )
//		*h = ( g - b ) / delta;		// between yellow & magenta
//	else if( g == max )
//		*h = 2 + ( b - r ) / delta;	// between cyan & yellow
//	else
//		*h = 4 + ( r - g ) / delta;	// between magenta & cyan
//	*h *= 60;				// degrees
//	if( *h < 0 )
//		*h += 360;
//}
		
		/// <summary>
		/// HSs the vto RG.
		/// </summary>
		/// <returns>
		/// The vto RG.
		/// </returns>
		/// <param name='h'>
		/// H [0, 2 * PI].
		/// </param>
		/// <param name='s'>
		/// S [0, 1].
		/// </param>
		/// <param name='v'>
		/// V [0, 1].
		/// </param>
		/// 
		public static Color4 Color4FromHSV(float h, float s, float v, float a) {
			float r, g, b;
			int i;
			float f, p, q, t;
			if (s == 0) {
				// achromatic (grey)
				r = g = b = v;
				return new Color4(r, g, b, a);
			}
			
			//	h /= 60;			// sector 0 to 5
			h = (float)(h % (2 * Math.PI));
			h /= (float)Math.PI / 3;			// sector 0 to 5
			i = (int)Math.Floor(h);
			f = h - i;			// factorial part of h
			p = v * (1 - s);
			q = v * (1 - s * f);
			t = v * (1 - s * (1 - f));
			
			switch (i) {
			case 0:
				r = v;
				g = t;
				b = p;
				break;
			case 1:
				r = q;
				g = v;
				b = p;
				break;
			case 2:
				r = p;
				g = v;
				b = t;
				break;
			case 3:
				r = p;
				g = q;
				b = v;
				break;
			case 4:
				r = t;
				g = p;
				b = v;
				break;
			default:		// case 5:
				r = v;
				g = p;
				b = q;
				break;
			}
			
			return new Color4(r, g, b, a);
		}

		public static Complex[] Conjugate(this Complex[] z) {
			Complex[] result = new Complex[z.Length];
			for (int i = 0; i < z.Length; i++)
				result[i] = z[i].Conjugate;

			return result;
		}
	}
}

