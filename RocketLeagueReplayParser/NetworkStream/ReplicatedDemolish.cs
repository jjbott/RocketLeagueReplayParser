﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolish
    {
        public bool Unknown1 { get; private set;}
        public Int32 AttackerActorId { get; private set;} 
        public bool Unknown2 { get; private set;}
        public Int32 VictimActorId { get; private set;} // Always equals this actor's id
        public Vector3D AttackerVelocity { get; private set;} // Not verified. Attacker/Victim velocity could be swapped
        public Vector3D VictimVelocity { get; private set;}

        public static ReplicatedDemolish Deserialize(BitReader br)
        {
            var rd = new ReplicatedDemolish();
            
            rd.Unknown1 = br.ReadBit();
            rd.AttackerActorId = br.ReadInt32();
            rd.Unknown2 = br.ReadBit();
            rd.VictimActorId = br.ReadInt32();
            rd.AttackerVelocity = Vector3D.Deserialize(br);
            rd.VictimVelocity = Vector3D.Deserialize(br);

            return rd;
        }
    }
}