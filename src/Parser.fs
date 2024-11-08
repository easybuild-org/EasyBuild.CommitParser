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

[optional tag line]

[optional body]

[optional footer]

Example:
-------------------------
feat: add new feature

[tag1][tag2]

This is the body of the commit message

This is the footer of the commit message
-------------------------"

    let private firstLineRegex =
        Regex("^(?<type>[^(!:]+)(\((?<scope>.+)\))?(?<breakingChange>!)?: (?<description>.{1,})$")

    // Matches one or more tags
    // We want the regex to fail if there are no tags
    // the validation will still be able to give a chance to the commit message to be valid
    // by checking the SkipTagLine property of the type
    let private tagLineRegex = Regex("^(\[(?<tag>[^\]]+)\])+$")

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

    let internal validateSecondLine (line: string) =
        if String.IsNullOrWhiteSpace line then
            Ok()
        else
            Error
                "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"

    let internal validateLinesAfterTagLine
        (previousLine: string)
        (lines: string list)
        (tags: string list option)
        =
        let errorMessage =
            "Invalid commit message format.

Expected an empty line after the tag line when adding a body or a footer.

Example:
-------------------------
feat: add new feature

[tag1][tag2]

This is the body of the commit message
with a second line
-------------------------"

        match tags with
        | None ->
            // If there are no tags, the previous line is considered as part of the body
            previousLine :: lines |> String.concat "\n" |> Ok
        | Some _ ->
            match lines with
            | [] -> Ok ""
            | firstLine :: rest ->
                if String.IsNullOrWhiteSpace firstLine then
                    rest |> String.concat "\n" |> Ok
                else
                    Error errorMessage

    let internal validateTagLine
        (config: CommitParserConfig)
        (commitMessage: FirstLineParsedResult)
        (line: string)
        =
        let typeConfigOpt =
            config.Types
            |> List.tryFind (fun currentType -> currentType.Name = commitMessage.Type)

        match typeConfigOpt with
        | None ->
            // In theory, this should never happen because we already validated the type
            Error "Commit message type not found in the configuration."

        | Some typeConfig ->
            let m = tagLineRegex.Match(line)

            if m.Success then
                m.Groups["tag"].Captures
                |> Seq.map (fun group -> group.Value)
                |> Seq.toList
                |> Some
                |> Ok
            // If the type is flagged as 'SkipTagLine', we can still consider the commit message as valid
            else if typeConfig.SkipTagLine then
                Ok None
            else
                let helpText =
                    match config.Tags with
                    | Some tags ->
                        let expectedTagsText =
                            tags |> List.map (fun tag -> $"- {tag}") |> String.concat "\n"

                        $"
Where tag is one of the following:

%s{expectedTagsText}
"

                    | None -> ""

                Error
                    $"Invalid commit message format.

Expected a tag line with the following format: '[tag1][tag2]...[tagN]'
%s{helpText}
Example:
-------------------------
feat: add new feature

[tag1][tag2]
-------------------------"

    let tryParseCommitMessage (config: CommitParserConfig) (commit: string) =
        let lines = commit.Replace("\r\n", "\n").Split('\n') |> Array.toList

        let validate firstLine secondLine tagLine (linesAfterTagLine: string list) =
            result {
                let! firstLine = validateFirstLine config firstLine
                let! _ = validateSecondLine secondLine
                let! tags = validateTagLine config firstLine tagLine
                let! bodyOrFooter = validateLinesAfterTagLine tagLine linesAfterTagLine tags

                return
                    {
                        Type = firstLine.Type
                        Scope = firstLine.Scope
                        Description = firstLine.Description
                        Body = bodyOrFooter
                        BreakingChange = firstLine.BreakingChange
                        Tags = tags
                    }
            }

        match lines with
        // short commit message
        // Give it a chance to be valid, if the type allows it
        | commit :: [] -> validate commit "" "" []
        | commit :: secondLine :: [] -> validate commit secondLine "" []
        // short commit message + tag line
        | commit :: secondLine :: tagLine :: [] -> validate commit secondLine tagLine []
        // short commit message + tag line + body description / footer
        | commit :: secondLine :: tagLine :: linesAfterTagLine ->
            validate commit secondLine tagLine linesAfterTagLine
        | _ -> Error invalidCommitMessage

    let tryValidateCommitMessage (config: CommitParserConfig) (commit: string) =
        tryParseCommitMessage config commit
        // Discard the parsed result
        |> Result.bind (fun _ -> Ok())
