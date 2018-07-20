using CommandLine;
using CommandLine.Text;
using csharp_taf_decoder;
using csharp_taf_decoder.entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StartTafDecoder
{
    class Program
    {
        class Options
        {
            [Option("Taf", Required = true, HelpText = "Path to the XML Configuration File.")]
            public string Taf { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            TafDecoder.SetStrictParsing(true);

            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                var decodedTaf = TafDecoder.ParseWithMode(options.Taf);
                Display(decodedTaf);
            }
        }

        private static void Display(object o, string prefix = "")
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(o))
            {
                var name = descriptor.Name;
                var value = descriptor.GetValue(o);

                if (value is ReadOnlyCollection<TafChunkDecoderException>)
                {
                    Console.WriteLine($"{name}.Count={(value as ReadOnlyCollection<TafChunkDecoderException>).Count}");
                }
                else if (value is SurfaceWind)
                {
                    var surfaceWind = value as SurfaceWind;
                    Display(surfaceWind, prefix + descriptor.Name + ".");
                }
                else if (value is Visibility)
                {
                    var visibility = value as Visibility;
                    Display(visibility, prefix + descriptor.Name + ".");
                }
                else if (value is ForecastPeriod)
                {
                    var forecastPeriod = value as ForecastPeriod;
                    Display(forecastPeriod, prefix + descriptor.Name + ".");
                }
                else if (value is Temperature)
                {
                    var temperature = value as Temperature;
                    Display(temperature, prefix + descriptor.Name + ".");
                }
                else if (value is List<WeatherPhenomenon>)
                {
                    var listPresentWeather = value as List<WeatherPhenomenon>;
                    if (listPresentWeather.Count == 0) Console.WriteLine($"{prefix}{descriptor.Name}=<empty>");
                    for (int i = 0; i < listPresentWeather.Count; i++)
                    {
                        var presentWeather = listPresentWeather[i];
                        Display(presentWeather, $"{prefix}{descriptor.Name}[{i}].");
                    }
                }
                else if (value is List<Evolution>)
                {
                    var listEvolution = value as List<Evolution>;
                    if (listEvolution.Count == 0) Console.WriteLine($"{prefix}{descriptor.Name}=<empty>");
                    for (int i = 0; i < listEvolution.Count; i++)
                    {
                        var evolution = listEvolution[i];
                        Display(evolution, $"{prefix}{descriptor.Name}[{i}].");
                    }
                }
                else if (value is List<CloudLayer>)
                {
                    var listCloud = value as List<CloudLayer>;
                    if (listCloud.Count == 0) Console.WriteLine($"{prefix}{descriptor.Name}=<empty>");
                    for (int i = 0; i < listCloud.Count; i++)
                    {
                        var cloud = listCloud[i];
                        Display(cloud, $"{prefix}{descriptor.Name}[{i}].");
                    }
                }
                else if (value is List<string>)
                {
                    Console.WriteLine($"{prefix}{descriptor.Name}={string.Join(", ", value as List<string>)}");
                }
                else
                {
                    Console.WriteLine($"{prefix}{name}={value?.ToString()}");
                }
            }
        }
    }
}
