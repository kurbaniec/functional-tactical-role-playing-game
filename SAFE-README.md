# functional-tactical-role-playing-game

# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

-   [.NET Core SDK](https://www.microsoft.com/net/download) 6.0 or higher
-   [Node 16](https://nodejs.org/en/download/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet run
```

Then open `http://localhost:8080` in your browser.

The build project in root directory contains a couple of different build targets. You can specify them after `--` (target name is case-insensitive).

To run concurrently server and client tests in watch mode (you can run this command in parallel to the previous one in new terminal):

```bash
dotnet run -- RunTests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

Finally, there are `Bundle` and `Azure` targets that you can use to package your app and deploy to Azure, respectively:

```bash
dotnet run -- Bundle
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

-   [Saturn](https://saturnframework.org/)
-   [Fable](https://fable.io/docs/)
-   [Elmish](https://elmish.github.io/elmish/)

## Sources

-   ###### [Animated Wizard](https://poly.pizza/m/kttbFvCl2C) by [Quaternius](https://poly.pizza/u/Quaternius) [[CC-BY](https://creativecommons.org/licenses/by/3.0/)] via Poly Pizza

-   ###### [Agile knight](https://poly.pizza/m/7aYuk5Rdlr-) by [Spiros Koutsourelis](https://poly.pizza/u/Spiros Koutsourelis) [[CC-BY](https://creativecommons.org/licenses/by/3.0/)] via Poly Pizza

-   ###### [Viking](https://poly.pizza/m/eVHUob4AIM3) by [Steve Schofield](https://poly.pizza/u/Steve Schofield) [[CC-BY](https://creativecommons.org/licenses/by/3.0/)] via Poly Pizza

-   ###### [Knight](https://poly.pizza/m/1TnT5Xc6vq) by [Vaporworks](https://poly.pizza/u/Vaporworks) [[CC-BY](https://creativecommons.org/licenses/by/3.0/)] via Poly Pizza

-   ###### [Rock](https://poly.pizza/m/RtLRqYjfMs) by [Quaternius](https://poly.pizza/u/Quaternius)

- ###### [Gold Rocks](https://poly.pizza/m/49NgnJzOHc) by [Quaternius](https://poly.pizza/u/Quaternius)
