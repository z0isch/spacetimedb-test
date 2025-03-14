.PHONY: publish-web
publish-web:
	dotnet publish ./WebClient/WebClient.csproj --configuration Release
	rsync -av --delete ./WebClient/bin/Release/net8.0/publish/ ./docs/

.PHONY: generate-bindings
generate-bindings:
	spacetime generate --lang csharp --out-dir WebClient/module_bindings --project-path server

.PHONY: publish-spacetime
publish-spacetime:
	spacetime publish --project-path server quickstart-chat
