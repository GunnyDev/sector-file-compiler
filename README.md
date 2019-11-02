# sector-file-compiler
Compiler and validator for the [VATSIM UK Sector File](https://github.com/VATSIM-UK/uk-sector-file).

[![Build Status](https://travis-ci.com/AndyTWF/sector-file-compiler.svg?branch=master)](https://travis-ci.com/AndyTWF/sector-file-compiler)

## What Does It Do?

The compiler parses a number of input files, validates their contents and then compiles the overall result into one big file for circulation and use in VATSIM controller clients. It can also apply transformations to the output, such as removing comment lines and replacing tokens.

## Compiling

The solution is best compiled using Visual Studio 2019. The required test runners and dependencies can then be installed via the dotnet CLI.

# Using The Compiler

To run the compiler, you need to use the CompilerCli executable.

## Input Configuration

The input for the compiler is determined through a configuration file in JSON format. This file is used to specify which files should be compiled, which sections of the output they should appear in and the order in which they should appear. All file paths are relative to the config file.

### Compiler configuration sections

`ese_header` - EuroScope only. A comment block or copyright notice to go at the top of the ESE output.
`ese_preamble` - EuroScope only. Any other information to appear at the top of the ESE.
`positions` - EuroScope only. The \[POSITIONS\] section of the ESE.
`freetext` - EuroScope only. The \[FREETEXT\] section of the ESE.
`sidsstars` - EuroScope only. EuroScope only. The \[SIDSSTARS\] section of the ESE.
`airspace` - EuroScope only. EuroScope only. The \[AIRSPACE\] section of the ESE.

### Example compiler configuration

```JSON
{
  "ese_header": [
    "Copyright.txt"
  ],
  "ese_preamble": [
    "preamble.txt"
  ],
  "sidsstars": [
    "Airports/EGBB/Sids.txt",
    "Airports/EGBB/Stars.txt",
  ],
  "positions": [
      "Airports/EGBB/Positions.txt"
  ],
  "freetext": [
      "Airports/EGBB/Freetext.txt"
  ],
  "airspace": [
      "Airports/EGBB/Airspace.txt"
  ],
}

```

## Command Line Flags

### Required Flags

`--config-file` - Takes a single argument. Path to the compiler configuration JSON file.

`--out-file` - Takes a single argument. Where the output file should be generated.

### Optional Flags

`--ignore-validation` - If set, any validation failures will not halt compilation.

`--strip-comments` - If set, any comments in the input will be removed. This does not apply to files named in the header parts of the compiler configuration.

`--strip-newlines` - If set, any lines that are empty (i.e. just newlines) in the input will be removed.

`--build-version` - Takes a single argument. Specifies the version of the build to replace the `{VERSION}` token in the input.

### Token Replacements

There are a number of tokens that may be added to comments in the input files, which will be replaced in the output.

`{YEAR}` - The year at the time of compilation.
`{VERSION}` - A user generated version for the sector file, e.g. the AIRAC date. Defaults to current date and time.
`{BUILD}` - The date on which the build occured.
