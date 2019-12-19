using UnityEngine;
using System.Collections;
namespace CrowdBehavior
{
	public static class Extensions
	{
		public static Vector3 Truncate (this Vector3 val, float maxLength)
		{
			if (val.magnitude > maxLength)
			{
				val.Normalize ();
				val *= maxLength;
			}

			return val;
		}
	}

}