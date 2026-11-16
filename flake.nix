{
  description = "BeatmapExporter - beatmap exporter utility for osu!lazer";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs =
    { self, nixpkgs }:
    let
      systems = [
        "x86_64-linux"
        "aarch64-linux"
      ];
      forAllSystems = f: nixpkgs.lib.genAttrs systems (system: f nixpkgs.legacyPackages.${system});
    in
    {
      packages = forAllSystems (pkgs: rec {
        gui = pkgs.callPackage ./nix/package.nix { };
        cli = pkgs.callPackage ./nix/package.nix { variant = "cli"; };
        default = gui;
      });

      apps = forAllSystems (pkgs: {
        default = {
          type = "app";
          program = "${self.packages.${pkgs.system}.gui}/bin/BeatmapExporter";
        };
        cli = {
          type = "app";
          program = "${self.packages.${pkgs.system}.cli}/bin/BeatmapExporterCLI";
        };
      });
    };
}
