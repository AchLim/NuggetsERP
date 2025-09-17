using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Seed;

public static class UomSeeder
{
    public static async Task SeedUomsAsync(NuggetsDbContext dbContext)
    {
        var uoms = new Dictionary<string, UnitOfMeasure>();

        void AddUom(string name, string abbr)
        {
            var existing = dbContext.Uoms.FirstOrDefault(u => u.Abbreviation == abbr);
            if (existing == null)
            {
                var uom = new UnitOfMeasure
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Abbreviation = abbr
                };
                dbContext.Uoms.Add(uom);
                uoms[abbr] = uom;
            }
            else
            {
                uoms[abbr] = existing;
            }
        }

        // -------- INVENTORY COMMON UNITS --------
        AddUom("Piece", "pcs");
        AddUom("Dozen", "doz");
        AddUom("Pair", "pair");
        AddUom("Bottle", "btl");
        AddUom("Can", "can");
        AddUom("Bag", "bag");
        AddUom("Roll", "roll");
        AddUom("Sheet", "sht");

        // -------- MASS/WEIGHT --------
        AddUom("Gram", "g");
        AddUom("Kilogram", "kg");
        AddUom("Milligram", "mg");
        AddUom("Ton", "t");
        AddUom("Pound", "lb");
        AddUom("Ounce", "oz");

        // -------- VOLUME --------
        AddUom("Liter", "L");
        AddUom("Milliliter", "ml");
        AddUom("Cubic Meter", "m3");
        AddUom("Gallon (US)", "gal");
        AddUom("Fluid Ounce (US)", "fl oz");

        // -------- LENGTH --------
        AddUom("Meter", "m");
        AddUom("Centimeter", "cm");
        AddUom("Millimeter", "mm");
        AddUom("Kilometer", "km");
        AddUom("Inch", "in");
        AddUom("Foot", "ft");
        AddUom("Yard", "yd");
        AddUom("Mile", "mi");

        // -------- AREA --------
        AddUom("Square Meter", "m2");
        AddUom("Square Kilometer", "km2");
        AddUom("Hectare", "ha");
        AddUom("Acre", "ac");

        await dbContext.SaveChangesAsync();

        if (dbContext.UomConversions.Any()) return;

        var conversions = new List<UnitOfMeasureConversion>
        {
            // ---- Inventory conversions ----
            new() { Id = Guid.NewGuid(), FromUomId = uoms["doz"].Id, ToUomId = uoms["pcs"].Id, ConversionRate = 12m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["pair"].Id, ToUomId = uoms["pcs"].Id, ConversionRate = 2m, IsBidirectional = true },

            // ---- Weight ----
            new() { Id = Guid.NewGuid(), FromUomId = uoms["kg"].Id, ToUomId = uoms["g"].Id, ConversionRate = 1000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["mg"].Id, ToUomId = uoms["g"].Id, ConversionRate = 0.001m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["t"].Id, ToUomId = uoms["kg"].Id, ConversionRate = 1000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["lb"].Id, ToUomId = uoms["g"].Id, ConversionRate = 453.59237m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["oz"].Id, ToUomId = uoms["g"].Id, ConversionRate = 28.34952m, IsBidirectional = true },

            // ---- Volume ----
            new() { Id = Guid.NewGuid(), FromUomId = uoms["ml"].Id, ToUomId = uoms["L"].Id, ConversionRate = 0.001m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["m3"].Id, ToUomId = uoms["L"].Id, ConversionRate = 1000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["gal"].Id, ToUomId = uoms["L"].Id, ConversionRate = 3.78541m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["fl oz"].Id, ToUomId = uoms["ml"].Id, ConversionRate = 29.5735m, IsBidirectional = true },

            // ---- Length ----
            new() { Id = Guid.NewGuid(), FromUomId = uoms["cm"].Id, ToUomId = uoms["m"].Id, ConversionRate = 0.01m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["mm"].Id, ToUomId = uoms["m"].Id, ConversionRate = 0.001m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["km"].Id, ToUomId = uoms["m"].Id, ConversionRate = 1000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["in"].Id, ToUomId = uoms["cm"].Id, ConversionRate = 2.54m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["ft"].Id, ToUomId = uoms["in"].Id, ConversionRate = 12m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["yd"].Id, ToUomId = uoms["ft"].Id, ConversionRate = 3m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["mi"].Id, ToUomId = uoms["ft"].Id, ConversionRate = 5280m, IsBidirectional = true },

            // ---- Area ----
            new() { Id = Guid.NewGuid(), FromUomId = uoms["km2"].Id, ToUomId = uoms["m2"].Id, ConversionRate = 1_000_000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["ha"].Id, ToUomId = uoms["m2"].Id, ConversionRate = 10_000m, IsBidirectional = true },
            new() { Id = Guid.NewGuid(), FromUomId = uoms["ac"].Id, ToUomId = uoms["m2"].Id, ConversionRate = 4046.86m, IsBidirectional = true },
        };

        dbContext.UomConversions.AddRange(conversions);
        await dbContext.SaveChangesAsync();
    }
}