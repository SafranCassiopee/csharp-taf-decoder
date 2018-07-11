using System;
using System.Collections.Generic;
using static csharp_taf_decoder.entity.DecodedTaf;

namespace csharp_taf_decoder.chunkdecoder
{
    public sealed class ReportTypeChunkDecoder : TafChunkDecoder
    {
        public const string TypeParameterName = "Type";

        public override string GetRegex()
        {
            return "^((TAF)( TAF)*( AMD| COR){0,1}) ";
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
                result = null;
            }
            else
            {
                // retrieve found params
                // 'TAF' sometimes happens to be duplicated
                result.Add(TypeParameterName, (TafType)Enum.Parse(typeof(TafType), found[1].Value.Replace("TAF TAF", "TAF")));
            }

            return GetResults(newRemainingTaf, result);
        }
    }
}
