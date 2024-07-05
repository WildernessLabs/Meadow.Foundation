namespace Unit.Tests;

public class WeatherReadingDTOCamelCase
{
    public Coordinates Coord { get; set; }
    public Weather[] Weather { get; set; }
    public WeatherValues Main { get; set; }
    public int Visibility { get; set; }
    public Wind Wind { get; set; }
    public Clouds Clouds { get; set; }
    public int Dt { get; set; }
    public System Sys { get; set; }
    public long Timezone { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cod { get; set; }
}

public class WeatherReadingDTO
{
    public Coordinates coord { get; set; }
    public Weather[] weather { get; set; }
    public WeatherValues main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public int dt { get; set; }
    public System sys { get; set; }
    public long timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; }
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
    public string nain { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
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
    public string country { get; set; }
    public long sunrise { get; set; }
    public long sunset { get; set; }
}