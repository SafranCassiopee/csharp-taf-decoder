using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.ForecastPeriod;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("ForecastPeriodChunkDecoder")]
    public class ForecastPeriodChunkDecoderTest
    {
        private static readonly ForecastPeriodChunkDecoder chunkDecoder = new ForecastPeriodChunkDecoder();
    }
}
