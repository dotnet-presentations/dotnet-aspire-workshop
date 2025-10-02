using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public record Zone(string Key, string Name, string State, string[] ForecastOffices, string[] TimeZone, string[] ObservationStations);

public record Forecast(string Name, string DetailedForecast);