## To switch from project references to nugets

* Change Meadow.Foundation branch to develop (don't forget to pull)
* Create a new release branch - e.g. `release/RC1`
* Clone https://github.com/adrianstevens/SolutionReferenceSwitcher
* Open in Visual Studio (tested using VS2022 on Windows)
* Open `Program.cs`
* Update `MeadowFoundationPath` to point to your Meadow.Foundation folder
* Ensure `SwitchToPublishingMode` is being called and `SwitchToDeveloperMode` in `Main`
* Run `SolutionReferenceSwitcher`
* Open Meadow.Foundation sln
* Remove `_External` solution folder and all projects within
* Cross fingers and rebuild M.F. solution

## To troubleshoot

Open Meadow.Foundation folder in Visual Studio Code and search for `ProjectReference Include`. You should see references to projects for unpublished Nugets only.