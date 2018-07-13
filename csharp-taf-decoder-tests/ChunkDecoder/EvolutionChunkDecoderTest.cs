using csharp_taf_decoder;
using csharp_taf_decoder.chunkdecoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.CloudLayer;
using static csharp_taf_decoder.entity.Evolution;

namespace csharp_taf_decoder_tests.ChunkDecoder
{
    [TestFixture, Category("EvolutionChunkDecoder")]
    public class EvolutionChunkDecoderTest
    {
        private static readonly EvolutionChunkDecoder chunkDecoder = new EvolutionChunkDecoder(false, false);

        public static List<EvolutionChunkDecoderTester> Chunks => new List<EvolutionChunkDecoderTester>()
        {
            new EvolutionChunkDecoderTester() {
                Strict = true,
                Base = "TAF KJFK 080500Z 0806/0910 23010KT 6 1/4SM BKN020",
                EvoChunk = "BECMG 0807/0810 23024KT P6SM +SHRA BKN025 TX08/0910Z TNM01/0904",
                Type = "BECMG",
                Probability = string.Empty,
                FromDay = 8,
                FromTime = "07:00 UTC",
                ToDay = 8,
                ToTime = "10:00 UTC",
                Elements = new List<Element>()
                {
                    new Element()
                    {
                        WindDirection = 230,
                        WindSpeed = 24,
                        Visibility = 6,
                        Cavok = false,
                        Greater = true,
                        WeatherPhenomena = new List<string>() { "RA" },
                        WeatherIntensity = "+",
                        WeatherDesc = "SH",
                        CloudsAmount = CloudAmount.BKN ,
                        CloudsBaseHeight = 2500,
                        MinimumTemperatureValue = -1,
                        MaximumTemperatureValue = 8,
                        EmbEvolutionType = null,
                    }
                }
            }
        };
    }

    public class EvolutionChunkDecoderTester
    {
        public bool Strict { get; set; }
        public string Base { get; set; }
        public string EvoChunk { get; set; }
        public string Chunk { get; set; }
        public string Type { get; set; }
        public string Probability { get; set; }
        public int FromDay { get; set; }
        public string FromTime { get; set; }
        public int ToDay { get; set; }
        public string ToTime { get; set; }
        public List<Element> Elements { get; set; }
    }

    public class Element
    {
        public int WindDirection { get; set; }
        public int WindSpeed { get; set; }
        public int? Visibility { get; set; }
        public bool Cavok { get; set; }
        public bool Greater { get; set; }
        public List<string> WeatherPhenomena { get; set; }
        public string WeatherIntensity { get; set; }
        public string WeatherDesc { get; set; }
        public CloudAmount CloudsAmount { get; set; }
        public int? CloudsBaseHeight { get; set; }
        public int? MinimumTemperatureValue { get; set; }
        public int? MaximumTemperatureValue { get; set; }
        public string EmbEvolutionType { get; set; }
    }
}
