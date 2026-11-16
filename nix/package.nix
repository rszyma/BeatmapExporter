{
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  ffmpeg,
  fontconfig,
  libGL,
  libICE,
  libSM,
  libX11,
  libXcursor,
  libXext,
  libXi,
  libXrandr,
  variant ? "gui",
}:

let
  isGui = variant == "gui";
in
buildDotnetModule {
  pname = if isGui then "beatmap-exporter" else "beatmap-exporter-cli";
  version = lib.trim (builtins.readFile ../VERSION);

  src = lib.fileset.toSource {
    root = ../.;
    fileset = lib.fileset.unions [
      ../BeatmapExporter.sln
      ../BeatmapExporterCore
      ../BeatmapExporterCLI
      ../BeatmapExporterGUI
      ../BeatmapExporterGUI.Desktop
    ];
  };

  projectFile =
    if isGui then
      "BeatmapExporterGUI.Desktop/BeatmapExporterGUI.Desktop.csproj"
    else
      "BeatmapExporterCLI/BeatmapExporterCLI.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = dotnetCorePackages.runtime_10_0;

  # The csproj files enable single-file publish for Windows releases.
  dotnetFlags = [
    "-p:PublishSingleFile=false"
    "-p:IncludeNativeLibrariesForSelfExtract=false"
  ];

  executables = [ (if isGui then "BeatmapExporterGUI.Desktop" else "BeatmapExporterCLI") ];

  runtimeDeps = lib.optionals isGui [
    fontconfig
    libGL
    libICE
    libSM
    libX11
    libXcursor
    libXext
    libXi
    libXrandr
  ];

  # FFmpeg is used to transcode non-mp3 audio during audio export.
  makeWrapperArgs = [
    "--prefix"
    "PATH"
    ":"
    (lib.makeBinPath [ ffmpeg ])
  ];

  postFixup = lib.optionalString isGui ''
    mv $out/bin/BeatmapExporterGUI.Desktop $out/bin/BeatmapExporter
  '';

  meta = {
    description = "Beatmap exporter utility for osu!lazer";
    homepage = "https://github.com/kabiiQ/BeatmapExporter";
    license = lib.licenses.mit;
    mainProgram = if isGui then "BeatmapExporter" else "BeatmapExporterCLI";
    platforms = lib.platforms.linux;
  };
}
