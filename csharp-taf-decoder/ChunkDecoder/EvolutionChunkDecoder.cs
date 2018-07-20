using csharp_taf_decoder.entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csharp_taf_decoder.chunkdecoder
{
    public sealed class EvolutionChunkDecoder : TafChunkDecoder
    {
        public const string ProbabilityParameterName = "Probability";

        public override string GetRegex()
        {
            var type = @"(BECMG\s+|TEMPO\s+|FM|PROB[34]0\s+){1}";
            var period = @"([0-9]{4}/[0-9]{4}\s+|[0-9]{6}\s+){1}";
            var rest = @"(.*)";
            return $"{type}{period}{rest}";
        }

        private static ReadOnlyCollection<TafChunkDecoder> _decoderChain = new ReadOnlyCollection<TafChunkDecoder>(new List<TafChunkDecoder>()
        {
            new SurfaceWindChunkDecoder(),
            new VisibilityChunkDecoder(),
            new WeatherChunkDecoder(),
            new CloudChunkDecoder(),
            new TemperatureChunkDecoder(),
        });

        // Logic >_<
        public bool IsStrict { private get; set; }
        private bool _with_cavok;

        public string Remaining { get; private set; }

        public EvolutionChunkDecoder(bool strict, bool with_cavok)
        {
            IsStrict = strict;
            _with_cavok = with_cavok;
        }

        public void Parse(string remaining_taf, DecodedTaf decoded_taf)
        {
            string newRemainingTaf;
            var found = Consume(remaining_taf, out newRemainingTaf);

            if (found.Count <= 1)
            {
                // the first chunk didn't match anything, so we remove it to avoid an infinite loop
                Remaining = ConsumeOneChunk(remaining_taf);
                return;
            }

            var evo_type = found[1].Value.Trim();
            var evo_period = found[2].Value.Trim();
            var remaining = found[3].Value;


            var evolution = new Evolution() { Type = evo_type };
            //php: if (strpos($result['remaining'], 'PROB') !== false) {
            if (remaining.Contains("PROB"))
            {
                // if the line started with PROBnn it won't have been consumed and we'll find it in remaining
                // LDA: Je ne suis pas d'accord avec ce commentaire, ce n'est pas ce que semble faire le code PHP
                // le code fait un "Contains" alors que le commentaire suggère un "StartsWith"
                evolution.Probability = remaining.Trim();
            }

            // period
            if (evo_type == "BECMG" || evo_type == "TEMPO")
            {
                var periodArr = evo_period.Split('/');
                evolution.FromDay = Convert.ToInt32(periodArr[0].Substring(0, 2));
                evolution.FromTime = periodArr[0].Substring(2, 2) + ":00 UTC";
                evolution.ToDay = Convert.ToInt32(periodArr[1].Substring(0, 2));
                evolution.ToTime = periodArr[1].Substring(2, 2) + ":00 UTC";
            }
            else
            {
                evolution.FromDay = Convert.ToInt32(evo_period.Substring(0, 2));
                evolution.FromTime = evo_period.Substring(2, 2) + ':' + evo_period.Substring(4, 2) + " UTC";
            }

            // rest
            remaining = ParseEntitiesChunk(evolution, remaining, decoded_taf);
        }

        /// <summary>
        /// Extract the weather elements (surface winds, visibility, etc) between 2 evolution tags (BECMG, TEMPO or FM)
        /// </summary>
        /// <param name="evolution"></param>
        /// <param name="remaining"></param>
        /// <param name="decoded_taf"></param>
        /// <returns></returns>
        private string ParseEntitiesChunk(Evolution evolution, string chunk, DecodedTaf decoded_taf)
        {
            // For each value we detect, we'll clone the evolution object, complete the clone,
            // and add it to the corresponding entity of the decoded taf

            var remaining_evo = chunk;
            var tries = 0;

            // call each decoder in the chain and use results to populate the decoded taf
            foreach (var chunk_decoder in _decoderChain)
            {
                try
                {
                    // we check for probability in each loop, as it can be anywhere
                    remaining_evo = ProbabilityChunkDecoder(evolution, remaining_evo, decoded_taf);

                    // reset cavok
                    _with_cavok = false;

                    // try to parse the chunk with the current chunk decoder
                    var decoded = chunk_decoder.Parse(remaining_evo, _with_cavok);

                    // map the obtained fields (if any) to a original entity in the decoded_taf
                    var result = decoded[TafDecoder.ResultKey] as Dictionary<string, object>;
                    // LDA: l'idée c'est de trouver la "clef" de l'objet récupéré
                    var entity_name = result.Keys.FirstOrDefault();
                    if (entity_name == VisibilityChunkDecoder.CavokParameterName)
                    {
                        if ((bool)result[entity_name])
                        {
                            _with_cavok = true;
                        }
                        entity_name = VisibilityChunkDecoder.VisibilityParameterName;
                    }
                    var entity = result[entity_name];

                    if (entity == null && entity_name != VisibilityChunkDecoder.VisibilityParameterName)
                    {
                        // visibility will be null if cavok is true but we still want to add the evolution
                        throw new TafChunkDecoderException(chunk, remaining_evo, TafChunkDecoderException.Messages.WeatherEvolutionBadFormat, this);
                    }
                    if (entity_name == TemperatureChunkDecoder.MaximumTemperatureParameterName || entity_name == TemperatureChunkDecoder.MinimumTemperatureParameterName)
                    {
                        AddEvolution(decoded_taf, evolution, result, TemperatureChunkDecoder.MaximumTemperatureParameterName);
                        AddEvolution(decoded_taf, evolution, result, TemperatureChunkDecoder.MinimumTemperatureParameterName);
                    }
                    else
                    {
                        AddEvolution(decoded_taf, evolution, result, entity_name);
                    }

                    // update remaining evo for the next round
                    remaining_evo = (string)decoded[TafDecoder.RemainingTafKey];
                }
                catch (Exception ex)
                {
                    if (++tries == _decoderChain.Count)
                    {
                        if (IsStrict)
                        {
                            throw new TafChunkDecoderException(chunk, remaining_evo, TafChunkDecoderException.Messages.EvolutionInformationBadFormat, this);
                        }
                        else
                        {
                            // we tried all the chunk decoders on the first chunk and none of them got a match,
                            // so we drop it
                            remaining_evo = ConsumeOneChunk(remaining_evo);
                        }
                    }
                }
            }
            return remaining_evo;
        }

        private void AddEvolution(DecodedTaf decoded_taf, Evolution evolution, Dictionary<string, object> result, string entity_name)
        {
            // clone the evolution entity
            var new_evolution = evolution.Clone() as Evolution;

            // add the new entity to it
            new_evolution.Entity = result[entity_name];

            if (entity_name == VisibilityChunkDecoder.VisibilityParameterName && _with_cavok)
            {
                new_evolution.Cavok = true;
            }

            // get the original entity from the decoded taf or a new one decoded taf doesn't contain it yet
            AbstractEntity decoded_entity = typeof(DecodedTaf).GetProperty(entity_name).GetValue(decoded_taf) as AbstractEntity;

            if (decoded_entity == null || entity_name == CloudChunkDecoder.CloudsParameterName || entity_name == WeatherChunkDecoder.WeatherPhenomenonParameterName)
            {
                // that entity is not in the decoded_taf yet, or it's a cloud layer which is a special case
                decoded_entity = InstantiateEntity(entity_name);
            }

            // add the new evolution to that entity
            decoded_entity.Evolutions.Add(new_evolution);

            // update the decoded taf's entity or add the new one to it
            switch (entity_name)
            {
                case CloudChunkDecoder.CloudsParameterName:
                    decoded_taf.Clouds.Add(decoded_entity as CloudLayer);
                    break;
                case WeatherChunkDecoder.WeatherPhenomenonParameterName:
                    decoded_taf.WeatherPhenomenons.Add(decoded_entity as WeatherPhenomenon);
                    break;
                case VisibilityChunkDecoder.VisibilityParameterName:
                    decoded_taf.Visibility = decoded_entity as Visibility;
                    break;
                case SurfaceWindChunkDecoder.SurfaceWindParameterName:
                    decoded_taf.SurfaceWind = decoded_entity as SurfaceWind;
                    break;
                case TemperatureChunkDecoder.MaximumTemperatureParameterName:
                    decoded_taf.MaximumTemperature = decoded_entity as Temperature;
                    break;
                case TemperatureChunkDecoder.MinimumTemperatureParameterName:
                    decoded_taf.MinimumTemperature = decoded_entity as Temperature;
                    break;
                default:
                    throw new TafChunkDecoderException(TafChunkDecoderException.Messages.UnknownEntity + decoded_entity.ToString());
            }
        }

        private AbstractEntity InstantiateEntity(string entity_name)
        {
            switch (entity_name)
            {
                case WeatherChunkDecoder.WeatherPhenomenonParameterName:
                    return new WeatherPhenomenon();
                case TemperatureChunkDecoder.MinimumTemperatureParameterName:
                case TemperatureChunkDecoder.MaximumTemperatureParameterName:
                    return new Temperature();
                case CloudChunkDecoder.CloudsParameterName:
                    return new CloudLayer();
                case SurfaceWindChunkDecoder.SurfaceWindParameterName:
                    return new SurfaceWind();
                case VisibilityChunkDecoder.VisibilityParameterName:
                    return new Visibility();
                default:
                    throw new TafChunkDecoderException(TafChunkDecoderException.Messages.UnknownEntity + entity_name);
            }
        }

        /// <summary>
        /// Look recursively for probability (PROBnn) attributes and embed a new evolution object one level deeper for each
        /// </summary>
        /// <param name="evolution"></param>
        /// <param name="remaining_evo"></param>
        /// <param name="decoded_taf"></param>
        /// <returns></returns>
        private string ProbabilityChunkDecoder(Evolution evolution, string chunk, DecodedTaf decoded_taf)
        {
            var regexp = @"^(PROB[34]0\s+){1}(TEMPO\s+){0,1}([0-9]{4}/[0-9]{4}){0,1}(.*)";
            var matches = Regex.Matches(chunk, regexp);
            List<Match> found = null;

            if (matches.Count > 0)
            {
                found = matches.Cast<Match>().ToList();
            }
            else
            {
                return chunk;
            }

            var prob = found[1].Value.Trim();
            var type = found[2].Value.Trim();
            var period = found[3].Value.Trim();
            var remaining = found[4].Value.Trim();

            if (prob.Contains("PROB"))
            {
                // LDA: même problèmatique
                evolution.Probability = prob;
                var embeddedEvolution = new Evolution() { Type = string.IsNullOrEmpty(type) ? type : ProbabilityParameterName };

                var periodArr = period.Split('/');
                embeddedEvolution.FromDay = Convert.ToInt32(periodArr[0].Substring(0, 2));
                embeddedEvolution.FromTime = periodArr[0].Substring(2, 2) + ":00 UTC";
                embeddedEvolution.ToDay = Convert.ToInt32(periodArr[1].Substring(0, 2));
                embeddedEvolution.ToTime = periodArr[1].Substring(2, 2) + ":00 UTC";

                evolution.Evolutions.Add(embeddedEvolution);
                // recurse on the remaining chunk to extract the weather elements it contains
                chunk = ParseEntitiesChunk(evolution, remaining, decoded_taf);
            }

            return string.Empty;
        }

        /// <summary>
        /// Not implemented because EvolutionChunkDecoder is not part of the decoder chain
        /// </summary>
        /// <param name="remainingTaf"></param>
        /// <param name="withCavok"></param>
        /// <returns></returns>
        public override Dictionary<string, object> Parse(string remainingTaf, bool withCavok = false)
        {
            throw new NotImplementedException();
        }
    }
}
