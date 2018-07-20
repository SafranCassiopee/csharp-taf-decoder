﻿using csharp_taf_decoder.entity;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Reflection;

namespace csharp_taf_decoder.chunkdecoder
{
    /// <summary>
    /// Chunk decoder for weather evolutions
    /// </summary>
    public sealed class EvolutionChunkDecoderv1 : TafChunkDecoder
    {
        private const string TypePattern = @"(BECMG\\s+|TEMPO\s+|FM|PROB[34]0\s+){1}";
        private const string PeriodPattern = @"([0-9]{4}/[0-9]{4}\s+|[0-9]{6}\s+){1}";
        private const string RestPattern = @"(.*)";

        private const string ProbabilityPattern = @"^(PROB[34]0\s+){1}(TEMPO\s+){0,1}([0-9]{4}/[0-9]{4}){0,1}(.*)";

        public bool WithCavok { get; private set; }
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
            new WeatherChunkDecoder(),
            new CloudChunkDecoder(),
            new TemperatureChunkDecoder(),
        });

        public EvolutionChunkDecoderv1(bool isStrict, bool withCavok)
        {
            IsStrict = isStrict;
            WithCavok = withCavok;
        }

        public void Parse(string remainingTaf, DecodedTaf decodedTaf)
        {
            string newRemainingTaf;
            var found = Consume(remainingTaf, out newRemainingTaf);

            if (found.Count <= 1)
            {
                // the first chunk didn't match anything, so we remove it to avoid an infinite loop
                Remaining = ConsumeOneChunk(remainingTaf);
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
            remaining = ParseEntitiesChunk(evolution, remaining, decodedTaf);
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
                    WithCavok = false;

                    // try to parse the chunk with the current chunk decoder
                    var decoded = chunkDecoder.Parse(remainingEvo, WithCavok);

                    // map the obtained fields (if any) to a original entity in the decoded_taf
                    var result = decoded[TafDecoder.ResultKey] as Dictionary<string, object>;
                    var entityName = string.Empty;
                    //retrieve first key of result, will require testing
                    foreach (var item in result)
                    {
                        entityName = item.Key;
                        break;
                    }

                    if (entityName == VisibilityChunkDecoder.CavokParameterName)
                    {
                        if (result[entityName] is bool && (bool)result[entityName])
                        {
                            WithCavok = true;
                        }
                        entityName = VisibilityChunkDecoder.VisibilityParameterName;
                    }
                    if (result.ContainsKey(entityName))
                    {
                        if (result[entityName] == null && entityName != VisibilityChunkDecoder.VisibilityParameterName)
                        {
                            // visibility will be null if cavok is true but we still want to add the evolution
                            throw new TafChunkDecoderException(chunk, remainingEvo, "Bad format for weather evolution", this);
                        }
                        if (entityName == TemperatureChunkDecoder.MinimumTemperatureParameterName || entityName == TemperatureChunkDecoder.MaximumTemperatureParameterName)
                        {
                            AddEvolution(evolution, decodedTaf, result, TemperatureChunkDecoder.MaximumTemperatureParameterName);
                            AddEvolution(evolution, decodedTaf, result, TemperatureChunkDecoder.MinimumTemperatureParameterName);
                        }
                        else
                        {
                            AddEvolution(evolution, decodedTaf, result, entityName);
                        }
                    }

                    // update remaining evo for the next round
                    remainingEvo = decoded[TafDecoder.RemainingTafKey] as string;
                }
                catch (Exception ex)
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
                            remainingEvo = ConsumeOneChunk(remainingEvo);
                        }
                    }
                }
            }
            return remainingEvo;
        }

        private string ProbabilityChunkDecoder(Evolution evolution, string chunk, DecodedTaf decodedTaf)
        {
            var match = Regex.Match(chunk, ProbabilityPattern);
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
            if (entityName == VisibilityChunkDecoder.VisibilityParameterName && WithCavok)
            {
                newEvolution.Cavok = true;
            }

            // get the original entity from the decoded taf or a new one decoded taf doesn't contain it yet
            var decodedEntityValue = typeof(DecodedTaf).GetProperty(entityName).GetValue(decodedTaf);
            var decodedEntity = InstantiateEntity(entityName);

            //if (decodedEntityValue != null || entityName == CloudChunkDecoder.CloudsParameterName || entityName == WeatherPhenomenonChunkDecoder.WeatherPhenomenonParameterName)
            //{
            //    // that entity is not in the decoded_taf yet, or it's a cloud layer which is a special case
            //    decodedEntity = InstantiateEntity(entityName);
            //}

            // add the new evolution to that entity
            decodedEntity.Evolutions.Add(newEvolution);

            // update the decoded taf's entity or add the new one to it
            if (entityName == CloudChunkDecoder.CloudsParameterName)
            {
                decodedTaf.Clouds.Add(decodedEntity as CloudLayer);
            }
            else if (entityName == WeatherChunkDecoder.WeatherPhenomenonParameterName)
            {
                decodedTaf.WeatherPhenomenons.Add(decodedEntity as WeatherPhenomenon);
            }
            else
            {
                //TODO
                try
                {
                    var property = typeof(DecodedTaf).GetProperty(entityName);
                    //decodedTaf.GetType().InvokeMember(entityName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, decodedTaf, new object[] { decodedEntity });
                    property.SetValue(decodedTaf, decodedEntity);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        private AbstractEntity InstantiateEntity(string entityName)
        {
            switch (entityName.ToUpper())
            {
                case "WEATHERPHENOMENONS":
                    return new WeatherPhenomenon();
                case "MAXIMUMTEMPERATURE":
                case "MINIMUMTEMPERATURE":
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
