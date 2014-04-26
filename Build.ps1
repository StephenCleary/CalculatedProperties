$ErrorActionPreference = "Stop"

# Build solution
$project = get-project
$build = $project.DTE.Solution.SolutionBuild
$oldConfiguration = $build.ActiveConfiguration
$build.SolutionConfigurations.Item("Release").Activate()
$build.Build($true)
$oldConfiguration.Activate()

nuget pack -Symbols "CalculatedProperties\CalculatedProperties.csproj" -Prop Configuration=Release
