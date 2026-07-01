# Report Populator

Copies cell ranges from source Excel workbooks into destination workbooks at specified positions.

Reads a JSON configuration file containing source/destination mappings and, for each mapping, copies the used range (starting at A1) from the first worksheet of the source `.xlsx` file to a target worksheet and cell in the destination `.xlsx` file.

## Usage

```
reportpopulator run <config.json>
reportpopulator init [output-path]
```

### Generate a sample config

```sh
reportpopulator init                    # writes ./sample-config.json
reportpopulator init path/to/config.json
```

### Run report population

```sh
reportpopulator run config.json
```

## Configuration

```json
{
  "mappings": [
    {
      "sourceFilePath": "data/source.xlsx",
      "destinationFilePath": "output/report.xlsx",
      "destinationWorksheet": "Summary",
      "destinationCellAddress": "A4"
    }
  ]
}
```

| Field | Description |
|---|---|
| `sourceFilePath` | Path to the source `.xlsx` file |
| `destinationFilePath` | Path to the destination `.xlsx` file (created if missing) |
| `destinationWorksheet` | Target worksheet name (created if missing) |
| `destinationCellAddress` | Top-left cell where source data is pasted (e.g. `A4`) |

Multiple mappings to the same destination file and worksheet are supported.

## License

MIT
