using SharpPcap;
using PacketDotNet;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PacketDotNet.Utils;
using Sniffer;
using PacketDotNet.Ieee80211;
using System.Windows;

public class PacketSniffer
{
    private CaptureDeviceList devices;
    private ICaptureDevice selectedDevice;
    private bool isCapturing = false;

    public delegate void OnGetAns(SniffData a);
    public OnGetAns OnGetAnsDeleGate;
    public class IPFragment
    {
        public int Offset { get; set; }         // 分片的偏移量
        public bool MoreFragments { get; set; } // 是否还有更多分片
        public byte[] Data { get; set; }       // 当前分片的数据
    }

    private readonly Dictionary<int, List<IPFragment>> fragmentCache = new();

    public PacketSniffer()
    {
        devices = CaptureDeviceList.Instance;
    }

    // 获取所有可用的网络接口
    public string[] GetAvailableInterfaces()
    {
        return devices.Select(dev => dev.Description).ToArray();
    }

    // 启动嗅探
    public void StartSniffing(string interfaceName)
    {
        selectedDevice = devices.FirstOrDefault(dev => dev.Description == interfaceName);

        if (selectedDevice == null)
        {
            Console.WriteLine($"Interface {interfaceName} not found.");
            return;
        }

        selectedDevice.OnPacketArrival += OnPacketArrival;
        selectedDevice.Open(SharpPcap.DeviceModes.Promiscuous);
        selectedDevice.StartCapture();
        isCapturing = true;
        Console.WriteLine($"Started sniffing on {interfaceName}");
    }

    // 停止嗅探
    public void StopSniffing()
    {
        if (isCapturing && selectedDevice != null)
        {
            selectedDevice.StopCapture();
            selectedDevice.Close();
            isCapturing = false;
            Console.WriteLine("Stopped sniffing.");
        }
    }

    // 处理数据包
    private void OnPacketArrival(object sender, PacketCapture e)
    {
        var rawPacket = e.GetPacket();
        var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

        if (packet is EthernetPacket ethPacket && ethPacket.PayloadPacket is IPPacket ipPacket)
        {
            // 获取源和目的 IP 地址
            string from = ipPacket.SourceAddress.ToString();
            string to = ipPacket.DestinationAddress.ToString();
            string type = "IP";

            // 获取 IP 包头信息 注意大小端
            byte[] ipHeader = ipPacket.HeaderData;
            int identification = (int)(ipHeader[4] << 8 | ipHeader[5]); // IP Identification
            //int identification = ipPacket.
            ushort flagsAndOffset = (ushort)(ipHeader[6] << 8 | ipHeader[7]);
            int a = sizeof(ushort);
            bool moreFragments = (flagsAndOffset & 0x2000) != 0; // MoreFragments 标志
            int fragmentOffset = (flagsAndOffset & 0x1FFF) * 8;  // 偏移量（以字节为单位）

            // 检查是否为分片数据包
            if (fragmentOffset > 0 || moreFragments)
            {
                HandleFragment(ipPacket, identification, fragmentOffset, moreFragments);
                return; // 暂时跳过，等待完整数据包重组
            }

            // 如果没有分片，直接处理完整数据包
            ProcessCompletePacket(ipPacket);
        }
    }


    // 递归解析嵌套协议
    private void ParseNestedPacket(Packet packet, SniffData sniffData)
    {
        if (packet == null) return;

        switch (packet)
        {
            case ArpPacket arpPacket:
                sniffData.Type = "ARP";
                sniffData.Sum = $"ARP Packet: {arpPacket.SenderProtocolAddress} -> {arpPacket.TargetProtocolAddress}";
                sniffData.Information = "No payload for ARP packets.";
                sniffData.InformationAscii = "No payload for ARP packets.";
                break;

            case IPv6Packet ipv6Packet:
                sniffData.Type = "IP (IPv6)";
                sniffData.From = ipv6Packet.SourceAddress.ToString();
                sniffData.To = ipv6Packet.DestinationAddress.ToString();
                sniffData.Sum = $"IPv6 Packet: {sniffData.From} -> {sniffData.To}, Next Header: {ipv6Packet.NextHeader}";
                ExtractPayloadData(ipv6Packet.PayloadPacket, sniffData);
                break;

            case IPPacket ipPacket:
                sniffData.Type = "IP (IPv4)";
                sniffData.From = ipPacket.SourceAddress.ToString();
                sniffData.To = ipPacket.DestinationAddress.ToString();
                sniffData.Sum = $"IPv4 Packet: {sniffData.From} -> {sniffData.To}";

                // 根据 IP 包的有效载荷类型递归解析
                ParseNestedPacket(ipPacket.PayloadPacket, sniffData);
                break;

            case TcpPacket tcpPacket:
                sniffData.Type = "TCP";
                sniffData.Sum = $"TCP Packet: {tcpPacket.SourcePort} -> {tcpPacket.DestinationPort}, Payload Length: {tcpPacket.PayloadData.Length} bytes";
                ExtractPayloadData(tcpPacket.PayloadData, sniffData);
                break;

            case UdpPacket udpPacket:
                sniffData.Type = "UDP";
                sniffData.Sum = $"UDP Packet: {udpPacket.SourcePort} -> {udpPacket.DestinationPort}, Payload Length: {udpPacket.PayloadData.Length} bytes";
                ExtractPayloadData(udpPacket.PayloadData, sniffData);
                break;

            case IcmpV4Packet icmpPacket:
                sniffData.Type = "ICMP";
                byte[] icmpBytes = icmpPacket.Bytes;
                byte type = icmpBytes[0];
                byte code = icmpBytes[1];
                sniffData.Sum = $"ICMP Packet: Type={type}, Code={code}, Payload Length: {icmpPacket.PayloadData.Length} bytes";
                ExtractPayloadData(icmpPacket.PayloadData, sniffData);
                break;

            default:
                sniffData.Type = "Unknown";
                sniffData.Sum = "Unknown Protocol";
                sniffData.Information = "No additional information.";
                sniffData.InformationAscii = "No additional information.";
                break;
        }
    }


