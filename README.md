# Cogito.Components

[![Build](https://github.com/alethic/Cogito.Components/actions/workflows/Cogito.Components.yml/badge.svg)](https://github.com/alethic/Cogito.Components/actions/workflows/Cogito.Components.yml)

A small host for long-running work: implement IRunnable, register it, and the host finds and runs it.

## Packages

**[Cogito.Components](https://www.nuget.org/packages/Cogito.Components)** — A small host for long-running work, built on Autofac assembly-module scanning.

Each package carries its own README with the detail; the links above go to nuget.org.

## Building

```shell
dotnet restore Cogito.Components.sln
dotnet msbuild -p:Configuration=Release Cogito.Components.dist.msbuildproj
```

Packages are staged into `dist/nuget` and test suites into `dist/tests`; run a suite with
`dotnet test -f <tfm> <path to its assembly>`.

## License

MIT — see [LICENSE](LICENSE).
