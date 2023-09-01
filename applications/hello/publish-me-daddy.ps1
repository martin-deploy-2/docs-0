param([String] $Tag)

dotnet publish "$PSScriptRoot" --configuration Release --no-self-contained --output "$PSScriptRoot/bin/publish"
nerdctl image build --tag "hello:$Tag" --namespace "k8s.io" "$PSScriptRoot"
