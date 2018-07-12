using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("ReportTypeChunkDecoder")]
    public class ReportTypeChunkDecoderTest
    {
        private static readonly ReportTypeChunkDecoder chunkDecoder = new ReportTypeChunkDecoder();
    }
}
