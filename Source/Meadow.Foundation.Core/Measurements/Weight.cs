namespace Meadow.Foundation
{
    public class Weight : Measurement<WeightUnits>
    {
        private decimal _weightInGrams;

        public WeightUnits StandardUnits { get => WeightUnits.Grams; }

        public Weight(decimal value, WeightUnits units)
        {
            StandardValue = Convert(value, units, WeightUnits.Grams);
        }

        public override decimal ConvertTo(WeightUnits units)
        {
            return Convert(StandardValue, WeightUnits.Grams, units);
        }

        public static decimal Convert(decimal value, WeightUnits fromUnits, WeightUnits toUnits)
        { 
            var factor = 1.0m;

            switch (fromUnits)
            {
                case WeightUnits.Carats:
                    switch (toUnits)
                    {
                        case WeightUnits.Grains: factor = 3.08647167m; break;
                        case WeightUnits.Grams: factor = 0.20m; break;
                        case WeightUnits.Kilograms: factor = 0.0002m; break;
                        case WeightUnits.Ounces: factor = 0.00705479m; break;
                        case WeightUnits.Pounds: factor = 0.00044092m; break;
                    }
                    break;
                case WeightUnits.Grains:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 0.32399455m; break;
                        case WeightUnits.Grams: factor = 0.06479891m; break;
                        case WeightUnits.Kilograms: factor = 0.0000648m; break;
                        case WeightUnits.Ounces: factor = 0.00228571m; break;
                        case WeightUnits.Pounds: factor = 0.00014286m; break;
                    }
                    break;
                case WeightUnits.Grams:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 5.0m; break;
                        case WeightUnits.Grains: factor = 15.4323584m; break;
                        case WeightUnits.Kilograms: factor = 0.001m; break;
                        case WeightUnits.Ounces: factor = 0.03527396m; break;
                        case WeightUnits.Pounds: factor = 0.00220462m; break;
                    }
                    break;
                case WeightUnits.Kilograms:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 5000m; break;
                        case WeightUnits.Grains: factor = 15432.3584m; break;
                        case WeightUnits.Grams: factor = 1000m; break;
                        case WeightUnits.Ounces: factor = 35.27396m; break;
                        case WeightUnits.Pounds: factor = 2.20462m; break;
                    }
                    break;
                case WeightUnits.Ounces:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 141.747615m; break;
                        case WeightUnits.Grains: factor = 437.5m; break;
                        case WeightUnits.Grams: factor = 28.3495231m; break;
                        case WeightUnits.Kilograms: factor = 0.2834952m; break;
                        case WeightUnits.Pounds: factor = 0.0625m; break;
                    }
                    break;
                case WeightUnits.Pounds:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 2267.96185m; break;
                        case WeightUnits.Grains: factor = 6999.99999m; break;
                        case WeightUnits.Grams: factor = 453.59237m; break;
                        case WeightUnits.Kilograms: factor = 0.45359237m; break;
                        case WeightUnits.Ounces: factor = 16m; break;
                    }
                    break;
            }

            return value * factor;
        }
    }
}