module EasyBuild.CommitParser.Types

open Thoth.Json.Core

type internal FirstLineParsedResult =
    {
        Type: string
        Scope: string option
        Description: string
        BreakingChange: bool
    }

type CommitMessage =
    {
        Type: string
        Scope: string option
        Description: string
        Body: string
        BreakingChange: bool
        Footers: Map<string, string list>
    }

type CommitType =
    {
        Name: string
        Description: string option
        SkipTagFooter: bool
    }

module CommitType =

    let decoder: Decoder<CommitType> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Description = get.Optional.Field "description" Decode.string
                SkipTagFooter =
                    get.Optional.Field "skipTagFooter" Decode.bool |> Option.defaultValue true
            }
        )

type CommitParserConfig =
    {
        Types: CommitType list
        Tags: string list option
    }

    static member Default =
        {
            Types =
                [
                    {
                        Name = "feat"
                        Description = Some "A new feature"
                        SkipTagFooter = true
                    }
                    {
                        Name = "fix"
                        Description = Some "A bug fix"
                        SkipTagFooter = true
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
                        SkipTagFooter = true
                    }
                    {
                        Name = "test"
                        Description = Some "Adding missing tests or correcting existing tests"
                        SkipTagFooter = true
                    }
                    {
                        Name = "style"
                        Description =
                            Some
                                "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)"
                        SkipTagFooter = true
                    }
                    {
                        Name = "refactor"
                        Description =
                            Some "A code change that neither fixes a bug nor adds a feature"
                        SkipTagFooter = true
                    }
                ]
            Tags = None
        }

module CommitParserConfig =

    let decoder: Decoder<CommitParserConfig> =
        Decode.object (fun get ->
            {
                Types = get.Required.Field "types" (Decode.list CommitType.decoder)
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            }
        )
