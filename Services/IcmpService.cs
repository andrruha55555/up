using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AdminUP.Services
{
    /// <summary>
    /// Проверка доступности сетевых устройств через ICMP Echo Request.
    /// Основной режим — RAW-сокет (уровень протокола ICMP, ниже команды ping).
    /// Fallback — System.Net.NetworkInformation.Ping (когда нет прав на RAW-сокет).
    /// </summary>
    public class IcmpService
    {
        /// <summary>Таймаут ожидания ответа (мс)</summary>
        private const int TimeoutMs = 2000;

        /// <summary>ICMP Type: Echo Request</summary>
        private const byte TypeEchoRequest = 8;

        /// <summary>ICMP Type: Echo Reply</summary>
        private const byte TypeEchoReply = 0;

        /// <summary>
        /// Проверяет хост по IP через RAW ICMP-сокет.
        /// При отсутствии прав администратора автоматически переключается
        /// на System.Net.NetworkInformation.Ping (тоже ICMP, но через WinAPI).
        /// </summary>
        /// <param name="ipAddress">IP-адрес устройства</param>
        /// <param name="ct">Токен отмены</param>
        public async Task<IcmpResult> CheckHostAsync(string ipAddress, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return Fail(ipAddress, "IP не указан");

            if (!IPAddress.TryParse(ipAddress.Trim(), out var ip))
                return Fail(ipAddress, "Некорректный IP-адрес");

            var rawResult = await Task.Run(() => RawIcmpEcho(ip, ipAddress, ct), ct);

            if (rawResult.FallbackNeeded)
                return await PingFallbackAsync(ipAddress, ip, ct);

            return rawResult;
        }

        // ─── RAW ICMP ────────────────────────────────────────────────────────

        /// <summary>
        /// Формирует и отправляет ICMP Echo Request через RAW-сокет,
        /// ждёт Echo Reply и проверяет идентификатор пакета.
        /// </summary>
        private IcmpResult RawIcmpEcho(IPAddress ip, string originalIp, CancellationToken ct)
        {
            Socket? sock = null;
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                sock.ReceiveTimeout = TimeoutMs;
                sock.SendTimeout = TimeoutMs;

                var remote = new IPEndPoint(ip, 0);

                ushort packetId = (ushort)(Environment.CurrentManagedThreadId & 0xFFFF);
                ushort packetSeq = (ushort)(Environment.TickCount & 0xFFFF);
                byte[] request = BuildPacket(packetId, packetSeq);

                var t0 = DateTime.UtcNow;
                sock.SendTo(request, remote);

                byte[] buf = new byte[256];
                EndPoint replyEp = new IPEndPoint(IPAddress.Any, 0);

                while (!ct.IsCancellationRequested)
                {
                    int received;
                    try { received = sock.ReceiveFrom(buf, ref replyEp); }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    { return Fail(originalIp, "Таймаут"); }

                    var rtt = (int)(DateTime.UtcNow - t0).TotalMilliseconds;

                    // IP-заголовок: длина = (первый байт & 0x0F) * 4
                    int ipHeaderLen = (buf[0] & 0x0F) * 4;
                    if (received < ipHeaderLen + 8) continue;

                    byte icmpType = buf[ipHeaderLen];
                    ushort replyId = (ushort)((buf[ipHeaderLen + 4] << 8) | buf[ipHeaderLen + 5]);

                    if (icmpType == TypeEchoReply && replyId == packetId)
                        return new IcmpResult { IpAddress = originalIp, IsAlive = true, RoundTripMs = rtt };

                    if (rtt >= TimeoutMs)
                        return Fail(originalIp, "Таймаут");
                }

                return Fail(originalIp, "Отменено");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AccessDenied)
            {
                return new IcmpResult { IpAddress = originalIp, IsAlive = false, FallbackNeeded = true };
            }
            catch (SocketException ex) when (
                ex.SocketErrorCode == SocketError.HostUnreachable ||
                ex.SocketErrorCode == SocketError.NetworkUnreachable)
            {
                return Fail(originalIp, "Хост недоступен");
            }
            catch (OperationCanceledException)
            {
                return Fail(originalIp, "Отменено");
            }
            catch (Exception ex)
            {
                return Fail(originalIp, ex.Message);
            }
            finally
            {
                sock?.Close();
            }
        }

        // ─── Fallback через Ping WinAPI (тоже ICMP) ──────────────────────────

        /// <summary>
        /// Fallback: System.Net.NetworkInformation.Ping использует IcmpSendEcho2 WinAPI —
        /// тот же ICMP-протокол, но без необходимости прав администратора.
        /// </summary>
        private async Task<IcmpResult> PingFallbackAsync(string originalIp, IPAddress ip, CancellationToken ct)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, TimeoutMs);

                if (reply.Status == IPStatus.Success)
                    return new IcmpResult
                    {
                        IpAddress = originalIp,
                        IsAlive = true,
                        RoundTripMs = (int)reply.RoundtripTime,
                        UsedFallback = true
                    };

                string reason = reply.Status switch
                {
                    IPStatus.TimedOut => "Таймаут",
                    IPStatus.DestinationUnreachable or
                    IPStatus.DestinationHostUnreachable or
                    IPStatus.DestinationNetworkUnreachable => "Хост недоступен",
                    IPStatus.TtlExpired => "TTL истёк",
                    _ => reply.Status.ToString()
                };

                return Fail(originalIp, reason, fallback: true);
            }
            catch (OperationCanceledException)
            {
                return Fail(originalIp, "Отменено", fallback: true);
            }
            catch (Exception ex)
            {
                return Fail(originalIp, ex.Message, fallback: true);
            }
        }

        // ─── Построение ICMP-пакета ───────────────────────────────────────────

        /// <summary>
        /// Строит ICMP Echo Request пакет (RFC 792).
        /// Структура: Type(1) + Code(1) + Checksum(2) + ID(2) + Seq(2) + Payload(32).
        /// </summary>
        private static byte[] BuildPacket(ushort id, ushort seq)
        {
            byte[] pkt = new byte[40];

            pkt[0] = TypeEchoRequest;
            pkt[1] = 0;
            pkt[2] = pkt[3] = 0; // checksum — заполним ниже

            pkt[4] = (byte)(id >> 8);
            pkt[5] = (byte)(id & 0xFF);

            pkt[6] = (byte)(seq >> 8);
            pkt[7] = (byte)(seq & 0xFF);

            // Payload: буквы A-Z по кругу
            for (int i = 8; i < pkt.Length; i++)
                pkt[i] = (byte)('A' + (i - 8) % 26);

            ushort cs = Checksum(pkt);
            pkt[2] = (byte)(cs >> 8);
            pkt[3] = (byte)(cs & 0xFF);

            return pkt;
        }

        /// <summary>
        /// Вычисляет контрольную сумму ICMP/IP (RFC 792).
        /// One's complement sum 16-bit words с добавлением переносов.
        /// </summary>
        private static ushort Checksum(byte[] data)
        {
            uint sum = 0;
            int i = 0;

            while (i < data.Length - 1)
            {
                sum += (uint)((data[i] << 8) | data[i + 1]);
                i += 2;
            }

            if (i < data.Length)
                sum += (uint)(data[i] << 8);

            while ((sum >> 16) != 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            return (ushort)~sum;
        }

        private static IcmpResult Fail(string ip, string error, bool fallback = false)
            => new() { IpAddress = ip, IsAlive = false, Error = error, UsedFallback = fallback };
    }

    /// <summary>Результат ICMP-проверки одного устройства.</summary>
    public class IcmpResult
    {
        /// <summary>IP-адрес проверяемого устройства</summary>
        public string IpAddress { get; set; } = "";

        /// <summary>Устройство ответило на Echo Request</summary>
        public bool IsAlive { get; set; }

        /// <summary>Round-trip time в миллисекундах</summary>
        public int RoundTripMs { get; set; }

        /// <summary>Текст ошибки (если недоступен)</summary>
        public string? Error { get; set; }

        /// <summary>Флаг: RAW-сокет недоступен, нужен Ping WinAPI fallback</summary>
        internal bool FallbackNeeded { get; set; }

        /// <summary>True — результат через Ping WinAPI (нет прав на RAW-сокет)</summary>
        public bool UsedFallback { get; set; }

        /// <summary>Строка для отображения в колонке «Статус в сети»</summary>
        public string StatusText
        {
            get
            {
                if (IsAlive)
                {
                    string method = UsedFallback ? " [ICMP/WinAPI]" : " [RAW ICMP]";
                    return $"✅ Доступен  {RoundTripMs} мс{method}";
                }

                return string.IsNullOrWhiteSpace(Error)
                    ? "❌ Недоступен"
                    : $"❌ {Error}";
            }
        }
    }
}
