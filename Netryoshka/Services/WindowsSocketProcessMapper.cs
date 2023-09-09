using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Netty
{
    public partial class WindowsSocketProcessMapper : ISocketProcessMapperService
    {
        // The version of IP used by the TCP/UDP endpoint. AF_INET is used for IPv4.
        private const int AF_INET = 2;

        // The GetExtendedTcpTable function retrieves a table that contains a list of
        // TCP endpoints available to the application. Decorating the function with
        // DllImport attribute indicates that the attributed method is exposed by an
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point.
        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            [MarshalAs(UnmanagedType.Bool)] bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        /*[DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);*/

        // The GetExtendedUdpTable function retrieves a table that contains a list of
        // UDP endpoints available to the application. Decorating the function with
        // DllImport attribute indicates that the attributed method is exposed by an
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point.
        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            [MarshalAs(UnmanagedType.Bool)] bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        /* [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);*/


        /// <summary>
        /// This function reads and parses the active TCP socket connections available
        /// and stores them in a list.
        /// </summary>
        /// <returns>
        /// It returns the current set of TCP socket connections which are active.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there
        /// is insufficient memory to satisfy the request.
        /// </exception>
        public List<TcpProcessRecord> GetAllTcpConnections()
        {
            var processes = Process.GetProcesses().ToDictionary(p => p.Id, p => p);

            int bufferSize = 0;
            var tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable.
            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the
            // specified number of bytes in 'bufferSize' variable.
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous
                // call must be used in this subsequent call to 'GetExtendedTcpTable'
                // function in order to successfully retrieve the table.
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed,
                // hence empty list is returned to the caller function.
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID'
                // to get number of entries of the specified TCP table structure.
                var tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)(Marshal.PtrToStructure(tcpTableRecordsPtr, typeof(MIB_TCPTABLE_OWNER_PID))
                    ?? throw new Exception("Failed to marshal TCP records table."));


                IntPtr tableRowPtr = checked((IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries)));

                // Reading and parsing the TCP records one by one from the table and
                // storing them in a list of 'TcpProcessRecord' structure type objects.
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    var tcpRow = (MIB_TCPROW_OWNER_PID)(Marshal.PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID))
                        ?? throw new Exception($"Failed to marshal TCP row for entry {row}."));

                    tcpTableRecords.Add(new TcpProcessRecord(
                        new IPAddress(tcpRow.localAddr),
                        new IPAddress(tcpRow.remoteAddr),
                        BitConverter.ToUInt16(new byte[2] {
                            tcpRow.localPort[1],
                            tcpRow.localPort[0] }, 0),
                        BitConverter.ToUInt16(new byte[2] {
                            tcpRow.remotePort[1],
                            tcpRow.remotePort[0] }, 0),
                        tcpRow.owningPid,
                        new WindowsTcpProcessState(tcpRow.state), //tcpRow.state,
                        processes.GetValueOrDefault(tcpRow.owningPid)?.ProcessName));
                    tableRowPtr = checked((IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow)));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                throw new Exception("Out Of Memory", outOfMemoryException);
            }
            catch (Exception exception)
            {
                throw new Exception("Exception", exception);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();
        }


        /// <summary>
        /// This function reads and parses the active UDP socket connections available
        /// and stores them in a list.
        /// </summary>
        /// <returns>
        /// It returns the current set of UDP socket connections which are active.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there
        /// is insufficient memory to satisfy the request.
        /// </exception>
        public List<UdpProcessRecord> GetAllUdpConnections()
        {
            int bufferSize = 0;
            var udpTableRecords = new List<UdpProcessRecord>();
            var processes = Process.GetProcesses().ToDictionary(p => p.Id, p => p);

            // Getting the size of UDP table, that is returned in 'bufferSize' variable.
            uint result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

            // Allocating memory from the unmanaged memory of the process by using the
            // specified number of bytes in 'bufferSize' variable.
            IntPtr udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous
                // call must be used in this subsequent call to 'GetExtendedUdpTable'
                // function in order to successfully retrieve the table.
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

                // Non-zero value represent the function 'GetExtendedUdpTable' failed,
                // hence empty list is returned to the caller function.
                if (result != 0)
                    return new List<UdpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated
                // managed object 'udpRecordsTable' of type 'MIB_UDPTABLE_OWNER_PID'
                // to get number of entries of the specified TCP table structure.
                var udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)(Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID))
                    ?? throw new Exception("Failed to marshal UDP records table."));
                IntPtr tableRowPtr = checked((IntPtr)((long)udpTableRecordPtr + Marshal.SizeOf(udpRecordsTable.dwNumEntries)));

                // Reading and parsing the UDP records one by one from the table and
                // storing them in a list of 'UdpProcessRecord' structure type objects.
                for (int row = 0; row < udpRecordsTable.dwNumEntries; row++)
                {
                    var udpRow = (MIB_UDPROW_OWNER_PID)(Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID))
                        ?? throw new Exception($"Failed to marshal UDP row for entry {row}."));

                    udpTableRecords.Add(new UdpProcessRecord(
                        new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new byte[2] { udpRow.localPort[1],
                            udpRow.localPort[0] }, 0),
                        udpRow.owningPid,
                        processes.GetValueOrDefault(udpRow.owningPid)?.ProcessName));
                    tableRowPtr = checked((IntPtr)((long)tableRowPtr + Marshal.SizeOf(udpRow)));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                throw new Exception("Out Of Memory", outOfMemoryException);
            }
            catch (Exception exception)
            {
                throw new Exception("Exception", exception);
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }
            return udpTableRecords != null ? udpTableRecords.Distinct()
                .ToList<UdpProcessRecord>() : new List<UdpProcessRecord>();
        }

        //private void tscProtocolType_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    // If the protocol type is selected as TCP then 'GetAllTcpConnections'
        //    // function is called or else if the protocol type is selected as UDP then
        //    // 'GetAllUdpConnections' is called to fetch the socket data. The data is
        //    // fetched automatically at an interval of 1 ms if the timer is enabled.
        //    // The retrieved active connection data is populated in the DataGridView.
        //    if (tscProtocolType.SelectedIndex == (int)Protocol.TCP)
        //    {
        //        if (tmrDataRefreshTimer.Enabled)
        //        {
        //            TcpActiveConnections = GetAllTcpConnections();
        //        }
        //        gdvSocketConnections.DataSource = TcpActiveConnections;
        //        tslTotalRecords.Text = (tmrDataRefreshTimer.Enabled ? "[Capturing active"
        //        + " connections...]   " : "") + "Total number of TCP connections : " +
        //        gdvSocketConnections.RowCount.ToString() + " records";
        //    }
        //    else if (tscProtocolType.SelectedIndex == (int)Protocol.UDP)
        //    {
        //        if (tmrDataRefreshTimer.Enabled)
        //        {
        //            UdpActiveConnections = GetAllUdpConnections();
        //        }
        //        gdvSocketConnections.DataSource = UdpActiveConnections;
        //        tslTotalRecords.Text = (tmrDataRefreshTimer.Enabled ? "[Capturing active"
        //        + " connections...]   " : "") + "Total number of UDP connections : " +
        //        gdvSocketConnections.RowCount.ToString() + " records";
        //    }
        //}

        public async Task<List<int>> GetLocalTcpPortsFromProcesses(HashSet<string> processNames, HashSet<int> processIDs)
        {
            var tcpConnections = await Task.Run(() => GetAllTcpConnections());
            return tcpConnections
                .Where(x => (x.ProcessName != null && processNames.Contains(x.ProcessName)) || processIDs.Contains(x.ProcessId))
                .Select(x => (int)x.LocalPort)
                .ToList();
        }


    }



    public class WindowsTcpProcessState : ITcpProcessState
    {
        public MibTcpState State { get; }

        public WindowsTcpProcessState(MibTcpState state)
        {
            State = state;
        }

        public string StateDescription => State.ToString();
    }


    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }



    ///// <summary>
    ///// This class provides access an IPv4 TCP connection addresses and ports and its
    ///// associated Process IDs and names.
    ///// </summary>
    //public record TcpProcessRecord(
    //    IPAddress LocalAddress,
    //    IPAddress RemoteAddress,
    //    ushort LocalPort,
    //    ushort RemotePort,
    //    int ProcessId,
    //    MibTcpState State,
    //    string? ProcessName)
    //{
    //    public override string ToString()
    //    {
    //        return $"{ProcessName}, {ProcessId}, {LocalPort}";
    //    }
    //}


    // Enum to define the set of values used to indicate the type of table returned by 
    // calls made to the function 'GetExtendedTcpTable'.
    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // Enum to define the set of values used to indicate the type of table returned by calls
    // made to the function GetExtendedUdpTable.
    public enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    // Enum for different possible states of TCP connection
    //public enum MibTcpState
    //{
    //    CLOSED = 1,
    //    LISTENING = 2,
    //    SYN_SENT = 3,
    //    SYN_RCVD = 4,
    //    ESTABLISHED = 5,
    //    FIN_WAIT1 = 6,
    //    FIN_WAIT2 = 7,
    //    CLOSE_WAIT = 8,
    //    CLOSING = 9,
    //    LAST_ACK = 10,
    //    TIME_WAIT = 11,
    //    DELETE_TCB = 12,
    //    NONE = 0
    //}

    /// <summary>
    /// The structure contains information that describes an IPv4 TCP connection with 
    /// IPv4 addresses, ports used by the TCP connection, and the specific process ID
    /// (PID) associated with connection.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public int owningPid;
    }

    /// <summary>
    /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that 
    /// are context bound to these PIDs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }



    /// <summary>
    /// The structure contains an entry from the User Datagram Protocol (UDP) listener
    /// table for IPv4 on the local computer. The entry also includes the process ID
    /// (PID) that issued the call to the bind function for the UDP endpoint.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public int owningPid;
    }

    /// <summary>
    /// The structure contains the User Datagram Protocol (UDP) listener table for IPv4
    /// on the local computer. The table also includes the process ID (PID) that issued
    /// the call to the bind function for each UDP endpoint.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
        public UdpProcessRecord[] table;
    }

    /// <summary>
    /// This class provides access an IPv4 UDP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    //[StructLayout(LayoutKind.Sequential)]
    public class UdpProcessRecord
    {
        public IPAddress LocalAddress { get; set; }
        public uint LocalPort { get; set; }
        public int ProcessId { get; set; }
        public string? ProcessName { get; set; }

        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId, string? processName = null)
        {
            LocalAddress = localAddress;
            LocalPort = localPort;
            ProcessId = pId;
            ProcessName = processName;
            //if (processName != null)
            //{
            //    ProcessName = processName;
            //}
            //else if (Process.GetProcesses().Any(process => process.Id == pId))
            //{
            //    ProcessName = Process.GetProcessById(ProcessId).ProcessName;
            //}   
        }
    }


}
