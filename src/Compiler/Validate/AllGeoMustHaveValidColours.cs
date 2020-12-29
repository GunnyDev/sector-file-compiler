﻿using Compiler.Event;
using Compiler.Model;
using Compiler.Error;
using Compiler.Argument;

namespace Compiler.Validate
{
    public class AllGeoMustHaveValidColours : IValidationRule
    {
        public void Validate(SectorElementCollection sectorElements, CompilerArguments args, IEventLogger events)
        {
            foreach (Geo geo in sectorElements.GeoElements)
            {
                ValidateBaseGeo(geo, sectorElements, events);
                foreach (GeoSegment segment in geo.AdditionalSegments)
                {
                    ValidateGeoSegment(segment, sectorElements, events);
                }
            }
        }

        private static void ValidateBaseGeo(Geo geo, SectorElementCollection sectorElements, IEventLogger events)
        {
            if (!ColourValid(geo.Colour, sectorElements))
            {
                string errorMessage = string.Format(
                    "Invalid colour value {0} in GEO declaration {1}",
                    geo.Colour,
                    geo.GetCompileData(sectorElements)
                );
                events.AddEvent(new ValidationRuleFailure(errorMessage));
            }
        }

        private static void ValidateGeoSegment(GeoSegment segment, SectorElementCollection sectorElements, IEventLogger events)
        {
            if (!ColourValid(segment.Colour, sectorElements))
            {
                string errorMessage = string.Format(
                    "Invalid colour value {0} in GEO segment {1}",
                    segment.Colour,
                    segment.GetCompileData(sectorElements)
                );
                events.AddEvent(new ValidationRuleFailure(errorMessage));
            }
        }

        private static bool ColourValid(string colour, SectorElementCollection sectorElements)
        {
            return colour == null || ColourValidator.ColourValid(sectorElements, colour);
        }
    }
}
