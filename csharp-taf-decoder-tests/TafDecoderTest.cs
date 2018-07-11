
using csharp_taf_decoder;
using csharp_taf_decoder.entity;
using NUnit.Framework;
using System.Linq;
using static csharp_taf_decoder.entity.DecodedTaf;

[TestFixture, Category("Entity")]
public class TafDecoderTest
{
    /// <summary>
    /// Test parsing of a valid TAF
    /// </summary>
    [Test]
    public void TestParse()
    {
        var rawTaf = "TAF TAF LIRU 032244Z 0318/0406 23010KT P6SM -SHDZRA BKN020CB TX05/0318Z TNM03/0405Z";
        var decoderTaf = TafDecoder.ParseWithMode(rawTaf);
        Assert.IsTrue(decoderTaf.IsValid);
        Assert.AreEqual("TAF TAF LIRU 032244Z 0318/0406 23010KT P6SM -SHDZRA BKN020CB TX05/0318Z TNM03/0405Z", decoderTaf.RawTaf);
        Assert.AreEqual(TafType.TAF, decoderTaf.Type);
        Assert.AreEqual("LIRU", decoderTaf.Icao);
        Assert.AreEqual(3, decoderTaf.Day);
        Assert.AreEqual("22:44 UTC", decoderTaf.Time);

        var forecastPeriod = decoderTaf.ForecastPeriod;
        Assert.AreEqual(3, forecastPeriod.FromDay);
        Assert.AreEqual(18, forecastPeriod.FromHour);
        Assert.AreEqual(4, forecastPeriod.ToDay);
        Assert.AreEqual(6, forecastPeriod.ToHour);

        var surfaceWind = decoderTaf.SurfaceWind;
        Assert.IsFalse(surfaceWind.VariableDirection);
        Assert.AreEqual(230, surfaceWind.MeanDirection.ActualValue);
        Assert.AreEqual(Value.Unit.Degree, surfaceWind.MeanDirection.ActualUnit);
        Assert.IsNull(surfaceWind.DirectionVariations);
        Assert.AreEqual(10, surfaceWind.MeanSpeed.ActualValue);
        Assert.AreEqual(Value.Unit.Knot, surfaceWind.MeanSpeed.ActualUnit);
        Assert.Null(surfaceWind.SpeedVariations);

        var visibility = decoderTaf.Visibility;
        //TODO: Fix
        //Assert.AreEqual(6, visibility.ActualVisibility.ActualValue);
        Assert.AreEqual(Value.Unit.StatuteMile, visibility.ActualVisibility.ActualUnit);
        Assert.True(visibility.Greater);

        var weatherPhenomenon = decoderTaf.WeatherPhenomenons;
        Assert.AreEqual("-", weatherPhenomenon[0].IntensityProximity);
        Assert.AreEqual("SH", weatherPhenomenon[0].Descriptor);
        var phenomena = weatherPhenomenon[0].Phenomena;
        Assert.AreEqual("DZ", phenomena[0]);
        Assert.AreEqual("RA", phenomena[1]);

        var cloud = decoderTaf.Clouds.FirstOrDefault();
        Assert.AreEqual(CloudLayer.CloudAmount.BKN, cloud.Amount);
        Assert.AreEqual(2000, cloud.BaseHeight.ActualValue);
        Assert.AreEqual(Value.Unit.Feet, cloud.BaseHeight.ActualUnit);
        Assert.AreEqual(CloudLayer.CloudType.CB, cloud.Type);

        var minimumTemperature = decoderTaf.MinimumTemperature;
        Assert.AreEqual(-3, minimumTemperature.TemperatureValue.ActualValue);
        Assert.AreEqual(Value.Unit.DegreeCelsius, minimumTemperature.TemperatureValue.ActualUnit);
        Assert.AreEqual(4, minimumTemperature.Day);
        Assert.AreEqual(5, minimumTemperature.Hour);

        var maximumTemperature = decoderTaf.MaximumTemperature;
        Assert.AreEqual(5, maximumTemperature.TemperatureValue.ActualValue);
        Assert.AreEqual(Value.Unit.DegreeCelsius, maximumTemperature.TemperatureValue.ActualUnit);
        Assert.AreEqual(3, maximumTemperature.Day);
        Assert.AreEqual(18, maximumTemperature.Hour);
    }
    /// <summary>
    /// Test parsing of a valid TAF
    /// </summary>
    [Test]
    public void TestParseSecond()
    {
        var rawTaf = "TAF TAF LIRU 032244Z 0318/0406 23010KT P6SM +TSRA FG BKN020CB TX05/0318Z TNM03/0405Z";
        var decoderTaf = TafDecoder.ParseWithMode(rawTaf);
        Assert.True(decoderTaf.IsValid);
        Assert.AreEqual("TAF TAF LIRU 032244Z 0318/0406 23010KT P6SM +TSRA FG BKN020CB TX05/0318Z TNM03/0405Z", decoderTaf.RawTaf);
        Assert.AreEqual(TafType.TAF, decoderTaf.Type);
        Assert.AreEqual("LIRU", decoderTaf.Icao);
        Assert.AreEqual(3, decoderTaf.Day);
        Assert.AreEqual("22:44 UTC", decoderTaf.Time);

        var forecastPeriod = decoderTaf.ForecastPeriod;
        Assert.AreEqual(3, forecastPeriod.FromDay);
        Assert.AreEqual(18, forecastPeriod.FromHour);
        Assert.AreEqual(4, forecastPeriod.ToDay);
        Assert.AreEqual(6, forecastPeriod.ToHour);

        var surfaceWind = decoderTaf.SurfaceWind;
        Assert.False(surfaceWind.VariableDirection);
        Assert.AreEqual(230, surfaceWind.MeanDirection.ActualValue);
        Assert.AreEqual(Value.Unit.Degree, surfaceWind.MeanDirection.ActualUnit);
        Assert.Null(surfaceWind.DirectionVariations);
        Assert.AreEqual(10, surfaceWind.MeanSpeed.ActualValue);
        Assert.AreEqual(Value.Unit.Knot, surfaceWind.MeanSpeed.ActualUnit);
        Assert.Null(surfaceWind.SpeedVariations);

        var visibility = decoderTaf.Visibility;
        //TODO: Fix
        //Assert.AreEqual(6, visibility.ActualVisibility.ActualValue);
        Assert.AreEqual(Value.Unit.StatuteMile, visibility.ActualVisibility.ActualUnit);
        Assert.True(visibility.Greater);

        var weatherPhenomenons = decoderTaf.WeatherPhenomenons;
        Assert.AreEqual("+", weatherPhenomenons[0].IntensityProximity);
        Assert.AreEqual("TS", weatherPhenomenons[0].Descriptor);
        var phenomena = weatherPhenomenons[0].Phenomena;
        Assert.AreEqual("RA", phenomena[0]);
        phenomena = weatherPhenomenons[1].Phenomena;
        Assert.AreEqual("FG", phenomena[0]);

        var cloud = decoderTaf.Clouds[0];
        Assert.AreEqual(CloudLayer.CloudAmount.BKN, cloud.Amount);
        Assert.AreEqual(2000, cloud.BaseHeight.ActualValue);
        Assert.AreEqual(Value.Unit.Feet, cloud.BaseHeight.ActualUnit);
        Assert.AreEqual(CloudLayer.CloudType.CB, cloud.Type);

        var minimumTemperature = decoderTaf.MinimumTemperature;
        Assert.AreEqual(-3, minimumTemperature.TemperatureValue.ActualValue);
        Assert.AreEqual(Value.Unit.DegreeCelsius, minimumTemperature.TemperatureValue.ActualUnit);
        Assert.AreEqual(4, minimumTemperature.Day);
        Assert.AreEqual(5, minimumTemperature.Hour);

        var maximumTemperature = decoderTaf.MaximumTemperature;
        Assert.AreEqual(5, maximumTemperature.TemperatureValue.ActualValue);
        Assert.AreEqual(Value.Unit.DegreeCelsius, maximumTemperature.TemperatureValue.ActualUnit);
        Assert.AreEqual(3, maximumTemperature.Day);
        Assert.AreEqual(18, maximumTemperature.Hour);
    }
    /// <summary>
    /// Test parsing of a short, invalid TAF, without strict option activated
    /// </summary>
    [Test]
    public void TestParseInvalid()
    {
        // launch decoding (forecast was cancelled)
        var d = TafDecoder.ParseNotStrict("TAF LFMT 032244Z 0318/0206 CNL");
        Assert.IsFalse(d.IsValid);
        // launch decoding (surface wind is invalid)
        d = TafDecoder.ParseNotStrict("TAF TAF LIRU 032244Z 0318/0420 2300ABKT PSSM\nBKN020CB TX05/0318Z TNM03/0405Z\n");
        Assert.IsFalse(d.IsValid);
    }
    /// <summary>
    /// Test object-wide strict option
    /// </summary>
    [Test]
    public void TestParseDefaultStrictMode()
    {
        // strict mode, max 1 error triggered
        TafDecoder.SetStrictParsing(true);
        var d = TafDecoder.Parse("TAF TAF LIR 032244Z 0318/0206 23010KT P6SM BKN020CB TX05/0318Z TNM03/0405Z\n");
        Assert.AreEqual(1, d.DecodingExceptions.Count);
        // not strict: several errors triggered (6 because the icao failure causes the next ones to fail too)
        TafDecoder.SetStrictParsing(false);
        d = TafDecoder.Parse("TAF TAF LIR 032244Z 0318/0206 23010KT\n");
        //TODO: Fix
        //Assert.AreEqual(6, d.DecodingExceptions.Count);
    }
    /// <summary>
    /// Test parsing of invalid TAFs
    /// </summary>
    [Test]
    public void TestParseErrors()
    {
        //error_dataset = array(
        //    array("TAF LFPG aaa bbb cccc", "DatetimeChunkDecoder", "AAA BBB CCCC END"),
        //    array("TAF LFPO 231027Z NIL 1234", "ForecastPeriodChunkDecoder", "NIL 1234 END"),
        //    array("TAF LFPO 231027Z 2310/2411 NIL 12345 ", "SurfaceWindChunkDecoder", "NIL 12345 END"),
        //);
        //foreach (error_dataset as taf_error)
        //{
        //    // launch decoding
        //    d = this.decoder.parseNotStrict(taf_error[0]);
        //    // check the error triggered
        //    Assert.False(d.isValid);
        //    errors = d.DecodingExceptions;
        //    first_error = errors[0];
        //    Assert.AreEqual(taf_error[1], first_error.ChunkDecoder);
        //    Assert.AreEqual(taf_error[2], first_error.Chunk);
        //    d.resetDecodingExceptions;
        //    Assert.Empty(d.DecodingExceptions);
        //}
    }
    /// <summary>
    /// Test invalid values
    /// </summary>
    [Test]
    public void TestValueErrors()
    {
        //var newValue = new Value(null, null);
        // Assert.Null(newValue.ActualValue);
        // Assert.Null(newValue.newValue(null, null));
        // Assert.Null(newValue.newIntValue("AB", null).ActualValue);
    }
}