    // 提取并转换数据包的有效载荷
    private void ExtractPayloadData(byte[] payloadData, SniffData sniffData)
    {
        if (payloadData == null || payloadData.Length == 0)
        {
            sniffData.Information = "No payload.";
            sniffData.InformationAscii = "No payload.";
            return;
        }

        // 转换为可读 ASCII 字符串
        StringBuilder asciiBuilder = new StringBuilder();
        foreach (var b in payloadData)
        {
            asciiBuilder.Append(b >= 32 && b <= 126 ? (char)b : '.');
        }

        // 转换为 ASCII 十六进制码
        string hexAscii = BitConverter.ToString(payloadData).Replace("-", " ");

        sniffData.Information = asciiBuilder.ToString();
        sniffData.InformationAscii = hexAscii;
    }

    // 提取并转换数据包的有效载荷（重载，支持 Packet 类型）
    private void ExtractPayloadData(Packet payloadPacket, SniffData sniffData)
    {
        if (payloadPacket == null)
        {
            sniffData.Information = "No payload.";
            sniffData.InformationAscii = "No payload.";
            return;
        }

        // 获取整个数据包的字节数据
        byte[] packetBytes = payloadPacket.Bytes;

        // 计算头部长度（根据协议类型推测）
        int headerLength = GetHeaderLength(payloadPacket);

        // 提取有效载荷数据
        byte[] payloadData = packetBytes.Skip(headerLength).ToArray();

        // 转换为 ASCII 格式和十六进制表示
        ExtractPayloadData(payloadData, sniffData);
    }

    // 获取头部长度的辅助方法
    private int GetHeaderLength(Packet packet)
    {
        // 根据协议类型估算头部长度
        switch (packet)
        {
            case EthernetPacket _:
                return EthernetFields.HeaderLength;
            case IPPacket ipPacket:
                return ipPacket.HeaderLength; // 使用 IP 的头部长度
            case TcpPacket tcpPacket:
                byte[] tcpHeader = tcpPacket.Bytes;
                int headerLength = (tcpHeader[12] >> 4) * 4;
                return headerLength; // 使用 TCP 的头部长度
            case UdpPacket _:
                return UdpFields.HeaderLength; // UDP 的固定头部长度
            default:
                return 0; // 如果无法确定头部长度，则认为没有头部
        }
    }

    // 辅助函数：处理数据包分片
    private void HandleFragment(IPPacket ipPacket, int identification, int fragmentOffset, bool moreFragments)
    {
        lock (fragmentCache)
        {
            // 如果是新的分片，初始化缓存
            if (!fragmentCache.ContainsKey(identification))
            {
                fragmentCache[identification] = new List<IPFragment>();
            }

            // 保存当前分片
            fragmentCache[identification].Add(new IPFragment
            {
                Offset = fragmentOffset,
                MoreFragments = moreFragments,
                Data = ipPacket.Bytes
            });

            // 如果没有更多分片，执行重组
            if (!moreFragments) // 最后一个分片
            {
                ReassembleFragments(identification);
            }
        }
    }


    // 辅助函数：重组所有分片
    // 辅助函数：重组所有分片
    private void ReassembleFragments(int identification)
    {
        if (!fragmentCache.ContainsKey(identification))
        {
            Console.WriteLine($"Fragment cache does not contain identification: {identification}");
            return;
        }

        var fragments = fragmentCache[identification];

        if (fragments == null || fragments.Count == 0)
        {
            Console.WriteLine($"No fragments available for identification: {identification}");
            return;
        }

        // 按偏移量排序
        fragments.Sort((a, b) => a.Offset.CompareTo(b.Offset));

        int totalSize = fragments.Sum(f => f.Data.Length);
        byte[] reassembledData = new byte[totalSize];

        int currentOffset = 0;
        foreach (var fragment in fragments)
        {
            if (fragment.Data == null)
            {
                Console.WriteLine($"Fragment data is null for offset: {fragment.Offset}");
                continue;
            }

            Buffer.BlockCopy(fragment.Data, 0, reassembledData, currentOffset, fragment.Data.Length);
            currentOffset += fragment.Data.Length;
        }

        // 重组完成后，删除缓存
        fragmentCache.Remove(identification);

        // 创建完整的 IP 包对象
        ByteArraySegment byteArraySegment = new ByteArraySegment(reassembledData);
        IPPacket completePacket = new IPv4Packet(byteArraySegment);
        ProcessCompletePacket(completePacket); // 调用处理完整包的逻辑
    }



    // 处理完整数据包
    private void ProcessCompletePacket(IPPacket ipPacket)
    {
        // 创建 SniffData 对象并传递回调
        SniffData sniffData = new SniffData
        {
            Type = string.Empty,
            Information = string.Empty,
            InformationAscii = string.Empty,
            From = string.Empty,
            To = string.Empty,
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // 当前时间戳
            Sum = string.Empty
        };
        ParseNestedPacket(ipPacket, sniffData);
        OnGetAnsDeleGate?.Invoke(sniffData);
    }
}

