﻿using System.Collections.Generic;
using Xunit;
using Moq;
using Compiler.Parser;
using Compiler.Error;
using Compiler.Model;
using Compiler.Event;
using Compiler.Output;

namespace CompilerTest.Parser
{
    public class AirwayParserTest
    {
        private readonly AirwayParser parser;

        private readonly SectorElementCollection collection;

        private readonly Mock<IEventLogger> log;

        public AirwayParserTest()
        {
            this.log = new Mock<IEventLogger>();
            this.collection = new SectorElementCollection();
            this.parser = (AirwayParser)(new SectionParserFactory(this.collection, this.log.Object))
                .GetParserForSection(OutputSections.SCT_LOW_AIRWAY);
        }

        [Fact]
        public void TestItRaisesSyntaxErrorNotEnoughSegments()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "EGHI",
                new List<string>(new string[] { "UN864 North" })
            );
            this.parser.ParseData(data);

            this.log.Verify(foo => foo.AddEvent(It.IsAny<SyntaxError>()), Times.Once);
        }

        [Fact]
        public void TestItRaisesSyntaxErrorInvalidEndPoint()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "EGHI",
                new List<string>(new string[] { "UN864 North N050.57.00.000 W001.21.24.490 N050.57.00.000 N001.21.24.490" })
            );
            this.parser.ParseData(data);

            this.log.Verify(foo => foo.AddEvent(It.IsAny<SyntaxError>()), Times.Once);
        }

        [Fact]
        public void TestItRaisesSyntaxErrorInvalidStartPoint()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "EGHI",
                new List<string>(new string[] { "UN864 North N050.57.00.000 N001.21.24.490 N050.57.00.000 W001.21.24.490" })
            );
            this.parser.ParseData(data);

            this.log.Verify(foo => foo.AddEvent(It.IsAny<SyntaxError>()), Times.Once);
        }

        [Fact]
        public void TestItHandlesMetadata()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "test",
                new List<string>(new string[] { "" })
            );

            this.parser.ParseData(data);
            Assert.IsType<BlankLine>(this.collection.Compilables[OutputSections.SCT_LOW_AIRWAY][0]);
        }

        [Fact]
        public void TestItAddsAirwayData()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "EGHI",
                new List<string>(new string[] { "UN864 North   N050.57.00.001 W001.21.24.490 N050.57.00.002 W001.21.24.490;comment" })
            );
            this.parser.ParseData(data);

            Airway result = this.collection.LowAirways[0];
            Assert.Equal("UN864 North", result.Identifier);
            Assert.Equal(AirwayType.LOW, result.Type);
            Assert.Equal(new Point(new Coordinate("N050.57.00.001", "W001.21.24.490")), result.StartPoint);
            Assert.Equal(new Point(new Coordinate("N050.57.00.002", "W001.21.24.490")), result.EndPoint);
            Assert.Equal("comment", result.Comment);
        }

        [Fact]
        public void TestItAddsAirwayDataWithIdentifiers()
        {
            SectorFormatData data = new SectorFormatData(
                "test",
                "EGHI",
                new List<string>(new string[] { "UN864 North   DIKAS DIKAS BHD BHD;comment" })
            );
            this.parser.ParseData(data);

            Airway result = this.collection.LowAirways[0];
            Assert.Equal("UN864 North", result.Identifier);
            Assert.Equal(AirwayType.LOW, result.Type);
            Assert.Equal(new Point("DIKAS"), result.StartPoint);
            Assert.Equal(new Point("BHD"), result.EndPoint);
            Assert.Equal("comment", result.Comment);
        }
    }
}
