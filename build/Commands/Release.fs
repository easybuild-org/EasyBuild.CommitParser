module EasyBuild.Commands.Release

open Spectre.Console.Cli
open EasyBuild.Workspace
open EasyBuild.Tools.DotNet
open EasyBuild.Tools.Git

type ReleaseSettings() =
    inherit CommandSettings()

type ReleaseCommand() =
    inherit Command<ReleaseSettings>()
    interface ICommandLimiter<ReleaseSettings>

    override __.Execute(context, settings) =
        Test.TestCommand().Execute(context, Test.TestSettings()) |> ignore

        let newVersion = DotNet.changelogGen Workspace.``CHANGELOG.md``

        let nupkgPath = DotNet.pack (workingDirectory = Workspace.src.``.``)

        DotNet.nugetPush nupkgPath

        Git.addAll ()
        Git.commitRelease newVersion
        Git.push ()

        0
