﻿using System.Collections.Generic;
using Xunit;
using Moq;
using Compiler.Parser;
using Compiler.Error;
using Compiler.Model;
using Compiler.Event;
using Compiler.Output;
using CompilerTest.Mock;

namespace CompilerTest.Parser
{
    public class SectorlineParserTest
    {
        private readonly AirspaceParser parser;

        private readonly SectorElementCollection collection;

        private readonly Mock<IEventLogger> log;

        public SectorlineParserTest()
        {
            this.log = new Mock<IEventLogger>();
            this.collection = new SectorElementCollection();
            this.parser = (AirspaceParser)(new DataParserFactory(this.collection, this.log.Object))
                .GetParserForSection(OutputSections.ESE_AIRSPACE);
        }

        public static IEnumerable<object[]> BadData => new List<object[]>
        {
            new object[] { new List<string>{
                "NOPE_SECTORLINE:BBTWR:EGBB:2.5 ;comment",
                "DISPLAY:BBAPP:BBAPP:BBTWR ;comment1",
                "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
            }}, // Invalid type
            new object[] { new List<string>{
                "CIRCLE_SECTORLINE:BBTWR:EGBB ;comment",
                "DISPLAY:BBAPP:BBAPP:BBTWR ;comment1",
                "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
            }}, // Invalid segments CIRCLE_SECTORLINE
            new object[] { new List<string>{
                "CIRCLE_SECTORLINE:AEAPP:W054.39.27.000:W006.12.57.000:30 ;comment3",
                "DISPLAY:AEAPP:AEAPP:Rathlin West ;comment4"
            }}, // Invalid coordinate CIRCLE_SECTORLINE
            new object[] { new List<string>{
                "CIRCLE_SECTORLINE:BBTWR:EGBB:abc ;comment",
                "DISPLAY:BBAPP:BBAPP:BBTWR ;comment1",
                "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
            }}, // Invalid radius CIRCLE_SECTORLINE
            new object[] { new List<string>{
                "CIRCLE_SECTORLINE:BBTWR:EGBB:2.5 ;comment",
                "DISPLAY:BBAPP:BBAPP ;comment1",
                "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
            }}, // Invalid display rule CIRCLE_SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6:LOLOL ;comment1",
                "DISPLAY:London S6:JJCTR:London S6 ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                "COORD:N050.00.00.000:W002.40.34.000 ;comment4",
                "COORD:N049.59.60.000:W002.29.35.000 ;comment5",
            }}, // Invalid declaration SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6 ;comment1",
                "WHATDISPLAY:London S6:JJCTR:London S6 ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                "COORD:N050.00.00.000:W002.40.34.000 ;comment4",
                "COORD:N049.59.60.000:W002.29.35.000 ;comment5",
            }}, // Unknown row SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6 ;comment1",
                "DISPLAY:London S6:JJCTR:London S6 ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
            }}, // No coordinates SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6 ;comment1",
                "DISPLAY:London S6:JJCTR:London S6:HI ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                "COORD:N050.00.00.000:W002.40.34.000 ;comment4",
                "COORD:N049.59.60.000:W002.29.35.000 ;comment5",
            }}, // Invalid display rule SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6 ;comment1",
                "DISPLAY:London S6:JJCTR:London S6 ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                "COORD:N050.00.00.000:W002.40.34.000:HI ;comment4",
                "COORD:N049.59.60.000:W002.29.35.000 ;comment5",
            }}, // Too many coordinate segments SECTORLINE
            new object[] { new List<string>{
                "SECTORLINE:JJCTR - S6 ;comment1",
                "DISPLAY:London S6:JJCTR:London S6 ;comment2",
                "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                "COORD:W050.00.00.000:W002.40.34.000 ;comment4",
                "COORD:N049.59.60.000:W002.29.35.000 ;comment5",
            }}, // Invalid coordinate SECTORLINE
        };

        [Theory]
        [MemberData(nameof(BadData))]
        public void ItRaisesSyntaxErrorsOnBadData(List<string> lines)
        {
            this.parser.ParseData(
                new MockSectorDataFile(
                    "test.txt",
                    lines
                )
            );

            Assert.Empty(this.collection.SectorLines);
            Assert.Empty(this.collection.CircleSectorLines);
            this.log.Verify(foo => foo.AddEvent(It.IsAny<SyntaxError>()), Times.Once);
        }

        [Fact]
        public void TestItAddsCircleSectorlines()
        {
            MockSectorDataFile data = new MockSectorDataFile(
                "test.txt",
                new List<string>(new string[] {
                    "CIRCLE_SECTORLINE:BBTWR:EGBB:2.5 ;comment",
                    "DISPLAY:BBAPP:BBAPP:BBTWR ;comment1",
                    "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
                    "CIRCLE_SECTORLINE:AEAPP:N054.39.27.000:W006.12.57.000:30 ;comment3",
                    "DISPLAY:AEAPP:AEAPP:Rathlin West ;comment4"
                })
            );

            this.parser.ParseData(data);

            // First
            CircleSectorline result1 = this.collection.CircleSectorLines[0];
            Assert.Equal("BBTWR", result1.Name);
            Assert.Equal("EGBB", result1.CentrePoint);
            Assert.Equal(2.5, result1.Radius);

            List<SectorlineDisplayRule> displayRules1 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("BBAPP", "BBAPP", "BBTWR", "comment1"),
                new SectorlineDisplayRule("BBTWR", "BBAPP", "BBTWR", "comment2"),
            };
            Assert.Equal(displayRules1, result1.DisplayRules);
            Assert.Equal("comment", result1.Comment);

            // Second
            CircleSectorline result2 = this.collection.CircleSectorLines[1];
            Assert.Equal("AEAPP", result2.Name);
            Assert.Equal(new Coordinate("N054.39.27.000", "W006.12.57.000"), result2.CentreCoordinate);
            Assert.Equal(30, result2.Radius);

            List<SectorlineDisplayRule> displayRules2 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("AEAPP", "AEAPP", "Rathlin West", "comment4"),
            };
            Assert.Equal(displayRules2, result2.DisplayRules);
            Assert.Equal("comment3", result2.Comment);
        }

        [Fact]
        public void TestItAddsSectorlines()
        {
            MockSectorDataFile data = new MockSectorDataFile(
                "test.txt",
                new List<string>(new string[] {
                    "SECTORLINE:JJCTR - S6 ;comment1",
                    "DISPLAY:London S6:JJCTR:London S6 ;comment2",
                    "DISPLAY:JJCTR:JJCTR:London S6 ;comment3",
                    "COORD:N050.00.00.000:W002.40.34.000 ;comment4",
                    "COORD:N049.59.59.000:W002.29.35.000 ;comment5",
                    "SECTORLINE:JJCTR - LS ;comment6",
                    "DISPLAY:London AC Worthing:JJCTR:London S6 ;comment7",
                    "DISPLAY:JJCTR:JJCTR:London AC Worthing ;comment8",
                    "COORD:N049.59.59.000:W002.29.35.000 ;comment9",
                    "COORD:N050.00.00.000:W001.47.00.000 ;comment10",
                })
            );

            this.parser.ParseData(data);

            // First
            Sectorline result1 = this.collection.SectorLines[0];
            Assert.Equal("JJCTR - S6", result1.Name);

            List<SectorlineDisplayRule> displayRules1 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("London S6", "JJCTR", "London S6", "comment2"),
                new SectorlineDisplayRule("JJCTR", "JJCTR", "London S6", "comment3"),
            };
            Assert.Equal(displayRules1, result1.DisplayRules);

            List<SectorlineCoordinate> coordinates1 = new List<SectorlineCoordinate>
            {
                new SectorlineCoordinate(new Coordinate("N050.00.00.000", "W002.40.34.000"), "comment4"),
                new SectorlineCoordinate(new Coordinate("N049.59.59.000", "W002.29.35.000"), "comment5"),
            };
            Assert.Equal(coordinates1, result1.Coordinates);
            Assert.Equal("comment1", result1.Comment);


            // Second
            Sectorline result2 = this.collection.SectorLines[1];
            Assert.Equal("JJCTR - LS", result2.Name);

            List<SectorlineDisplayRule> displayRules2 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("London AC Worthing", "JJCTR", "London S6", "comment7"),
                new SectorlineDisplayRule("JJCTR", "JJCTR", "London AC Worthing", "comment8"),
            };
            Assert.Equal(displayRules2, result2.DisplayRules);

            List<SectorlineCoordinate> coordinates2 = new List<SectorlineCoordinate>
            {
                new SectorlineCoordinate(new Coordinate("N049.59.59.000", "W002.29.35.000"), "comment9"),
                new SectorlineCoordinate(new Coordinate("N050.00.00.000", "W001.47.00.000"), "comment10"),
            };
            Assert.Equal(coordinates2, result2.Coordinates);
            Assert.Equal("comment6", result2.Comment);
        }

        [Fact]
        public void TestItAddsMixedData()
        {
            MockSectorDataFile data = new MockSectorDataFile(
                "test.txt",
                new List<string>(new string[] {
                    "SECTORLINE:JJCTR - LS ;comment6",
                    "DISPLAY:London AC Worthing:JJCTR:London S6 ;comment7",
                    "DISPLAY:JJCTR:JJCTR:London AC Worthing ;comment8",
                    "COORD:N049.59.59.000:W002.29.35.000 ;comment9",
                    "COORD:N050.00.00.000:W001.47.00.000 ;comment10",
                    "",
                    "CIRCLE_SECTORLINE:BBTWR:EGBB:2.5 ;comment",
                    "DISPLAY:BBAPP:BBAPP:BBTWR ;comment1",
                    "DISPLAY:BBTWR:BBAPP:BBTWR ;comment2",
                })
            );

            this.parser.ParseData(data);

            // First
            CircleSectorline result1 = this.collection.CircleSectorLines[0];
            Assert.Equal("BBTWR", result1.Name);
            Assert.Equal("EGBB", result1.CentrePoint);
            Assert.Equal(2.5, result1.Radius);

            List<SectorlineDisplayRule> displayRules1 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("BBAPP", "BBAPP", "BBTWR", "comment1"),
                new SectorlineDisplayRule("BBTWR", "BBAPP", "BBTWR", "comment2"),
            };
            Assert.Equal(displayRules1, result1.DisplayRules);
            Assert.Equal("comment", result1.Comment);


            // Second
            Sectorline result2 = this.collection.SectorLines[0];
            Assert.Equal("JJCTR - LS", result2.Name);

            List<SectorlineDisplayRule> displayRules2 = new List<SectorlineDisplayRule>
            {
                new SectorlineDisplayRule("London AC Worthing", "JJCTR", "London S6", "comment7"),
                new SectorlineDisplayRule("JJCTR", "JJCTR", "London AC Worthing", "comment8"),
            };
            Assert.Equal(displayRules2, result2.DisplayRules);

            List<SectorlineCoordinate> coordinates2 = new List<SectorlineCoordinate>
            {
                new SectorlineCoordinate(new Coordinate("N049.59.59.000", "W002.29.35.000"), "comment9"),
                new SectorlineCoordinate(new Coordinate("N050.00.00.000", "W001.47.00.000"), "comment10"),
            };
            Assert.Equal(coordinates2, result2.Coordinates);
            Assert.Equal("comment6", result2.Comment);
        }

        [Fact]
        public void TestItAddsMixedDataNoDisplayRules()
        {
            MockSectorDataFile data = new MockSectorDataFile(
                "test.txt",
                new List<string>(new string[] {
                    "SECTORLINE:JJCTR - LS ;comment6",
                    "COORD:N049.59.59.000:W002.29.35.000 ;comment9",
                    "COORD:N050.00.00.000:W001.47.00.000 ;comment10",
                    "",
                    "CIRCLE_SECTORLINE:BBTWR:EGBB:2.5 ;comment",
                })
            );

            this.parser.ParseData(data);

            // First
            CircleSectorline result1 = this.collection.CircleSectorLines[0];
            Assert.Equal("BBTWR", result1.Name);
            Assert.Equal("EGBB", result1.CentrePoint);
            Assert.Equal(2.5, result1.Radius);

            List<SectorlineDisplayRule> displayRules1 = new List<SectorlineDisplayRule>
            {
            };
            Assert.Equal(displayRules1, result1.DisplayRules);
            Assert.Equal("comment", result1.Comment);


            // Second
            Sectorline result2 = this.collection.SectorLines[0];
            Assert.Equal("JJCTR - LS", result2.Name);

            List<SectorlineDisplayRule> displayRules2 = new List<SectorlineDisplayRule>
            {
            };
            Assert.Equal(displayRules2, result2.DisplayRules);

            List<SectorlineCoordinate> coordinates2 = new List<SectorlineCoordinate>
            {
                new SectorlineCoordinate(new Coordinate("N049.59.59.000", "W002.29.35.000"), "comment9"),
                new SectorlineCoordinate(new Coordinate("N050.00.00.000", "W001.47.00.000"), "comment10"),
            };
            Assert.Equal(coordinates2, result2.Coordinates);
            Assert.Equal("comment6", result2.Comment);
        }
    }
}
