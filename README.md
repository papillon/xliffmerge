# XliffMerge
A tool we use at [Singhammer IT Consulting](https://www.singhammer.com/) to merge translations between XLIFF files. It's main purpose is to take updated translations from the Microsoft Business Central base app and merge them into our customized XLIFF file.

## General
The tool goes through the target file and tries to find a matching trans-unit in the source file. If one is found, it will add (and optionally replace) the translation to the target trans-unit. It will not add new trans-units to the target file from source, since its main use is to add translations to the generated XLIFF (*.g.xlf) file that is created during compilation of the app.

### Usage
`XliffMerge.exe --source "Base Application.de-DE.xlf" --target "Base Application.g.xlf"`

 * -s, --source     Required. Filename where translations are read from.
 * -t, --target     Required. Filename where translations are written to.
 * -r, --replace    Replace existing translations in destination.
 * --help           Display this help screen.
 * --version        Display version information.