#!/usr/bin/env nu

let scriptDir = (path dirname $nu.currentFile)
cd $scriptDir

cp template.xlsx report.xlsx

reportpopulator run config.json

if $env.LAST_EXIT_CODE == 0 {
    print "Report populated successfully: report.xlsx"
} else {
    print "Report population failed."
}
