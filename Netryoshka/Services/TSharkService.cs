using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Netryoshka.Services
{
    //tshark -r my_capture.pcap -Y 'http' -T fields -E header=y -E separator=, -E quote=d -e frame.number -e ip.src -e ip.dst -e http.request.method -e http.request.uri -e http.response.code

    public class TSharkService
    {
        private string TSharkFileName { get; }
        private ProcessStartInfo psi;
        private readonly ICaptureService _captureService;

        public TSharkService(ICaptureService captureService)
        {
            _captureService = captureService;
            TSharkFileName = @"C:\Users\rando\source\repos\wireshark\wireshark_build\run\RelWithDebInfo\tshark.exe";
            psi = new ProcessStartInfo
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
        public byte[] GetPcapBytes(List<BasicPacket> packets)
        {
            var binFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(binFolderPath)) { Directory.CreateDirectory(binFolderPath); }
            var dummyFilePath = Path.Combine(binFolderPath, "tshark_dummy_file.pcap");

            _captureService.WritePacketsToFile(dummyFilePath, packets);

            byte[] fileBytes = File.ReadAllBytes(dummyFilePath);
            return fileBytes;
        }


        public string ParseHttpPackets(List<BasicPacket> packets)
        {

            byte[] packetStream = GetPcapBytes(packets);
            string parsedResult;

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();

                using Stream stdin = process.StandardInput.BaseStream;
                using StreamReader sr = process.StandardOutput;

                stdin.Write(packetStream, 0, packetStream.Length);
                parsedResult = sr.ReadToEnd();
            }
            return parsedResult;
        }



        public async Task<string> ParseHttpPacketsAsync(List<BasicPacket> packets)
        {
            byte[] packetStream = GetPcapBytes(packets);
            string parsedResult;
            string errorResult;
            

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();

                using (Stream stdin = process.StandardInput.BaseStream)
                using (StreamReader sr = process.StandardOutput)
                using (StreamReader serr = process.StandardError)
                {
                    await stdin.WriteAsync(packetStream);
                    stdin.Close();
                    parsedResult = await sr.ReadToEndAsync();
                    errorResult = await serr.ReadToEndAsync();
                }
                process.WaitForExit();
            }
            var tSharkPackets = JsonConvert.DeserializeObject<List<TSharkPacket>>(parsedResult);
            //return tSharkPackets
            return parsedResult;
        }

        //public async Task<string> ParseHttpPacketsAsync(List<BasicPacket> packets, CancellationToken cancellationToken)
        //{
        //    byte[] packetStream = GetPcapBytes(packets);
        //    string parsedResult;
        //    string errorResult;

        //    using (Process process = new Process { StartInfo = psi })
        //    {
        //        process.Start();

        //        using (Stream stdin = process.StandardInput.BaseStream)
        //        using (StreamReader sr = process.StandardOutput)
        //        using (StreamReader serr = process.StandardError)
        //        {
        //            var writeTask = stdin.WriteAsync(packetStream, cancellationToken);
        //            var readTask = sr.ReadToEndAsync();
        //            var readErrorTask = serr.ReadToEndAsync();

        //            var completedTask = await Task.WhenAny(Task.WhenAll(writeTask, readTask, readErrorTask), Task.Delay(5000));  // 5-second timeout

        //            if (completedTask == readTask)
        //            {
        //                parsedResult = await readTask;
        //                errorResult = await readErrorTask;
        //            }
        //            else
        //            {
        //                cancellationToken.ThrowIfCancellationRequested();
        //                throw new TimeoutException("Parsing HTTP packets took too long.");
        //            }
        //        }
        //        process.WaitForExit();
        //    }
        //    return parsedResult;
        //}




    }

}
