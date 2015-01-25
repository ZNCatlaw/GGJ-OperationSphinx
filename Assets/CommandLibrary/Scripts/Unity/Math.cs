using System;
using UnityEngine;

namespace CommandLibrary
{

public static class MathExtensions
{
	/// <summary>
	/// Performs a normalised lerp of two quaternions. Normalised lerps
	/// are commutative, and torque-minimal (shortest path), but have
	/// non-constant velocity, (unlike slerp).
	/// </summary>
	/// <returns>The interpolated rotation</returns>
	/// <param name="startRotation">  The starting rotation. This will be output when t = 0. </param>
	/// <param name="endRotation"> The finish rotation. This will be output when t = 1. </param>
	/// <param name="t">
	/// 	The progress fraction between startRotation and endRotation. 
	/// 	Note that unlike Unity's lerp functions, this value is not clamped between 0-1, 
	/// 	which allows eases like Elastic and Bounce to behave correctly.
	/// </param> 
	public static Quaternion Nlerp(Quaternion startRotation, Quaternion endRotation, float t)
	{
		Vector4 startRotationVec = new Vector4(startRotation.x, startRotation.y, startRotation.z, startRotation.w);
		Vector4 endRotationVec = new Vector4(endRotation.x, endRotation.y, endRotation.z, endRotation.w);
		Vector4 resultVec = ((endRotationVec - startRotationVec) * (float) t + startRotationVec).normalized;
		return new Quaternion(resultVec.x, resultVec.y, resultVec.z, resultVec.w);
	}
}

}

