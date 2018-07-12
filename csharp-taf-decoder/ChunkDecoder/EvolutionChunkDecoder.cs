using csharp_taf_decoder.entity;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace csharp_taf_decoder.chunkdecoder
{
    /// <summary>
    /// Chunk decoder for weather evolutions
    /// </summary>
    public sealed class EvolutionChunkDecoder : TafChunkDecoder
    {
        private const string TypePattern = "(BECMG\\s+|TEMPO\\s+|FM|PROB[034]{2}\\s+){1}";
        private const string PeriodPattern = "([0-9]{4}/[0-9]{4}\\s+|[0-9]{6}\\s+){1}";
        private const string RestPattern = "(.*)";

        private bool _withCavok;
        public bool IsStrict { get; set; }
        public string Remaining { get; private set; }
        public override string GetRegex()
        {
            return $"{TypePattern}{PeriodPattern}{RestPattern}";
        }

        private readonly ReadOnlyCollection<TafChunkDecoder> _decoderChain = new ReadOnlyCollection<TafChunkDecoder>(new List<TafChunkDecoder>
        {
            new SurfaceWindChunkDecoder(),
            new VisibilityChunkDecoder(),
            new WeatherPhenomenonChunkDecoder(),
            new CloudChunkDecoder(),
            new TemperatureChunkDecoder(),
        });

        public EvolutionChunkDecoder(bool isStrict, bool withCavok)
        {
            IsStrict = isStrict;
            _withCavok = withCavok;
        }

        public void Parse(string remainingTaf, DecodedTaf decodedTaf)
        {
            var consumed = Consume(remainingTaf);
            var found = consumed.Value;
            
            if (found.Count <= 1)
            {
                // the first chunk didn't match anything, so we remove it to avoid an infinite loop
                // note: regex approach wasn't working
                Remaining = remainingTaf.Substring(remainingTaf.IndexOfAny(new char[] { ' ', '\r', '\n' }) + 1);
                return;
            }

            var evolutionType = found[1].Value.Trim();
            var evolutionPeriod = found[2].Value.Trim();
            var remaining = found[3].Value;

            var evolution = new Evolution() { Type = evolutionType };
            if (remaining.StartsWith("PROB"))
            {
                // if the line started with PROBnn it won't have been consumed and we'll find it in remaining
                evolution.Probability = remaining.Trim();
            }
            // period
            if (evolutionType == "BECMG" || evolutionType == "TEMPO")
            {
                var periodArr = evolutionPeriod.Split('/');
                evolution.FromDay = Convert.ToInt32(periodArr[0].Substring(0, 2));
                evolution.FromTime = periodArr[0].Substring(2, 2) + ":00 UTC";
                evolution.ToDay = Convert.ToInt32(periodArr[1].Substring(0, 2));
                evolution.ToTime = periodArr[1].Substring(2, 2) + ":00 UTC";
            }
            else
            {
                evolution.FromDay = Convert.ToInt32(evolutionPeriod.Substring(0, 2));
                evolution.FromTime = evolutionPeriod.Substring(2, 2) + ':' + evolutionPeriod.Substring(4, 2) + " UTC";
            }
            // rest
            Remaining = ParseEntitiesChunk(evolution, remaining, decodedTaf);
            Remaining = remaining;
        }

        /// <summary>
        /// Extract the weather elements (surface winds, visibility, etc) between 2 evolution tags (BECMG, TEMPO or FM)
        /// </summary>
        /// <param name="evolution"></param>
        /// <param name="chunk"></param>
        /// <param name="decodedTaf"></param>
        /// <returns></returns>
        private string ParseEntitiesChunk(Evolution evolution, string chunk, DecodedTaf decodedTaf)
        {
            // For each value we detect, we'll clone the evolution object, complete the clone,
            // and add it to the corresponding entity of the decoded taf

            var remainingEvo = chunk;
            var tries = 0;

            foreach (var chunkDecoder in _decoderChain)
            {
                try
                {
                    // we check for probability in each loop, as it can be anywhere
                    remainingEvo = ProbabilityChunkDecoder(evolution, remainingEvo, decodedTaf);

                    // reset cavok
                    _withCavok = false;

                    // try to parse the chunk with the current chunk decoder
                    var decoded = chunkDecoder.Parse(remainingEvo, _withCavok);

                    // map the obtained fields (if any) to a original entity in the decoded_taf
                    var result = decoded[TafDecoder.ResultKey] as Dictionary<string, object>;
                    var entityName = string.Empty;
                    //retrieve first key of result, will require testing
                    foreach (var item in result)
                    {
                        entityName = item.Key;
                        break;
                    }
                    //var entityName = current(array_keys(result));
                    if (entityName == "cavok")
                    {
                        if (result[entityName] is bool && (bool)result[entityName])
                        {
                            _withCavok = true;
                        }
                        entityName = "visibility";
                    }
                    var entity = result[entityName];
                    if (entity == null && entityName != "visibility")
                    {
                        // visibility will be null if cavok is true but we still want to add the evolution
                        throw new TafChunkDecoderException(chunk, remainingEvo, "Bad format for weather evolution", this);
                    }
                    if (entityName == "maxTemperature")
                    {
                        AddEvolution(evolution, decodedTaf, result, "maxTemperature");
                        AddEvolution(evolution, decodedTaf, result, "minTemperature");
                    }
                    else
                    {
                        AddEvolution(evolution, decodedTaf, result, entityName);
                    }

                    // update remaining evo for the next round
                    remainingEvo = decoded[TafDecoder.RemainingTafKey] as string;
                }
                catch (Exception)
                {
                    if (++tries == _decoderChain.Count)
                    {
                        if (IsStrict)
                        {
                            throw new TafChunkDecoderException(chunk, remainingEvo, "Bad format for evolution information", this);
                        }
                        else
                        {
                            // we tried all the chunk decoders on the first chunk and none of them got a match,
                            // so we drop it
                            remainingEvo = Regex.Replace(remainingEvo, "(\\S+\\s+)", string.Empty);
                        }
                    }
                }
            }
            return remainingEvo;
        }

        private string ProbabilityChunkDecoder(Evolution evolution, string chunk, DecodedTaf decodedTaf)
        {
            var regexp = "^(PROB[034]{2}\\s+){1}(TEMPO\\s+){0,1}([0-9]{4}/[0-9]{4}){0,1}(.*)";

            var match = Regex.Match(regexp, chunk);
            if (!match.Success)
            {
                return chunk;
            }

            var prob = match.Groups[1].Value.Trim();
            var type = match.Groups[2].Value.Trim();
            var period = match.Groups[3].Value.Trim();
            var remaining = match.Groups[4].Value.Trim();

            if (prob.StartsWith("PROB"))
            {
                evolution.Probability = prob;
                var embeddedEvolution = new Evolution();
                if (!string.IsNullOrEmpty(type))
                {
                    embeddedEvolution.Type = type;
                }
                else
                {
                    embeddedEvolution.Type = "probability";
                }
                var periodArr = period.Split('/');
                embeddedEvolution.FromDay = Convert.ToInt32(periodArr[0].Substring(0, 2));
                embeddedEvolution.FromTime = periodArr[0].Substring(2, 2) + ":00 UTC";
                embeddedEvolution.ToDay = Convert.ToInt32(periodArr[1].Substring(0, 2));
                embeddedEvolution.ToTime = periodArr[1].Substring(2, 2) + ":00 UTC";
                evolution.Evolutions.Add(embeddedEvolution);
                // recurse on the remaining chunk to extract the weather elements it contains
                chunk = ParseEntitiesChunk(evolution, remaining, decodedTaf);
            }

            return chunk;
        }

        private void AddEvolution(Evolution evolution, DecodedTaf decodedTaf, Dictionary<string, object> result, string entityName)
        {
            // clone the evolution entity
            var newEvolution = (Evolution)evolution.Clone();

            // add the new entity to it
            newEvolution.Entity = result[entityName];

            // possibly add cavok to it
            if (entityName == "visibility" && _withCavok)
            {
                newEvolution.Cavok = true;
            }

            // get the original entity from the decoded taf or a new one decoded taf doesn't contain it yet
            var decodedEntityValue = typeof(DecodedTaf).GetProperty(entityName).GetValue(decodedTaf).ToString();
            AbstractEntity decodedEntity = null;

            //var        getter_name = "get".ucfirst(entityName);
            //var        setter_name = "set".ucfirst(entityName);
            //var        decoded_entity = decodedTaf.$getter_name();

            if (string.IsNullOrEmpty(decodedEntityValue) || entityName == "clouds" || entityName == "weatherPhenomenons")
            {
                // that entity is not in the decoded_taf yet, or it's a cloud layer which is a special case
                decodedEntity = InstantiateEntity(entityName);
            }

            // add the new evolution to that entity
            decodedEntity.Evolutions.Add(newEvolution);
        }

        private AbstractEntity InstantiateEntity(string entityName)
        {
            switch (entityName.ToUpper())
            {
                case "WEATHERPHENOMENONS":
                    return new WeatherPhenomenon();
                case "MAXTEMPERATURE":
                    return new Temperature();
                case "MINTEMPERATURE":
                    return new Temperature();
                case "CLOUDS":
                    return new CloudLayer();
                case "SURFACEWIND":
                    return new SurfaceWind();
                case "VISIBILITY":
                    return new Visibility();
            }
            return null;
        }

        public override Dictionary<string, object> Parse(string remainingTaf, bool withCavok = false)
        {
            throw new NotImplementedException();
        }
    }
}
