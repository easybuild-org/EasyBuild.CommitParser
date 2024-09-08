module Tests.Parser

open Tests.Setup
open EasyBuild.CommitParser
open EasyBuild.CommitParser.Types
open System.Threading.Tasks

let private opiniatedTagsConfig: CommitParserConfig =
    {
        Types =
            [
                {
                    Name = "feat"
                    Description = Some "A new feature"
                    SkipTagLine = false
                }
                {
                    Name = "fix"
                    Description = Some "A bug fix"
                    SkipTagLine = false
                }
                {
                    Name = "ci"
                    Description = Some "Changes to CI/CD configuration"
                    SkipTagLine = true
                }
                {
                    Name = "chore"
                    Description =
                        Some
                            "Changes to the build process or auxiliary tools and libraries such as documentation generation"
                    SkipTagLine = true
                }
                {
                    Name = "docs"
                    Description = Some "Documentation changes"
                    SkipTagLine = false
                }
                {
                    Name = "test"
                    Description = Some "Adding missing tests or correcting existing tests"
                    SkipTagLine = false
                }
                {
                    Name = "style"
                    Description =
                        Some
                            "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)"
                    SkipTagLine = false
                }
                {
                    Name = "refactor"
                    Description = Some "A code change that neither fixes a bug nor adds a feature"
                    SkipTagLine = false
                }
            ]
        Tags = None
    }

module ValidateFirstLine =

    [<Test>]
    let ``works for 'feat' type with default config`` () =
        let expected =
            ({
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "feat: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'fix' type with default config`` () =
        let expected =
            ({
                Type = "fix"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "fix: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'ci' type with default config`` () =
        let expected =
            ({
                Type = "ci"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "ci: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'chore' type with default config`` () =
        let expected =
            ({
                Type = "chore"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "chore: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'docs' type with default config`` () =
        let expected =
            ({
                Type = "docs"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "docs: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'test' type with default config`` () =
        let expected =
            ({
                Type = "test"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "test: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'style' type with default config`` () =
        let expected =
            ({
                Type = "style"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "style: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for 'refactor' type with default config`` () =
        let expected =
            ({
                Type = "refactor"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "refactor: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``supports `!` for indicating a breaking change`` () =
        let expected =
            ({
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = true
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "feat!: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``supports `!` for indicating a breaking change with a scope`` () =
        let expected =
            ({
                Type = "feat"
                Scope = Some "scope"
                Description = "<description>"
                BreakingChange = true
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "feat(scope)!: <description>"
            |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``support providing a scope`` () =
        let expected =
            ({
                Type = "feat"
                Scope = Some "scope"
                Description = "<description>"
                BreakingChange = false
            }
            : FirstLineParsedResult)
            |> Ok

        let actual =
            "feat(scope): <description>"
            |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``fails if the scope is empty`` () =
        let expected =
            "Invalid commit message format.

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
-------------------------"
            |> Error

        let actual =
            "feat(): <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``reject invalid message format and generate help test based on the provided configuration``
        ()
        =
        let expected =
            "Invalid commit message format.

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
-------------------------"
            |> Error

        let actual =
            "invalid: <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

module ValidateSecondLine =

    [<Test>]
    let ``should works for empty line"`` () =
        let expected = Ok()

        let actual = "" |> Parser.validateSecondLine

        Expect.equal actual expected

    [<Test>]
    let ``should fail for non-empty line"`` () =
        let expected =
            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"

            |> Error

        let actual = "non empty line" |> Parser.validateSecondLine

        Expect.equal actual expected

module ValidateTagLine =

    [<Test>]
    let ``works with an empty line if the type is flagged as 'SkipTagLine'"`` () =
        let commitMessage: FirstLineParsedResult =
            {
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }

        let expected = None |> Ok

        let actual = "" |> Parser.validateTagLine CommitParserConfig.Default commitMessage

        Expect.equal actual expected

    [<Test>]
    let ``return the tag if the type is flagged as 'SkipTagLine'"`` () =
        let commitMessage: FirstLineParsedResult =
            {
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }

        let expected = [ "converter" ] |> Some |> Ok

        let actual =
            "[converter]" |> Parser.validateTagLine CommitParserConfig.Default commitMessage

        Expect.equal actual expected

    [<Test>]
    let ``return the list of tags if the type is flagged as 'SkipTagLine'"`` () =
        let commitMessage: FirstLineParsedResult =
            {
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }

        let expected = [ "converter"; "web" ] |> Some |> Ok

        let actual =
            "[converter][web]"
            |> Parser.validateTagLine CommitParserConfig.Default commitMessage

        Expect.equal actual expected

    [<Test>]
    let ``return an error if the type is not flagged as 'SkipTagLine'"`` () =
        let commitMessage: FirstLineParsedResult =
            {
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }

        let expected =
            Error
                "Invalid commit message format.

Expected a tag line with the following format: '[tag1][tag2]...[tagN]'

Example:
-------------------------
feat: add new feature

[tag1][tag2]
-------------------------"

        let actual = "" |> Parser.validateTagLine opiniatedTagsConfig commitMessage

        Expect.equal actual expected

    [<Test>]
    let ``return an error if tag line is in an invalid format and the type is not flagged as 'SkipTagLine'``
        ()
        =
        let commitMessage: FirstLineParsedResult =
            {
                Type = "feat"
                Scope = None
                Description = "<description>"
                BreakingChange = false
            }

        let expected =
            Error
                "Invalid commit message format.

Expected a tag line with the following format: '[tag1][tag2]...[tagN]'

Example:
-------------------------
feat: add new feature

[tag1][tag2]
-------------------------"

        let actual =
            "invalid tag line" |> Parser.validateTagLine opiniatedTagsConfig commitMessage

        Expect.equal actual expected

module TryValidateCommitMessage =

    [<Test>]
    let ``works for short commit message only`` () =
        let actual =
            "feat: add new feature"
            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual (Ok())

    [<Test>]
    let ``works for commit message / tag line`` () =
        let actual =
            "feat: add new feature

[converter]"
            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual (Ok())

    [<Test>]
    let ``works for commit message / tag line / body message`` () =
        let actual =
            "feat: add new feature

[converter]

This is the body message"
            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual (Ok())

    [<Test>]
    let ``works for commit message / body message if tag line is not required`` () =
        let actual =
            "feat: add new feature

This is the body message"

            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual (Ok())

    [<Test>]
    let ``returns an error if an empty line is missing between the commit message and body`` () =
        let expected =
            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"
            |> Error

        let actual =
            "feat: add new feature
This is the body message"

            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``returns an error if an empty line is missing between the commit message and tag line``
        ()
        =
        let expected =
            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"
            |> Error

        let actual =
            "feat: add new feature
[converter]"

            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``returns an error if an empty line is missing after the tag line and body`` () =
        let expected =
            "Invalid commit message format.

Expected an empty line after the tag line.

Example:
-------------------------
feat: add new feature

[tag1][tag2]

-------------------------"
            |> Error

        let actual =
            "feat: add new feature

[converter]
This is the body message"

            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual expected
