module Tests.Parser

open Tests.Setup
open EasyBuild.CommitParser
open EasyBuild.CommitParser.Types
open System.Threading.Tasks

let private opiniatedTypeConfig: CommitParserConfig =
    {
        Types =
            [
                {
                    Name = "feat"
                    Description = Some "A new feature"
                    SkipTagFooter = false
                }
                {
                    Name = "fix"
                    Description = Some "A bug fix"
                    SkipTagFooter = false
                }
                {
                    Name = "ci"
                    Description = Some "Changes to CI/CD configuration"
                    SkipTagFooter = true
                }
                {
                    Name = "chore"
                    Description =
                        Some
                            "Changes to the build process or auxiliary tools and libraries such as documentation generation"
                    SkipTagFooter = true
                }
                {
                    Name = "docs"
                    Description = Some "Documentation changes"
                    SkipTagFooter = false
                }
                {
                    Name = "test"
                    Description = Some "Adding missing tests or correcting existing tests"
                    SkipTagFooter = false
                }
                {
                    Name = "style"
                    Description =
                        Some
                            "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)"
                    SkipTagFooter = false
                }
                {
                    Name = "refactor"
                    Description = Some "A code change that neither fixes a bug nor adds a feature"
                    SkipTagFooter = false
                }
            ]
        Tags = None
    }

