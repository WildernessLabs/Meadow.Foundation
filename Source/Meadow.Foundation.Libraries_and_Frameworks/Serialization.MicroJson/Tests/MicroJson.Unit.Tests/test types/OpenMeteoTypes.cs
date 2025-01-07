namespace Unit.Tests;

public class MeteoResponse
{
    public float latitude { get; set; }
    public float longitude { get; set; }
    public float generationtime_ms { get; set; }
    public int utc_offset_seconds { get; set; }
    public string timezone { get; set; } = default!;
    public string timezone_abbreviation { get; set; } = default!;
    public float elevation { get; set; }
    public Current_Units current_units { get; set; } = default!;
    public Current current { get; set; } = default!;
    public Hourly_Units hourly_units { get; set; } = default!;
    public Hourly hourly { get; set; } = default!;
    public Daily_Units daily_units { get; set; } = default!;
    public Daily daily { get; set; } = default!;
}

public class Current_Units
{
    public string time { get; set; } = default!;
    public string interval { get; set; } = default!;
    public string temperature_2m { get; set; } = default!;
    public string relative_humidity_2m { get; set; } = default!;
}

public class Current
{
    public string time { get; set; } = default!;
    public int interval { get; set; }
    public float temperature_2m { get; set; }
    public int relative_humidity_2m { get; set; }
}

public class Hourly_Units
{
    public string time { get; set; } = default!;
    public string temperature_2m { get; set; } = default!;
}

public class Hourly
{
    public string[] time { get; set; } = default!;
    public float[] temperature_2m { get; set; } = default!;
}

public class Daily_Units
{
    public string time { get; set; } = default!;
    public string weather_code { get; set; } = default!;
    public string temperature_2m_max { get; set; } = default!;
    public string temperature_2m_min { get; set; } = default!;
}

public class Daily
{
    public string[] time { get; set; } = default!;
    public int[] weather_code { get; set; } = default!;
    public float[] temperature_2m_max { get; set; } = default!;
    public float[] temperature_2m_min { get; set; } = default!;
}