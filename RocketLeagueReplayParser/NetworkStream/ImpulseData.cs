using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
	public class ImpulseData
	{
		public Int32 CompressedRotation { get; private set; }
		public float ImpulseSpeed { get; private set; }

		public static ImpulseData Deserialize(BitReader br)
		{
			var id = new ImpulseData();
			id.CompressedRotation = br.ReadInt32();
			id.ImpulseSpeed = br.ReadFloat();
			return id;
		}

		public void Serialize(BitWriter bw)
		{
			bw.Write(CompressedRotation);
			bw.Write(ImpulseSpeed);
		}

		public override string ToString()
		{
			return string.Format("CompressedRotation: {0}, ImpulseSpeed: {1}", CompressedRotation, ImpulseSpeed);
		}
	}
}
