using csharp_taf_decoder.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_taf_decoder.chunkdecoder
{
    public class ForecastPeriodChunkDecoder : TafChunkDecoder
    {
        public const string ForecastPeriodParameterName = "ForecastPeriod";
        public override string GetRegex()
        {
            return "^([0-9]{2})([0-9]{2})/([0-9]{2})([0-9]{2}) ";
        }

        public override Dictionary<string, object> Parse(string remainingTaf, bool withCavok = false)
        {
            var consumed = Consume(remainingTaf);
            var found = consumed.Value;
            var newRemainingTaf = consumed.Key;
            var result = new Dictionary<string, object>();

            // handle the case where nothing has been found
            if (found.Count <= 1)
            {
                throw new TafChunkDecoderException(remainingTaf, newRemainingTaf, TafChunkDecoderException.Messages.InvalidForecastPeriodInformation, this);
            }
            else
            {
                // retrieve found params and check them
                var forecastPeriod = new ForecastPeriod()
                {
                    FromDay = Convert.ToInt32(found[1].Value),
                    FromHour = Convert.ToInt32(found[2].Value),
                    ToDay = Convert.ToInt32(found[3].Value),
                    ToHour = Convert.ToInt32(found[4].Value),
                };
                if (!forecastPeriod.IsValid)
                {
                    throw new TafChunkDecoderException(remainingTaf, newRemainingTaf, TafChunkDecoderException.Messages.InvalidValuesForTheForecastPeriod, this);
                }

                result.Add(ForecastPeriodParameterName, forecastPeriod);
            }

            return GetResults(newRemainingTaf, result);
        }
    }
}
