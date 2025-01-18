namespace Unit.Tests;

public class WeatherReadingDTOCamelCase
{
    public Coordinates Coord { get; set; } = default!;
    public Weather[] Weather { get; set; } = default!;
    public WeatherValues Main { get; set; } = default!;
    public int Visibility { get; set; }
    public Wind Wind { get; set; } = default!;
    public Clouds Clouds { get; set; } = default!;
    public int Dt { get; set; }
    public System Sys { get; set; } = default!;
    public long Timezone { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Cod { get; set; }
}

public class WeatherReadingDTO
{
    public Coordinates coord { get; set; } = default!;
    public Weather[] weather { get; set; } = default!;
    public WeatherValues main { get; set; } = default!;
    public int visibility { get; set; } = default!;
    public Wind wind { get; set; } = default!;
    public Clouds clouds { get; set; } = default!;
    public int dt { get; set; }
    public System sys { get; set; } = default!;
    public long timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int cod { get; set; }
}

public class Coordinates
{
    public double lon { get; set; }
    public double lat { get; set; }
}

public class Weather
{
    public int id { get; set; }
    public string nain { get; set; } = default!;
    public string description { get; set; } = default!;
    public string icon { get; set; } = default!;
}

public class WeatherValues
{
    public double temp { get; set; }
    public double feels_like { get; set; }
    public double temp_min { get; set; }
    public double temp_max { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }
}

public class Wind
{
    public decimal speed { get; set; }
    public int deg { get; set; }
    public double gust { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class System
{
    public int Type { get; set; }
    public int Id { get; set; }
    public string country { get; set; } = default!;
    public long sunrise { get; set; }
    public long sunset { get; set; }
}