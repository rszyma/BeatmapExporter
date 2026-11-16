using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace BeatmapExporterCore.Utilities
{
    public class Transcoder
    {
        public Transcoder()
        {
            try
            {
                // Resolve FFmpeg to an absolute location so that the OS process search
                // never picks up an ffmpeg executable from the current working directory.
                var binaryFolder = FindFFmpegFolder();
                if (binaryFolder is null)
                {
                    Available = false;
                    return;
                }
                GlobalFFOptions.Configure(options => options.BinaryFolder = binaryFolder);
                var _ = FFMpeg.GetAudioCodecs();
                Available = true;
            }
            catch (Exception)
            {
                Available = false;
            }
        }

        /// <summary>
        /// Finds the directory containing the FFmpeg executable: the application directory first, then each absolute PATH entry.
        /// </summary>
        private static string? FindFFmpegFolder()
        {
            string exe = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
            var candidates = new List<string> { AppContext.BaseDirectory };
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            candidates.AddRange(path
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().Trim('"'))
                .Where(Path.IsPathFullyQualified));

            return candidates.FirstOrDefault(dir => File.Exists(Path.Combine(dir, exe)));
        }

        /// <summary>
        /// If the FFMpeg runtime was successfully when this Transcoder was initalized.
        /// </summary>
        public bool Available { get; }

        /// <summary>
        /// Performs a transcode on an audio file into MP3 format, saving it to disk.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the FFMpeg runtime was not found on application launch. This condition can be checked before attempting the transcode, <see cref="Available" /></exception>
        public bool TranscodeMP3(FileStream sourceFile, string destFile)
        {
            if (!Available) throw new InvalidOperationException("Audio transcoder is not available within this runtime.");

            return FFMpegArguments
                .FromPipeInput(new StreamPipeSource(sourceFile))
                .OutputToFile(destFile, overwrite: false, o => o.WithAudioCodec(AudioCodec.LibMp3Lame))
                .ProcessSynchronously();
        }
    }
}
