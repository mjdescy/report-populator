# Business requirements

Create a dotnet 10 console app that does the following:

1. Read a configuration file (.json).
2. From the configuration file, read a list of source file names, destination file names, destination worksheet names, and destination cell addresses (e.g. "A4"). Each source file name, destination file name, and destination cell address should be stored in a record.
3. For each record, open the source file (which will be a .xlsx file), read the region/range starting at cell A1 in the first worksheet, then copy those values to the destination file (which is a .xlsx file) into the worksheet defined by the destination worksheet name, into the destination cell address. The destination cell address will be the top-left cell in the range that is pasted. The range will be larger than one cell is almost all cases. Save and close the destination Excel file.

Use ClosedXML for .xlsx reading and writing.

Use CommandLineParser to parse command line arguments.

The main function can be called via reportpopulator.exe run [config.json].

Create a sample configuration file. Create a command (verb) named 'init' that outputs the sample configuration file. The init command will output the file to a path if one is specified or to the current working directory if no path is specified.

Create unit tests for all functionality. Unit tests should use xUnit. Unit tests should include end-to-end tests with sample .xlsx files. Sample .xlsx files can be created programmatially in unit tests and stored in temporary directories that are deleted after they are used.

Assign the MIT license.

Create a git repository. Commit after major changes are made.

# Solution structure

Use dotnet sln and dotnet new to create the solution and projects.
Use dotnet new to create a gitignore file.
The solution is named report-populator. 
The exe file is named reportpopulator.
Source files and dotnet projects should be within a /src directory.
Unit tests should be in a /tests directory.
There should be a console app and a library as two separate projects.
The console app project should only contain functionality related to parsing command line arguments. All the real work must be done by the library.
Set the initial version number to 0.1.0.
