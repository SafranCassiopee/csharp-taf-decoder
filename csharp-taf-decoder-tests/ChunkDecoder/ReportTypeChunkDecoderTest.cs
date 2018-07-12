using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.DecodedTaf;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("ReportTypeChunkDecoder")]
    public class ReportTypeChunkDecoderTest
    {
        private static readonly ReportTypeChunkDecoder chunkDecoder = new ReportTypeChunkDecoder();

        [Test, TestCaseSource("ValidChunks")]
        public void TestParse(Tuple<string, TafType, string> chunk)
        {
            var decoded = chunkDecoder.Parse(chunk.Item1);
            Assert.AreEqual(chunk.Item2, (decoded[TafDecoder.ResultKey] as Dictionary<string, object>)[ReportTypeChunkDecoder.TypeParameterName]);
            Assert.AreEqual(chunk.Item3, decoded[TafDecoder.RemainingTafKey]);
        }
        public static List<Tuple<string, TafType, string>> ValidChunks => new List<Tuple<string, TafType, string>>()
        {
            new Tuple<string, TafType, string>("TAF LFPG", TafType.TAF, "LFPG"),
            new Tuple<string, TafType, string>("TAF TAF LFPG", TafType.TAF, "LFPG"),
            new Tuple<string, TafType, string>("TAF AMD LFPO",TafType.TAFAMD, "LFPO"),
            new Tuple<string, TafType, string>("TA LFPG", TafType.NULL, "TA LFPG"),
            new Tuple<string, TafType, string>("123 LFPO", TafType.NULL, "123 LFPO"),
            new Tuple<string, TafType, string>("TAF COR LFPO", TafType.TAFCOR, "LFPO"),
        };
    }
}
