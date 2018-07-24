﻿using csharp_taf_decoder.entity;
using System.Collections.Generic;

namespace csharp_taf_decoder.chunkdecoder
{
    public sealed class WeatherChunkDecoder : TafChunkDecoder
    {
        public const string WeatherPhenomenonParameterName = "WeatherPhenomenons";

        public static HashSet<string> Descriptions = new HashSet<string>() {
            "TS", "FZ", "SH", "BL",
            "DR", "MI", "BC", "PR",
        };
        public static HashSet<string> Phenomenons = new HashSet<string>() {
            "DZ", "RA", "SN", "SG",
            "PL", "DS", "GR", "GS",
            "UP", "IC", "FG", "BR",
            "SA", "DU", "HZ", "FU",
            "VA", "PY", "DU", "PO",
            "SQ", "FC", "DS", "SS",
            "//",
        };
        public static string PwRegexPattern = $"([-+]|VC)?({string.Join("|", Descriptions)})?({string.Join("|", Phenomenons)})?({string.Join("|", Phenomenons)})?({string.Join("|", Phenomenons)})?";

        public override string GetRegex()
        {
            return $"^({PwRegexPattern} )?({PwRegexPattern} )?({PwRegexPattern} )?()?";
        }

        public override Dictionary<string, object> Parse(string remainingTaf, bool withCavok = false)
        {
            string newRemainingTaf;
            var found = Consume(remainingTaf, out newRemainingTaf);
            var result = new Dictionary<string, object>();

            var weatherPhenomenons = new List<WeatherPhenomenon>();
            for (var i = 1; i <= 13; i += 6)
            {
                if (!string.IsNullOrEmpty(found[i].Value) && found[i + 3].Value != "//")
                {
                    var weatherPhenomenon = new WeatherPhenomenon();
                    weatherPhenomenon.IntensityProximity = found[i + 1].Value;
                    weatherPhenomenon.Descriptor = found[i + 2].Value;
                    for (var k = 3; k <= 5; ++k)
                    {
                        if (!string.IsNullOrEmpty(found[i + k].Value))
                        {
                            weatherPhenomenon.Phenomena.Add(found[i + k].Value);
                        }
                    }
                    weatherPhenomenons.Add(weatherPhenomenon);
                }
            }

            result.Add(WeatherPhenomenonParameterName, weatherPhenomenons);

            return GetResults(newRemainingTaf, result);
        }
    }
}
