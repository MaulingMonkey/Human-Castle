using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D9;
namespace HumanCastle.Model
{
	class Material
	{
		public string Name { get; set; }
		public Texture Texture { get; set; }

		public Material()
		{
			Name = "Grass";
			Texture = null;
		}
	}
}
