Poincare'
(c) 2012 Blake Courter  (bcourter <at> mac <dot> com)
Alpha 0.01
WARNING: This is barely-tested software. Using it could destroy the planet, for all we know.

Usage: Poincare.exe [Path to image directory]
Requires .NET Framework 4 or Mono 2.10.5

Drag inside the horizon to translate.
Drag outside the horizon to rotate.
To achieve a limit rotation, match the translation and rotation so a point on the horizon is fixed.

Keyboard commands:
	F: Fullscreen. Currently experiences different behavior across platforms.
	Escape: Quit.
	p/P: Increase/Decrease vertices per polygon.
	q/Q: Increase/Decrease polygons per vertex.
	R: Reset the origin and stop all movement. Also resets framerate optimization.
	n/N: Advance to next/previous image.
	Z: Randomize p, q, and the image.
	I: Invert color of odd-parity regions.

Note: p and q are equavalent to Schläfli symbol{p,q}, where (p - 2)(q - 2) > 4 for hyperbolic tessellations.

References:
The incredibly readable Visual Complex Analysis by Tristan Needham: http://www.amazon.com/Visual-Complex-Analysis-Tristan-Needham/dp/0198534469
Great paper on the basics of tiling the disc and a hi fidelity lookup technique: http://poincare.sourceforge.net/
Original inspiration. I used it so much I copied his shortcuts: http://www.plunk.org/~hatch/HyperbolicApplet/
Inverting generalized circle-lines: http://www.cefns.nau.edu/~schulz/moe.pdf