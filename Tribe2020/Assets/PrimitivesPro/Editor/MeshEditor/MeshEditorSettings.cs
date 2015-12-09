using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PrimitivesPro.Editor.MeshEditor
{
	public class MeshEditorSettings
	{
		private int size = 10;
		private float dim = 10.0f;
		private bool show = false;
		private bool gridSnap = false;
		private bool stickOverlappingPoints = true;
		private bool vertexSnap = false;

		public bool StickOverlappingPoints
		{
			get { return stickOverlappingPoints; }
			set
			{
				if (stickOverlappingPoints != value)
				{
					stickOverlappingPoints = value;
					Serialize();
				}
			}
		}

		public bool VertexSnap
		{
			get { return vertexSnap; }
			set
			{
				if (vertexSnap != value)
				{
					vertexSnap = value;
					Serialize();
				}
			}
		}

		public int Size
		{
			get { return size; }
			set
			{
				if (size != value)
				{
					size = value;
					Serialize();
				}
			}
		}
		
		public float Dim
		{
			get { return dim; }
			set
			{
				if (Mathf.Abs(value - dim) > Mathf.Epsilon)
				{
					dim = value;
					Serialize();
				}
			}
		}
		
		public bool Show
		{
			get { return show; }
			set
			{
				if (show != value)
				{
					show = value;
					Serialize();
				}
			}
		}
		
		public bool GridSnap
		{
			get { return gridSnap; }
			set
			{
				if (gridSnap != value)
				{
					gridSnap = value;
					Serialize();
				}
			}
		}

		public void Serialize()
		{
			var dic = new Dictionary<string, object>
			{
				{"GridSize", Size},
				{"GridDim", Dim},
				{"GridShow", Show},
				{"GridSnap", GridSnap},
				{"VertexSnapping", VertexSnap},
				{"StickOverlappingPoints", StickOverlappingPoints}
			};
			
			var jsonString = ThirdParty.Json.Serialize(dic);
			Utils.WriteTextFile(Application.dataPath + "/PrimitivesPro/Config/config.json", jsonString);
		}
		
		public bool Deserialize()
		{
			var jsonString = Utils.ReadTextFile(Application.dataPath + "/PrimitivesPro/Config/config.json");
			
			if (jsonString != null)
			{
				var dic = ThirdParty.Json.Deserialize(jsonString) as Dictionary<string, object>;
				
				if (dic != null)
				{
					try
					{
						Size = System.Convert.ToInt32(dic["GridSize"]);
						Dim = System.Convert.ToSingle(dic["GridDim"]);
						Show = System.Convert.ToBoolean(dic["GridShow"]);
						GridSnap = System.Convert.ToBoolean(dic["GridSnap"]);
						VertexSnap = System.Convert.ToBoolean(dic["VertexSnapping"]);
						StickOverlappingPoints = System.Convert.ToBoolean(dic["StickOverlappingPoints"]);
					}
					catch
					{
						return false;
					}

					return true;
				}
			}
			
			return false;
		}
	}
}