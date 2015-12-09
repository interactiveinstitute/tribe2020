// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor.MeshEditor
{
    public class Grid
    {
		MeshEditorSettings settings;

		public Grid(MeshEditorSettings settings)
        {
			this.settings = settings;
        }

        public void ShowToggle()
        {
			settings.Show = !settings.Show;
        }

        public bool IsVisible()
        {
            return settings.Show;
        }

        public Vector3 FindClosestGridPointXZ(Vector3 point)
        {
            var gridSize = settings.Dim/settings.Size;

            var xCoord = (int)System.Math.Round(point.x/gridSize, 0);
            var yCoord = (int)System.Math.Round(point.z/gridSize, 0);

            return new Vector3(xCoord*gridSize, point.y, yCoord*gridSize);
        }

        public void Draw()
        {
			if (!settings.Show)
            {
                return;
            }

			var perSide = settings.Size;

			var min = -settings.Dim / 2;
			var max = settings.Dim / 2;

            for (int i = 0; i <= perSide; i++)
            {
                var t = ((float) i/perSide);

                var p0 = new Vector3(t*min + (1 - t)*max, 0.0f, min);
                var p1 = new Vector3(t * min + (1 - t) * max, 0.0f, max);

                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);

                MeshUtils.Swap(ref p0.x, ref p0.z);
                MeshUtils.Swap(ref p1.x, ref p1.z);
                Handles.DrawLine(p0, p1);
            }
        }
    }
}
