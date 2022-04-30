Invoke-WebRequest -OutFile openapi-generator-cli.jar https://repo1.maven.org/maven2/org/openapitools/openapi-generator-cli/5.4.0/openapi-generator-cli-5.4.0.jar
Invoke-WebRequest -OutFile open-meteo_openapi.yaml https://github.com/open-meteo/open-meteo/blob/1.5.0/openapi.yml

java -jar openapi-generator-cli.jar generate -g csharp-netcore -c openapi_config.json
