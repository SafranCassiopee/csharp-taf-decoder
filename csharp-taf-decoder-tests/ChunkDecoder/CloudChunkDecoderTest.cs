using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.CloudLayer;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("CloudChunkDecoder")]
    public class CloudChunkDecoderTest
    {
        private readonly CloudChunkDecoder chunkDecoder = new CloudChunkDecoder();

        /// <summary>
        /// Test parsing of valid cloud chunks
        /// </summary>
        [Test, TestCaseSource("ValidChunks")]
        public void TestParse(Tuple<string, int, CloudAmount, int?, CloudType, string> validChunk)
        {
            var decoded = chunkDecoder.Parse(validChunk.Item1);
            var clouds = (decoded[TafDecoder.ResultKey] as Dictionary<string, object>)[CloudChunkDecoder.CloudsParameterName] as List<CloudLayer>;
            Assert.AreEqual(validChunk.Item2, clouds.Count);
            if (clouds.Count > 0)
            {
                var cloud = clouds[0];
                Assert.AreEqual(validChunk.Item3, cloud.Amount);
                if (validChunk.Item4.HasValue)
                {
                    Assert.AreEqual(validChunk.Item4, cloud.BaseHeight.ActualValue);
                    Assert.AreEqual(Value.Unit.Feet, cloud.BaseHeight.ActualUnit);
                }
                else
                {
                    Assert.IsNull(cloud.BaseHeight);
                }
                Assert.AreEqual(validChunk.Item5, cloud.Type);
            }
            Assert.AreEqual(validChunk.Item6, decoded[TafDecoder.RemainingTafKey]);
        }

        /// <summary>
        /// Test parsing with invalid cloud chunks but with CAVOK earlier in the TAF
        /// </summary>
        /// <param name="chunk"></param>
        [Test, TestCaseSource("InvalidChunks")]
        public void TestParseCAVOKChunk(string chunk)
        {
            Dictionary<string, object> decoded = null;
            Assert.DoesNotThrow(() =>
            {
                decoded = chunkDecoder.Parse(chunk, true);
            });
            var clouds = (decoded[TafDecoder.ResultKey] as Dictionary<string, object>)[CloudChunkDecoder.CloudsParameterName] as List<CloudLayer>;
            Assert.AreEqual(0, clouds.Count);
        }

        /// <summary>
        ///  Test parsing of invalid cloud chunks
        /// </summary>
        /// <param name="chunk"></param>
        [Test, TestCaseSource("InvalidChunks")]
        public void TestParseInvalidChunk(string chunk)
        {
            Assert.Throws(typeof(TafChunkDecoderException), () =>
            {
                chunkDecoder.Parse(chunk);
            });
        }
        public static List<Tuple<string, int, CloudAmount, int?, CloudType, string>> ValidChunks => new List<Tuple<string, int, CloudAmount, int?, CloudType, string>>()
        {
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("VV085 AAA", 1, CloudAmount.VV, 8500, CloudType.NULL, "AAA"),
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("BKN200TCU OVC250 VV/// BBB", 3, CloudAmount.BKN, 20000, CloudType.TCU, "BBB"),
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("OVC////// FEW250 CCC", 2, CloudAmount.OVC, null, CloudType.CannotMeasure, "CCC"),
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("NSC DDD", 0, CloudAmount.NULL, null, CloudType.NULL, "DDD"),
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("SKC EEE", 0, CloudAmount.NULL, 1, CloudType.CannotMeasure, "EEE"),
            new Tuple<string, int, CloudAmount, int?, CloudType, string>("NCD FFF", 0, CloudAmount.NULL, 1, CloudType.CannotMeasure, "FFF"),
        };

        public static List<string> InvalidChunks => new List<string>()
        {
            "FEW10 EEE" , "AAA EEE", "BKN100A EEE",
        };
    }
}