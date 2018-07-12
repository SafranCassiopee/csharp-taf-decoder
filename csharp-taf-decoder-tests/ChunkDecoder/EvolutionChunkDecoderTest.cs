using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.Evolution;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("EvolutionChunkDecoder")]
    public class EvolutionChunkDecoderTest
    {
        private static readonly EvolutionChunkDecoder chunkDecoder = new EvolutionChunkDecoder(false, false);
    }
}
