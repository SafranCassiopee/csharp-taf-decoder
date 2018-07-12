using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("WeatherPhenomenon")]
    public class WeatherPhenomenonDecoderTest
    {
        private static readonly WeatherPhenomenon chunkDecoder = new WeatherPhenomenon();
    }
}
