﻿using Netryoshka.Extensions;
using Netryoshka.Json;
using Netryoshka.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Netryoshka.Services
{
    public class TSharkService
    {
        private readonly bool IsTraceDebugging = false;
        private readonly bool IsStandardErrorDebugging = false;
        // piecemeal has a performance cost but allows more granular cancellation
        private readonly bool IsPiecemealDeserialization = true; 

        private readonly ICaptureService _captureService;
        private readonly ILogger _logger;
        private readonly string _pcapFilePath;
        private readonly string _pcapngFilePath;
        private readonly string _foundKeysFilePath;

        public string TSharkExecutable { get; set; }
        public string EditCapExecutable { get; set; }
        public string KeysFile { get; set; }


        public TSharkService(ICaptureService captureService, ILogger logger)
        {
            _captureService = captureService;
            _logger = logger;

            var tempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(tempFolderPath)) { Directory.CreateDirectory(tempFolderPath); }
            _pcapFilePath = Path.Combine(tempFolderPath, "tshark_dummy_file.pcap");
            _pcapngFilePath = Path.Combine(tempFolderPath, "tshark_dummy_file.pcapng");
            _foundKeysFilePath = Path.Combine(tempFolderPath, "found_keys_dummy_file.txt");

            // TODO: make this configurable
            TSharkExecutable = @"C:\Users\rando\source\repos\wireshark\wireshark_build\run\RelWithDebInfo\tshark.exe";
            EditCapExecutable = @"C:\Users\rando\source\repos\wireshark\wireshark_build\run\RelWithDebInfo\editcap.exe";
            KeysFile = Environment.GetEnvironmentVariable("SSLKEYLOGFILE", EnvironmentVariableTarget.User) ?? "";
        }


        // TODO: write our own binary writer
        private async Task<byte[]> GetPcapBytesAsync(List<BasicPacket> packets, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            _captureService.WritePacketsToFile(_pcapFilePath, packets);
            ct.ThrowIfCancellationRequested();

            var randoms = await GetTlsHandshakeRandomsAsync(ct);
            var keys = GetKeysFromRandoms(randoms);
            CheckKeyAndRandomCounts(randoms, keys);
            ct.ThrowIfCancellationRequested();

            if (keys.Count > 0)
            {
                await WritePcapngWithKeys(keys, ct);
                ct.ThrowIfCancellationRequested();

                if (File.Exists(_pcapngFilePath))
                {
                    return File.ReadAllBytes(_pcapngFilePath);
                }
            }

            return File.ReadAllBytes(_pcapFilePath);
        }


        public async Task<string> SerializePacketsToJsonAsync(List<BasicPacket> packets, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            byte[] packetStream = await GetPcapBytesAsync(packets, ct);
            ct.ThrowIfCancellationRequested();

            stopwatch.Stop();
            _logger.Info($"Read {packets.Count} packets into  PCAP bytes in {stopwatch.ElapsedMilliseconds} ms");

            string json;

            var psi = new ProcessStartInfo
            {
                FileName = TSharkExecutable,
                Arguments = "-i - -T json", //  --no-duplicate-keys
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!File.Exists(TSharkExecutable))
            {
                throw new FileNotFoundException($"TShark executable not found: {TSharkExecutable}");
            }

            using (Process process = new() { StartInfo = psi })
            {
                stopwatch.Restart();
                ct.ThrowIfCancellationRequested();

                try
                {
                    process.Start();

                    using (Stream stdin = process.StandardInput.BaseStream)
                    using (StreamReader sr = process.StandardOutput)
                    using (StreamReader serr = process.StandardError)
                    {
                        await stdin.WriteAsync(packetStream, ct).ConfigureAwait(false);
                        stdin.Close();
                        ct.ThrowIfCancellationRequested();

                        json = await sr.ReadToEndAsync(ct).ConfigureAwait(false);
                        ct.ThrowIfCancellationRequested();

                        var errorResult = await serr.ReadToEndAsync(ct).ConfigureAwait(false);
                        if (IsStandardErrorDebugging && !string.IsNullOrEmpty(errorResult))
                        {
                            _logger.Info(errorResult);
                        }
                    }


                    var waitForExitTask = process.WaitForExitAsync(ct);
                    var delayTask = Task.Delay(TimeSpan.FromSeconds(10), ct);
                    var completedTask = await Task.WhenAny(waitForExitTask, delayTask);
                    if (completedTask == delayTask && !waitForExitTask.IsCompleted)
                    {
                        throw new TimeoutException("TShark process did not exit in time.");
                    }
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("SerializePacketsToJsonAsync was canceled.");
                    process.Kill();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"An exception occurred: {ex.Message}", ex);
                    process.Kill();
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    _logger.Info($"Serialized {packets.Count} packets ({packetStream.Length / 1048576.0:N2} MB) into JSON in {stopwatch.ElapsedMilliseconds} ms");
                }
            }

            return json;
        }
        

        public async Task<List<WireSharkData>> ConvertToWireSharkDataAsync(List<BasicPacket> packets, CancellationToken ct, IProgress<double> progress)
        {
            ct.ThrowIfCancellationRequested();

            var json = await SerializePacketsToJsonAsync(packets, ct);
            ct.ThrowIfCancellationRequested();

            var jsonList = JsonUtils.SplitJsonObjects(json);
            ct.ThrowIfCancellationRequested();

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            if (IsTraceDebugging)
            {
                serializerSettings.TraceWriter = new CustomTraceWriter();
            }

            var sharkPackets = new List<WireSharkPacket>();

            var stopwatch = Stopwatch.StartNew();
            if (IsPiecemealDeserialization)
            {
                int totalPackets = jsonList.Count;
                for (int i = 0; i < totalPackets; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var sharkPacket = JsonConvert.DeserializeObject<WireSharkPacket>(jsonList[i], serializerSettings)
                        ?? throw new JsonException("Failed to deserialize a WireSharkPacket.");
                    sharkPackets.Add(sharkPacket);

                    progress.Report((double)i / totalPackets);
                }
            }
            else
            {
                sharkPackets = JsonConvert.DeserializeObject<List<WireSharkPacket>>(json, serializerSettings)
                    ?? throw new JsonException("Failed to deserialize json to WireSharkPacket list");
            }
            stopwatch.Stop();
            _logger.Info($"Deserialized {sharkPackets.Count} packets in {stopwatch.ElapsedMilliseconds} ms");

            if (packets.Count != jsonList.Count || jsonList.Count != sharkPackets.Count)
            {
                throw new InvalidOperationException($"Mismatch in element counts: packets ({packets.Count}), jsonList ({jsonList.Count}), sharkPackets ({sharkPackets.Count})");
            }

            var sharkData = new List<WireSharkData>();
            for (int i = 0; i < packets.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                sharkData.Add(new WireSharkData(jsonList[i], sharkPackets[i]));
            }

            return sharkData;
        }


        public async Task<List<string>> GetTlsHandshakeRandomsAsync(CancellationToken ct)
        {
            var tlsHandshakeInfoProcess = new ProcessStartInfo
            {
                FileName = TSharkExecutable,
                Arguments = 
                    $"-otls.keylog_file:{KeysFile} " +  // Set the TLS key log file to decrypt TLS traffic
                    "-Tfields " +                       // Output format will be custom fields defined by the `-e` options
                    "-Ytls.handshake.type==1 " +        // Filter packets to only show TLS handshake type 1 (ClientHello)
                    "-etls.handshake.random " +         // Extract only the 'tls.handshake.random' field
                    $"-r {_pcapFilePath}",              // Read packet data from the given pcap file
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var tlsHandshakeRandomList = new List<string>();

            if (!File.Exists(TSharkExecutable))
            {
                throw new FileNotFoundException($"TShark executable not found: {TSharkExecutable}");
            }

            if (!File.Exists(KeysFile))
            {
                _logger.Warn($"Keys file not found: {KeysFile}");
                return tlsHandshakeRandomList;
            }

            using (Process process = new() { StartInfo = tlsHandshakeInfoProcess })
            {
                try
                {
                    process.Start();

                    using (StreamReader sr = process.StandardOutput)
                    using (StreamReader serr = process.StandardError)
                    {

                        tlsHandshakeRandomList = (await sr.ReadToEndAsync(ct))
                            .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                        var errorResult = await serr.ReadToEndAsync(ct).ConfigureAwait(false);
                        if (IsStandardErrorDebugging && !string.IsNullOrEmpty(errorResult))
                        {
                            _logger.Info(errorResult);
                        }
                    }

                    await process.WaitForExitAsync(ct);
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

            return tlsHandshakeRandomList;
        }


        public List<string> GetKeysFromRandoms(List<string> randoms)
        {
            if (!File.Exists(KeysFile))
            {
                _logger.Error($"Keys file not found: {KeysFile}");
                return new List<string>();
            }

            var keys = new List<string>();

            using (FileStream fs = new(KeysFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using var sr = new StreamReader(fs);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (randoms.Any(random => line.Matches($@"\b{random}\b", RegexOptions.IgnoreCase).Any()))
                    {
                        keys.Add(line);
                    }
                }
            }

            return keys;
        }


        public void CheckKeyAndRandomCounts(List<string> tlsHandshakeRandoms, List<string> keys)
        {
            int nrands = tlsHandshakeRandoms.Count;
            int nkeys = keys.Count;
            int nkeys_unique = keys.Distinct().Count();

            var explainMissingSessions = @$"Potential reasons for this:
 - TLS runs on a custom port. Use 'Decode As' 'TCP Port' -> TLS.
 - The packet capture was started before keys were captured.
 - The TLS handshake was not captured, try restarting the connection.";

            var explainMissingKeys = @$"Potential reasons for this: 
 - The TLS handshake was not completed.
 - Traffic goes through multiple hosts or programs and are
   reencrypted (proxied), but keys are captured from the wrong one.";

            if (nrands == 0)
            {
                _logger.Warn("No TLS sessions found.");
                _logger.Info(explainMissingSessions);
            }
            else if (nkeys == 0)
            {
                _logger.Warn($"No secrets found for {nrands} sessions.");
                _logger.Info(explainMissingKeys);
            }
            else if (nrands > nkeys_unique)
            {
                _logger.Warn($"Note: found keys for {nkeys_unique} sessions, but there are more sessions in total ({nrands})");
                _logger.Info(explainMissingKeys);
            }
            else if (nrands < nkeys_unique)
            {
                _logger.Warn($"Note: found keys for {nkeys_unique} sessions, but there are fewer sessions in total ({nrands})");
                _logger.Info(explainMissingSessions);
            }
        }


        public async Task WritePcapngWithKeys(List<string> keys, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = EditCapExecutable,
                Arguments = $"--discard-all-secrets --inject-secrets tls,{_foundKeysFilePath} {_pcapFilePath} {_pcapngFilePath}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // delete the pcapng file. if something goes wrong, the caller can decide what to do
            if (File.Exists(_pcapngFilePath))
            {
                File.Delete(_pcapngFilePath);
            }

            try
            {
                File.WriteAllLines(_foundKeysFilePath, keys);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while writing keys to {_foundKeysFilePath}: {ex.Message}", ex);
            }

            if (!File.Exists(EditCapExecutable))
            {
                throw new FileNotFoundException($"Editcap executable not found: {EditCapExecutable}");
            }

            if (!File.Exists(_pcapngFilePath))
            {
                File.Create(_pcapngFilePath).Close();
            }

            using Process process = new() { StartInfo = psi };
            try
            {
                process.Start();

                using (StreamReader sr = process.StandardOutput)
                using (StreamReader serr = process.StandardError)
                {
                    await sr.ReadToEndAsync(ct);

                    var errorResult = await serr.ReadToEndAsync(ct).ConfigureAwait(false);
                    if (IsStandardErrorDebugging && !string.IsNullOrEmpty(errorResult))
                    {
                        _logger.Info(errorResult);
                    }
                }

                await process.WaitForExitAsync(ct);
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


    }
}
