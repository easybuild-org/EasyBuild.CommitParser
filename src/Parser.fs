namespace EasyBuild.CommitParser

open EasyBuild.CommitParser.Types
open System.Text.RegularExpressions
open System
open FsToolkit.ErrorHandling

module Parser =

    let private invalidCommitMessage =
        "Invalid commit message format.

Expected a commit message with the following format:

<type>[optional scope]: <description>

[optional body]

[optional footer]

Example:
-------------------------
feat: add new feature

This is the body of the commit message

Signed-off-by: John Doe <john.doe@mail.com>
Refs #123
-------------------------"

    let private firstLineRegex =
        Regex("^(?<type>[^(!:]+)(\((?<scope>.+)\))?(?<breakingChange>!)?:\s*(?<description>.{1,})$")

    let private gitTailerRegex =
        Regex(
            "^((?<key_1>([A-Za-z0-9-]+|BREAKING CHANGE)):\s+(?<value_1>.*)|(?<key_2>[A-Za-z0-9-]+) (?<value_2>#.*))$"
        )

    let internal validateFirstLine
        (config: CommitParserConfig)
        (line: string)
        : Result<FirstLineParsedResult, string>
        =
        let m = firstLineRegex.Match(line)

        let generateErrorMessage () =
            let expectedTypesText =
                config.Types
                |> List.map (fun currentType ->
                    match currentType.Description with
                    | Some description -> $"- {currentType.Name}: {description}"
                    | None -> $"- {currentType.Name}"
                )
                |> String.concat "\n"

            let exemple =
                match config.Types with
                | [] -> "feat: some description"
                | head :: _ -> $"{head.Name}: some description"

            $"Invalid commit message format.

Expected a commit message with the following format: '<type>[optional scope]: <description>'.

Where <type> is one of the following:

%s{expectedTypesText}

Example:
-------------------------
%s{exemple}
-------------------------"

        if m.Success then
            let scope =
                if m.Groups.["scope"].Success then
                    Some m.Groups.["scope"].Value
                else
                    None

            let typ = m.Groups.["type"].Value

            let knownType =
                config.Types |> List.exists (fun currentType -> currentType.Name = typ)

            if knownType then
                {
                    Type = typ
                    Scope = scope
                    Description = m.Groups.["description"].Value
                    BreakingChange = m.Groups.["breakingChange"].Success
                }
                |> Ok
            else
                generateErrorMessage () |> Error
        else
            generateErrorMessage () |> Error

    let internal validateRawBody (lines: string list) =
        let errorMessage =
            "Invalid commit message format.

Expected an empty line after subject line.

Example:
-------------------------
feat: add new feature

This is the body of the commit message
with a second line
-------------------------"

        match lines with
        | [] -> Ok []
        | firstLine :: rest ->
            if String.IsNullOrWhiteSpace firstLine then
                rest |> Ok
            else
                Error errorMessage

    let internal validateBodyAndFooters
        (config: CommitParserConfig)
        (commitMessageType: string)
        (rawBodyLines: string list)
        =

        let footerLines =
            rawBodyLines
            |> List.rev
            |> List.takeWhile (fun line ->
                // Lines after the footer can be empty or a comment
                // We still capture them here, so we can retrieve the correct body
                String.IsNullOrEmpty line || gitTailerRegex.IsMatch line || line.StartsWith("#")
            )

        let body =
            rawBodyLines[0 .. (rawBodyLines.Length - footerLines.Length - 1)]
            |> String.concat "\n"

        let footer =
            footerLines
            |> List.choose (fun line ->
                let m = gitTailerRegex.Match(line)

                if m.Groups.["key_1"].Success then
                    Some(m.Groups.["key_1"].Value, m.Groups.["value_1"].Value)
                elif m.Groups.["key_2"].Success then
                    Some(m.Groups.["key_2"].Value, m.Groups.["value_2"].Value)
                else
                    None
            )
            |> List.groupBy fst
            |> List.map (fun (key, values) -> (key, values |> List.map snd))
            |> Map.ofList

        let typeConfigOpt =
            config.Types
            |> List.tryFind (fun currentType -> currentType.Name = commitMessageType)

        match typeConfigOpt with
        | None ->
            // In theory, this should never happen because we already validated the type
            Error "Commit message type not found in the configuration."
        | Some typeConfig ->
            let expectedTagsList =
                match config.Tags with
                | Some tags -> tags |> List.map (fun tag -> $"- {tag}") |> String.concat "\n"
                | None -> ""

            // No validation needed if the tag footer is skipped
            if typeConfig.SkipTagFooter then
                Ok(body, footer)
            else
                match Map.tryFind "Tag" footer with
                | Some tagFooters ->
                    match config.Tags with
                    | Some expectedTags ->
                        if List.forall (fun x -> List.contains x expectedTags) tagFooters then
                            Ok(body, footer)
                        else
                            let receivedTagsList =
                                tagFooters |> List.map (fun tag -> $"- {tag}") |> String.concat "\n"

                            Error
                                $"Unkonwn tag(s) in the footer.

Received:

%s{receivedTagsList}

But allowed tags are:

%s{expectedTagsList}"
                    | None -> Ok(body, footer)

                | None ->
                    let helpText =
                        if expectedTagsList.Length > 0 then
                            $"where tag is one of the following:

%s{expectedTagsList}"
                        else
                            ""

                    Error
                        $"Invalid commit message format.

Expected a 'Tag' footer %s{helpText}

Example:
-------------------------
feat: add new feature

This is the body of the commit message

Tag: converter
-------------------------"

    let tryParseCommitMessage (config: CommitParserConfig) (commit: string) =
        let lines = commit.Replace("\r\n", "\n").Split('\n') |> Array.toList

        let validate firstLine (linesAfterSubjetLine: string list) =
            result {
                let! firstLine = validateFirstLine config firstLine
                let! rawBody = validateRawBody linesAfterSubjetLine
                let! (body, footers) = validateBodyAndFooters config firstLine.Type rawBody

                return
                    {
                        Type = firstLine.Type
                        Scope = firstLine.Scope
                        Description = firstLine.Description
                        Body = body
                        BreakingChange =
                            firstLine.BreakingChange
                            || footers.ContainsKey "BREAKING CHANGE"
                            || footers.ContainsKey "BREAKING-CHANGE"
                        Footers = footers
                    }
            }

        match lines with
        // short commit message
        // Give it a chance to be valid, if the type allows it
        | subject :: [] -> validate subject []
        // short commit message + body description / footer
        | subject :: linesAfterSubject -> validate subject linesAfterSubject
        | _ -> Error invalidCommitMessage

    let tryValidateCommitMessage (config: CommitParserConfig) (commit: string) =
        tryParseCommitMessage config commit
        // Discard the parsed result
        |> Result.bind (fun _ -> Ok())
