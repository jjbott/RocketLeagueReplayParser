using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RocketLeagueReplayParser.NetworkStream;

namespace RocketLeagueReplayParser.Tests
{
    [TestFixture]
    public class ActorStatePropertyTests
    {
        [Test]
        [TestCase("0100110110100000000000110000000001100100110000000100000000000000000000000000000110000000000000000001010001000000010001111111100111001110001010100001110000110000001")]
        [TestCase("01001101101000000000001100000000011001001100000001000000000000000000000000000000000000000000000000")]
        public void ReplicatedRBState(string bitString)
        {
            var property = new Mock<IClassNetCacheProperty>(MockBehavior.Strict);
            property.SetupGet(x => x.Id).Returns(50);
            property.SetupGet(x => x.Index).Returns(50);

            var classMap = new Mock<IClassNetCache>(MockBehavior.Strict);
            classMap.SetupGet(x => x.MaxPropertyId).Returns(50);
            classMap.SetupGet(x => x.ObjectIndex).Returns(1);
            classMap.Setup(x => x.GetProperty(50)).Returns(property.Object);

            var objectToNameMap = new Dictionary<int, string>();
            objectToNameMap[50] = "TAGame.RBActor_TA:ReplicatedRBState";
            objectToNameMap[1] = "TAGame.Car_TA";

            var br = new BitReader(bitString);

            var asp = ActorStateProperty.Deserialize(classMap.Object, objectToNameMap, br);

            Assert.IsTrue(br.EndOfStream);
        }
    }
}
