using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MiniSExtraData 
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct extraInfo
	{
		//左脚着地
		public bool		isLeftGround;
		//右脚着地
		public bool		isRightGround;
		public bool		isLeftStatic;
		public bool		isRightStatic;

		public int		motionType;
		public int		action;
		public bool		isMoving;
		public int		isForward;
		public double	calorie;
		public Vector3	skatingSpeed;
		public Vector3	lFootSpeed;
		public Vector3	rFootSpeed;
		//为什么这里的震动模块是int类型的呢？
		public int		VibrateAction;
	};

	public static extraInfo GetExtraInfoMiniS(KATNativeSDK.TreadMillData data)
	{
		GCHandle handle = GCHandle.Alloc(data.extraData, GCHandleType.Pinned);
		extraInfo info;
		try
		{
			info = (extraInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(extraInfo));
		}
		finally
		{
			handle.Free();
		}
		return info;
	}
}
