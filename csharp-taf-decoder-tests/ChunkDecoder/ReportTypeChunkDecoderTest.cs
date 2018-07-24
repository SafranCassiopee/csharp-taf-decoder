using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("ReportTypeChunkDecoder")]
    public class ReportTypeChunkDecoderTest
    {
        private static readonly ReportTypeChunkDecoder chunkDecoder = new ReportTypeChunkDecoder();

        [Test, TestCaseSource("ValidChunks")]
        public void TestParse(Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string> chunk)
        {
            var decoded = chunkDecoder.Parse(chunk.Item1);
            Assert.AreEqual(chunk.Item2, (decoded[TafDecoder.ResultKey] as Dictionary<string, object>)[ReportTypeChunkDecoder.TypeParameterName]);
            Assert.AreEqual(chunk.Item3, decoded[TafDecoder.RemainingTafKey]);
        }
        public static List<Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>> ValidChunks => new List<Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>>()
        {
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("TAF LFPG",     csharp_taf_decoder.entity.DecodedTaf.TafType.TAF,       "LFPG"),
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("TAF TAF LFPG", csharp_taf_decoder.entity.DecodedTaf.TafType.TAF,       "LFPG"),
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("TAF AMD LFPO", csharp_taf_decoder.entity.DecodedTaf.TafType.TAFAMD,    "LFPO"),
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("TA LFPG",      csharp_taf_decoder.entity.DecodedTaf.TafType.NULL,      "TA LFPG"),
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("123 LFPO",     csharp_taf_decoder.entity.DecodedTaf.TafType.NULL,      "123 LFPO"),
            new Tuple<string, csharp_taf_decoder.entity.DecodedTaf.TafType, string>("TAF COR LFPO", csharp_taf_decoder.entity.DecodedTaf.TafType.TAFCOR,    "LFPO"),
        };
    }
}
