using Netryoshka.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Netryoshka.Services
{
    //tshark -r my_capture.pcap -Y 'http' -T fields -E header=y -E separator=, -E quote=d -e frame.number -e ip.src -e ip.dst -e http.request.method -e http.request.uri -e http.response.code

    public class TSharkService
    {
        private string TSharkFileName { get; }
        private readonly ProcessStartInfo _psi;
        private readonly ICaptureService _captureService;
        private readonly ILogger _logger;

        public TSharkService(ICaptureService captureService, ILogger logger)
        {
            _captureService = captureService;
            _logger = logger;
            TSharkFileName = @"C:\Users\rando\source\repos\wireshark\wireshark_build\run\RelWithDebInfo\tshark.exe";
            _psi = new ProcessStartInfo
            {
                FileName = TSharkFileName,
                Arguments = "-i - -T json", //"-Y http",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        // TODO: write our own binary writer
        private byte[] GetPcapBytes(List<BasicPacket> packets)
        {
            var binFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(binFolderPath)) { Directory.CreateDirectory(binFolderPath); }
            var dummyFilePath = Path.Combine(binFolderPath, "tshark_dummy_file.pcap");

            _captureService.WritePacketsToFile(dummyFilePath, packets);

            byte[] fileBytes = File.ReadAllBytes(dummyFilePath);
            return fileBytes;
        }

        private string SerializePacketsToJson(List<BasicPacket> packets)
        {
            byte[] packetStream = GetPcapBytes(packets);
            string parsedResult;

            using (Process process = new Process { StartInfo = _psi })
            {
                process.Start();

                using Stream stdin = process.StandardInput.BaseStream;
                using StreamReader sr = process.StandardOutput;

                stdin.Write(packetStream, 0, packetStream.Length);
                parsedResult = sr.ReadToEnd();
            }
            return parsedResult;
        }


        public async Task<string> SerializePacketsToJsonAsync(List<BasicPacket> packets, CancellationToken cancellationToken)
        {
            byte[] packetStream = GetPcapBytes(packets);
            string json;

            using (Process process = new() { StartInfo = _psi })
            {
                try
                {
                    process.Start();

                    using (Stream stdin = process.StandardInput.BaseStream)
                    using (StreamReader sr = process.StandardOutput)
                    using (StreamReader serr = process.StandardError)
                    {
                        await stdin.WriteAsync(packetStream, cancellationToken).ConfigureAwait(false);
                        stdin.Close();

                        json = await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                        var errorResult = await serr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(errorResult))
                        {
                            _logger.Info(errorResult);
                        }
                    }

                    // Add timeout or cancellation logic?
                    process.WaitForExit();  
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Operation was canceled.");
                    process.Kill();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"An exception occurred: {ex.Message}", ex);
                    process.Kill();
                    throw;
                }
            }

            return json;
        }

        public async Task<string> SerializePacketsToJsonAsync(List<BasicPacket> packets)
        {
            byte[] packetStream = GetPcapBytes(packets);
            string json;

            using (Process process = new() { StartInfo = _psi })
            {
                process.Start();

                using (Stream stdin = process.StandardInput.BaseStream)
                using (StreamReader sr = process.StandardOutput)
                using (StreamReader serr = process.StandardError)
                {
                    await stdin.WriteAsync(packetStream);
                    stdin.Close();
                    json = await sr.ReadToEndAsync();

                    var errorResult = await serr.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(errorResult))
                    {
                        _logger.Info(errorResult);
                    }
                }
                process.WaitForExit();
            }
            return json;
        }

        public async Task<List<WireSharkData>> ConvertToWireSharkDataAsync(List<BasicPacket> packets, CancellationToken cts)
        {
            var json = await SerializePacketsToJsonAsync(packets, cts);

            var jsonList = Util.SplitJsonObjects(json);
            var sharkPackets = JsonConvert.DeserializeObject<List<WireSharkPacket>>(json) 
                ?? throw new InvalidOperationException("Failed to deserialize json to WireSharkPacket list");

            if (packets.Count != jsonList.Count || jsonList.Count != sharkPackets.Count)
            {
                throw new InvalidOperationException($"Mismatch in element counts: packets ({packets.Count}), jsonList ({jsonList.Count}), sharkPackets ({sharkPackets.Count})");
            }

            var sharkData = new List<WireSharkData>();
            for (int i = 0; i < packets.Count; i++)
            {
                sharkData.Add(new WireSharkData(jsonList[i], sharkPackets[i]));
            }

            return sharkData;
        }

        public static List<WireSharkPacket> DeserializeToTSharkPackets(string json)
        {
            return JsonConvert.DeserializeObject<List<WireSharkPacket>>(json) ?? new List<WireSharkPacket>();
        }
        
    }

}
