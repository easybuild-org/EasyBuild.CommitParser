# EasyBuild.Parser


[![NuGet](https://img.shields.io/nuget/v/EasyBuild.CommitParser.svg)](https://www.nuget.org/packages/EasyBuild.CommitParser)
[![](https://img.shields.io/badge/Sponsors-EA4AAA)](https://mangelmaxime.github.io/sponsors/)

Common commit parser library used by other EasyBuild tools like [EasyBuild.ChangelogGen](https://github.com/easybuild-org/EasyBuild.ChangelogGen) or [EasyBuild.CommitLinter](https://github.com/easybuild-org/EasyBuild.CommitLinter).

It aims to provide helpful contextual error messages like:

Example of error messages:

```text
Invalid commit message format.

Expected a commit message with the following format: '<type>[optional scope]: <description>'.

Where <type> is one of the following:

- feat: A new feature
- fix: A bug fix
- ci: Changes to CI/CD configuration
- chore: Changes to the build process or auxiliary tools and libraries such as documentation generation
- docs: Documentation changes
- test: Adding missing tests or correcting existing tests
- style: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- refactor: A code change that neither fixes a bug nor adds a feature

Example:
-------------------------
feat: some description
-------------------------
```

or

```text
Unkonwn tag(s) in the footer.

Received:

- some-tag

But allowed tags are:

- converter
- web
```

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
> EasyBuild.CommitParser format is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
>
> It add a special `Tag` footer, allowing it to be used in a mono-repo context.
>
> Tools like [EasyBuild.ChangelogGen](https://github.com/easybuild-org/EasyBuild.ChangelogGen) can use the tag to filter the commits to include in the changelog.

```text
<type>[optional scope][optional !]: <description>

[optional body]

[optional footer]
```

- `[optional body]` is a free-form text.

    ```text
    This is a single line body.
    ```

    ```text
    This is a

    multi-line body.
    ```

- `[optional footer]` is inspired by [git trailer format](https://git-scm.com/docs/git-interpret-trailers) `key: value` but also allows `key #value`

    ```text
    BREAKING CHANGE: <description>
    Signed-off-by: Alice <alice@example.com>
    Signed-off-by: Bob <bob@example.com>
    Refs #123
    Tag: cli
    ```

    💡 The `Tag` footer can be provided multiple times.

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
- Type: `{ name: string, description?: string, skipTagFooter?: bool } []`

List of accepted commit types.

| Property      | Type   | Required | Description                           |
| --------------| ------ | :------: | ------------------------------------- |
| name          | string |    ✅    | The name of the commit type.          |
| description   | string |    ❌    | The description of the commit type.   |
| skipTagFooter | bool   |    ❌    | If `true` skip the tag footer validation. <br> If `false`, checks that the tag footer is provided and contains knows tags. <br><br>Default is `true`. |

#### tags

- Required: ❌
- Type: `string []`

List of accepted commit tags.

#### Examples

```json
{
    "types": [
        { "name": "feat", "description": "A new feature", "skipTagFooter": false },
        { "name": "fix", "description": "A bug fix", "skipTagFooter": false },
        { "name": "docs", "description": "Documentation changes", "skipTagFooter": false },
        { "name": "test", "description": "Adding missing tests or correcting existing tests", "skipTagFooter": false },
        { "name": "style", "description": "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)", "skipTagFooter": false },
        { "name": "refactor", "description": "A code change that neither fixes a bug nor adds a feature", "skipTagFooter": false },
        { "name": "ci", "description": "Changes to CI/CD configuration" },
        { "name": "chore", "description": "Changes to the build process or auxiliary tools and libraries such as documentation generation" }
    ],
    "tags": [
        "cli",
        "converter"
    ]
}
```
