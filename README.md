# EasyBuild.Parser


[![NuGet](https://img.shields.io/nuget/v/EasyBuild.CommitParser.svg)](https://www.nuget.org/packages/EasyBuild.CommitParser)
[![](https://img.shields.io/badge/Sponsors-EA4AAA)](https://mangelmaxime.github.io/sponsors/)

Common commit parser library used by other EasyBuild tools like [EasyBuild.ChangelogGen](https://github.com/easybuild-org/EasyBuild.ChangelogGen) or [EasyBuild.CommitLinter](https://github.com/easybuild-org/EasyBuild.CommitLinter).

## Usage

```fs
open EasyBuild.CommitParser
open EasyBuild.CommitParser.Types

let commitText = "..."

// If you need the commit message information
Parser.tryParseCommitMessage CommitParserConfig.Default commitText
// > it: Result<CommitMessage,string>

// If you just want to validate the commit message
Parser.tryValidateCommitMessage CommitParserConfig.Default commitText
// > it: Result<unit,string>
```

For the configuration, you can use the default configuration or provide a custom one.

```fs
open EasyBuild.CommitParser.Types

// Default configuration
CommitParserConfig.Default

// My custom configuration
{
    Types =
        [
            // ...
        ]
    Tags =
        [
            // ...
        ] |> Some
}

// You can also use a configuration file by passing the JSON content to the included Decoder
open Thoth.Json.Newtonsoft

let configurationJson = "..."

match Decode.fromString CommitParserConfig.decoder configContent with
| Ok config -> config
| Error error ->
    failwithf "Error while parsing the configuration file: %s" error
```

## Commit Format

> [!NOTE]
> EasyBuild.CommitParser format is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) and extends it to work in a monorepo environment.

```text
<type>[optional scope][optional !]: <description>

[optional tags]

[optional body]

[optional footer]
```

- `[optional tags]` format is as follows:

    ```text
    [tag1][tag2][tag3]
    ```

- `[optional body]` is a free-form text.

    ```text
    This is a single line body.
    ```

    ```text
    This is a

    multi-line body.
    ```

- `[optional footer]` follows [git trailer format](https://git-scm.com/docs/git-interpret-trailers)

    ```text
    BREAKING CHANGE: <description>
    Signed-off-by: Alice <alice@example.com>
    Signed-off-by: Bob <bob@example.com>
    ```

## Configuration

EasyBuild.CommitParser comes with a default configuration to validate your commit.

The default configuration allows the following commit types with no tags required:

- `feat` - A new feature
- `fix` - A bug fix
- `ci` - Changes to CI/CD configuration
- `chore` - Changes to the build process or auxiliary tools and libraries such as documentation generation
- `docs` - Documentation changes
- `test` - Adding missing tests or correcting existing tests
- `style` - Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- `refactor` - A code change that neither fixes a bug nor adds a feature

If needed, you can provide a custom configuration either by code directly or by using a configuration file using JSON format.

### Configuration File Format

The configuration file is a JSON file with the following properties:

#### types

- Required: ✅
- Type: `{ name: string, description?: string, skipTagLine?: bool } []`

List of accepted commit types.

| Property    | Type   | Required | Description                           |
| ----------- | ------ | :------: | ------------------------------------- |
| name        | string |    ✅    | The name of the commit type.          |
| description | string |    ❌    | The description of the commit type.   |
| skipTagLine | bool   |    ❌    | If `true` skip the tag line validation. <br> If `false`, checks that the tag line format is valid and contains knows tags. <br><br>Default is `true`. |

#### tags

- Required: ❌
- Type: `string []`

List of accepted commit tags.

#### Examples

```json
{
    "types": [
        { "name": "feat", "description": "A new feature", "skipTagLine": false },
        { "name": "fix", "description": "A bug fix", "skipTagLine": false },
        { "name": "docs", "description": "Documentation changes", "skipTagLine": false },
        { "name": "test", "description": "Adding missing tests or correcting existing tests", "skipTagLine": false },
        { "name": "style", "description": "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)", "skipTagLine": false },
        { "name": "refactor", "description": "A code change that neither fixes a bug nor adds a feature", "skipTagLine": false },
        { "name": "ci", "description": "Changes to CI/CD configuration" },
        { "name": "chore", "description": "Changes to the build process or auxiliary tools and libraries such as documentation generation" }
    ],
    "tags": [
        "cli",
        "converter"
    ]
}
```