let private opiniatedTypeConfigWithProjectList =
    { opiniatedTypeConfig with
        Tags = Some [ "converter"; "web" ]
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
    let ``works for summary without space after colon`` () =
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
            "refactor:<description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for summary without multiple spaces after colon`` () =
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
            "refactor:<description>" |> Parser.validateFirstLine CommitParserConfig.Default

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

    [<Test>]
    let ``report an error is ':' is missing`` () =
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
            "feat <description>" |> Parser.validateFirstLine CommitParserConfig.Default

        Expect.equal actual expected

module ValidateBodyAndFooters =

    [<Test>]
    let ``works for single line body message only`` () =
        let actual =
            [ "This is the body message" ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal actualBody "This is the body message"
            Expect.equal actualFooters Map.empty
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``works for multi-line body message only`` () =
        let actual =
            [
                "This is the body message"
                "with a second line"
                ""
                "This is another paragraph"
            ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal
                actualBody
                """This is the body message
with a second line

This is another paragraph"""

            Expect.equal actualFooters Map.empty
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``works for single line body message and footer`` () =
        let actual =
            [ "This is the body message"; ""; "Project: converter"; "Issue #123" ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        let expectedFooters =
            [ "Issue", [ "#123" ]; "Project", [ "converter" ] ] |> Map.ofList

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal actualBody "This is the body message\n"

            Expect.equal actualFooters expectedFooters
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``works for multi-line body message and footer`` () =
        let actual =
            [
                "This is the body message"
                "with a second line"
                ""
                "This is another paragraph"
                ""
                "Project: converter"
                "Issue #123"
            ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        let expectedFooters =
            [ "Issue", [ "#123" ]; "Project", [ "converter" ] ] |> Map.ofList

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal
                actualBody
                """This is the body message
with a second line

This is another paragraph
"""

            Expect.equal actualFooters expectedFooters
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``works for multi-line body message and footer with multiple values`` () =
        let actual =
            [
                "This is the body message"
                "with a second line"
                ""
                "This is another paragraph"
                ""
                "Project: converter"
                "Project: web"
                "Issue: #123"
            ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        let expectedFooters =
            [ "Project", [ "web"; "converter" ]; "Issue", [ "#123" ] ] |> Map.ofList

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal
                actualBody
                """This is the body message
with a second line

This is another paragraph
"""

            Expect.equal actualFooters expectedFooters
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``only known tags are allowed`` () =
        let expected =
            "Unkonwn tag(s) in the footer.

Received:

- some-tag

But allowed tags are:

- converter
- web"
            |> Error

        let actual =
            [
                "This is the body message"
                "with a second line"
                ""
                "This is another paragraph"
                ""
                "Tag: some-tag"
            ]
            |> Parser.validateBodyAndFooters opiniatedTypeConfigWithProjectList "feat"

        Expect.equal actual expected

    [<Test>]
    let ``report an error if one of the tags is unkonwn`` () =
        let expected =
            "Unkonwn tag(s) in the footer.

Received:

- converter
- some-tag

But allowed tags are:

- converter
- web"
            |> Error

        let actual =
            [
                "This is the body message"
                "with a second line"
                ""
                "This is another paragraph"
                ""
                "Tag: some-tag"
                "Tag: converter"
            ]
            |> Parser.validateBodyAndFooters opiniatedTypeConfigWithProjectList "feat"

        Expect.equal actual expected

    [<Test>]
    let ``works if all tags are known`` () =
        let actual =
            [ "This is the body message"; ""; "Tag: converter"; "Tag: web" ]
            |> Parser.validateBodyAndFooters opiniatedTypeConfigWithProjectList "feat"

        let expectedFooters = [ "Tag", [ "web"; "converter" ] ] |> Map.ofList

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal
                actualBody
                """This is the body message
"""

            Expect.equal actualFooters expectedFooters
        | _ -> failwith "Expected Ok"

    [<Test>]
    let ``work of hashed footer`` () =
        let actual =
            [ "This is the body message"; ""; "Refs #123" ]
            |> Parser.validateBodyAndFooters CommitParserConfig.Default "feat"

        let expectedFooters = [ "Refs", [ "#123" ] ] |> Map.ofList

        match actual with
        | Ok(actualBody, actualFooters) ->
            Expect.equal
                actualBody
                """This is the body message
"""

            Expect.equal actualFooters expectedFooters
        | _ -> failwith "Expected Ok"

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

Expected an empty line after subject line.

Example:
-------------------------
feat: add new feature

This is the body of the commit message
with a second line
-------------------------"
            |> Error

        let actual =
            "feat: add new feature
This is the body message"

            |> Parser.tryValidateCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

module TryParseCommitMessage =

    [<Test>]
    let ``works for short commit message only`` () =
        let expected =
            {
                Type = "feat"
                Scope = None
                Description = "add new feature"
                Body = ""
                BreakingChange = false
                Footers = Map.empty
            }
            |> Ok

        let actual =
            "feat: add new feature"
            |> Parser.tryParseCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``works for commit message / footer`` () =
        let expected =
            {
                Type = "feat"
                Scope = None
                Description = "add new feature"
                Body = ""
                BreakingChange = false
                Footers = Map.ofList [ "Project", [ "converter" ] ]
            }
            |> Ok

        let actual =
            "feat: add new feature

Project: converter"
            |> Parser.tryParseCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    //     [<Test>]
    //     let ``works for commit message / tag line / body message`` () =
    //         let expected =
    //             {
    //                 Type = "feat"
    //                 Scope = None
    //                 Description = "add new feature"
    //                 Body = "This is the body message"
    //                 BreakingChange = false
    //                 Tags = Some [ "converter" ]
    //             }
    //             |> Ok

    //         let actual =
    //             "feat: add new feature

    // [converter]

    // This is the body message"
    //             |> Parser.tryParseCommitMessage CommitParserConfig.Default

    //         Expect.equal actual expected

    [<Test>]
    let ``works for commit message / body message if tag line is not required`` () =
        let expected =
            {
                Type = "feat"
                Scope = None
                Description = "add new feature"
                Body = "This is the body message"
                BreakingChange = false
                Footers = Map.empty
            }
            |> Ok

        let actual =
            "feat: add new feature

This is the body message"
            |> Parser.tryParseCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``footer 'BREAKING CHANGE' is detected as a breaking change`` () =
        let expected =
            {
                Type = "feat"
                Scope = None
                Description = "add new feature"
                Body = ""
                BreakingChange = true
                Footers = Map.ofList [ "BREAKING CHANGE", [ "this is change is breaking" ] ]
            }
            |> Ok

        let actual =
            "feat: add new feature

BREAKING CHANGE: this is change is breaking"

            |> Parser.tryParseCommitMessage CommitParserConfig.Default

        Expect.equal actual expected

    [<Test>]
    let ``footer 'BREAKING-CHANGE' is detected as a breaking change`` () =
        let expected =
            {
                Type = "feat"
                Scope = None
                Description = "add new feature"
                Body = ""
                BreakingChange = true
                Footers = Map.ofList [ "BREAKING-CHANGE", [ "this is change is breaking" ] ]
            }
            |> Ok

        let actual =
            "feat: add new feature

BREAKING-CHANGE: this is change is breaking"
            |> Parser.tryParseCommitMessage CommitParserConfig.Default

        Expect.equal actual expected
