﻿using System;
using System.Collections.Generic;
using Compiler.Model;
using Compiler.Event;
using Compiler.Error;
using Compiler.Input;
using System.Linq;

namespace Compiler.Parser
{
    public class RegionParser : ISectorDataParser
    {
        private readonly SectorElementCollection elements;
        private readonly IEventLogger eventLogger;
        const string regionNameDeclaration = "REGIONNAME";

        public RegionParser(
            SectorElementCollection elements,
            IEventLogger eventLogger
        ) {
            this.elements = elements;
            this.eventLogger = eventLogger;
        }

        public void ParseData(AbstractSectorDataFile data)
        {
            List<RegionPoint> points = new List<RegionPoint>();
            bool foundFirst = false;
            string regionName = "";
            bool expectingColourDefinition = false;
            bool expectingRegionNameDefinition = true;
            SectorData declarationLine = new SectorData();
            foreach (SectorData line in data)
            {
                // Check for the first REGIONNAME
                if (expectingRegionNameDefinition && (line.dataSegments.Count < 2 || !line.rawData.StartsWith(RegionParser.regionNameDeclaration)))
                {
                    this.eventLogger.AddEvent(
                        new SyntaxError("Invalid first line of region " + data.CurrentLine, line)
                    );
                    return;
                }

                if (expectingColourDefinition && line.dataSegments.Count != 3)
                {
                    this.eventLogger.AddEvent(
                        new SyntaxError("Unexpected colour definition " + data.CurrentLine, line)
                    );
                    return;
                }

                // We've found our first region name if we get here for the first time, so we don't demand another in this file
                expectingRegionNameDefinition = false;

                // Expecting a colour definition for the region
                if (expectingColourDefinition) {
                    if (line.dataSegments.Count != 3)
                    {
                        this.eventLogger.AddEvent(
                            new SyntaxError("All regions must have a colour " + data.CurrentLine, line)
                        );
                        return;
                    }

                    // Setup a new segment
                    Point parsedNewPoint = PointParser.Parse(line.dataSegments[1], line.dataSegments[2]);
                    if (parsedNewPoint.Equals(PointParser.invalidPoint))
                    {
                        this.eventLogger.AddEvent(
                            new SyntaxError("Invalid region point format: " + data.CurrentLine, line)
                        );
                        return;
                    }

                    points.Add(
                        new RegionPoint(
                            parsedNewPoint,
                            line.definition,
                            line.docblock,
                            line.inlineComment,
                            line.dataSegments[0]
                        )
                    );
                    expectingColourDefinition = false;
                    continue;
                }


                /**
                 * We've found a new REGIONNAME declaration, so must be the start of a new region.
                 */
                if (line.rawData.StartsWith(RegionParser.regionNameDeclaration))
                {
                    // If it's not the first in the data stream, then it's a new declaration, save the previous
                    if (foundFirst)
                    {
                        this.elements.Add(
                            new Region(
                                regionName,
                                points,
                                declarationLine.definition,
                                declarationLine.docblock,
                                declarationLine.inlineComment
                            )
                        );

                        // Clear the segments array
                        points = new List<RegionPoint>();
                    } else {
                        foundFirst = true;
                    }

                    // Save the new declaration line
                    declarationLine = line;
                    regionName = string.Join(" ", line.dataSegments.Skip(1));

                    // Immediately after REGIONNAME, we should expect a colour defition
                    expectingColourDefinition = true;
                    continue;
                }

                Point parsedPoint = PointParser.Parse(line.dataSegments[0], line.dataSegments[1]);
                if (parsedPoint == PointParser.invalidPoint)
                {
                    this.eventLogger.AddEvent(
                        new SyntaxError("Invalid region point format: " + data.CurrentLine, line)
                    );
                    return;
                }

                points.Add(
                    new RegionPoint(
                        parsedPoint,
                        line.definition,
                        line.docblock,
                        line.inlineComment
                    )
                );
            }

            // We shouldn't end without a fully defined region
            if (expectingColourDefinition)
            {
                this.eventLogger.AddEvent(
                    new SyntaxError(
                        "Incomplete region at end of file",
                        data.FullPath
                    )
                );
                return;
            }

            // Add the last element
            this.elements.Regions.Add(
                new Region(
                    regionName,
                    points,
                    declarationLine.definition,
                    declarationLine.docblock,
                    declarationLine.inlineComment
                )
            );
        }
    }
}
