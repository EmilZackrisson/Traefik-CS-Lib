# Get the version number
newVersion=$(cat Traefik.csproj | grep -m 1 "<Version>" | sed 's/[^0-9.]*//g')

# Commit the new version number
git add Traefik.csproj
git commit -m "Bump version to $newVersion"

# Tag the new version
git tag $newVersion

# Push the new version
git push
git push --tags

# Build and publish the new version
dotnet build -c Release
dotnet pack -c Release
dotnet nuget push -s "github" --skip-duplicate ./bin/Release/Traefik.*.nupkg