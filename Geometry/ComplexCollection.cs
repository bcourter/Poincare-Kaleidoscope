using System;
using System.Collections.Generic;
using OpenTK;

namespace Poincare.Geometry {	
	public struct ComplexCollection {
		int sectorCount;
		SortedList<int, List<Complex>>[] sectors;
		const int radiusResolution = 10000;

		public ComplexCollection(int sectorCount) {
			this.sectorCount = sectorCount;
			
			sectors = new SortedList<int, List<Complex>>[sectorCount];
			Clear();
		}
		
		public void Add(Complex c) {
			int sector = Sector(c);
			int radius = (int)Math.Floor(c.ModulusSquared * radiusResolution);
			
			if (sectors[sector].ContainsKey(radius)) {
		//		if (!ContainsValue(c)) {
					sectors[sector][radius].Add(c);
		//		}	 
				return;
			}
			
			sectors[sector].Add(radius, new List<Complex>(100));
			sectors[sector][radius].Add(c);
		}

		public bool ContainsValue(Complex c) {
			int sector = Sector(c);
			int radius = (int)Math.Floor(c.ModulusSquared * radiusResolution);
			if (!sectors[sector].ContainsKey(radius))
				return false;
			
			foreach (Complex point in sectors[sector][radius]) {
				if ((point - c).ModulusSquared < Accuracy.LinearToleranceSquared)
					return true;
			}
			
			return false;
		}
		
		public void Clear() {
			for (int i = 0; i < sectorCount; i++) 
				sectors[i] = new SortedList<int, List<Complex>>();
		}
		
		public int Sector(Complex c) {
			return (int) ((c.Argument + Math.PI) / (Math.PI * 2 / sectorCount)) % sectorCount;
		}

	}
}

