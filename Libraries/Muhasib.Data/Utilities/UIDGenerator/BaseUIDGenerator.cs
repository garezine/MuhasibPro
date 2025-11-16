namespace Muhasib.Data.Utilities.UIDGenerator
{
    public class BaseUIDGenerator
    {
        // Bit konfigürasyonu
        protected const int NODE_ID_BITS = 10; // Node ID için 10 bit
        protected const int SEQUENCE_BITS = 12; // Sıralama numarası için 12 bit

        // Maksimum değerler
        protected const long MAX_NODE_ID = (1L << NODE_ID_BITS) - 1;
        protected const long SEQUENCE_MASK = (1L << SEQUENCE_BITS) - 1;

        // Zaman ölçümü
        private static long _lastTimestamp = -1L;
        private static long _sequence = 0L;

        // Node ID (sunucu/process kimliği)
        private static readonly long _nodeId;

        static BaseUIDGenerator()
        {
            _nodeId = GetConfiguredNodeId();
            if (_nodeId < 0 || _nodeId > MAX_NODE_ID)
                throw new ArgumentException($"Node ID 0 ile {MAX_NODE_ID} arasında bir değer olmalı");
        }

        public long GenerateId(long moduleId)
        {
            lock (typeof(BaseUIDGenerator)) // Thread-safe yapmak için lock kullanıyoruz
            {
                long currentTimestamp = GetCurrentTimestamp();

                if (currentTimestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException("Zaman geriye gidemez!");
                }

                if (currentTimestamp == _lastTimestamp)
                {
                    _sequence = _sequence + 1 & SEQUENCE_MASK;

                    if (_sequence == 0)
                    {
                        // Sıralama numarası doldu, bir sonraki milisaniyeyi bekle
                        currentTimestamp = WaitNextMillis(currentTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = currentTimestamp;

                // Benzersiz ID oluştur
                return currentTimestamp << NODE_ID_BITS + SEQUENCE_BITS
                       | _nodeId << SEQUENCE_BITS
                       | _sequence
                       | moduleId << NODE_ID_BITS + SEQUENCE_BITS + 1; // Modül ID'sini en üst bite yerleştir
            }
        }

        private static long GetCurrentTimestamp()
        {
            // UNIX zaman damgası (milisaniye cinsinden)
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static long WaitNextMillis(long lastTimestamp)
        {
            long timestamp;
            do
            {
                timestamp = GetCurrentTimestamp();
            } while (timestamp <= lastTimestamp);

            return timestamp;
        }

        private static long GetConfiguredNodeId()
        {
            // Ortam değişkeninden veya varsayılan bir değer al
            string nodeIdStr = Environment.GetEnvironmentVariable("NODE_ID") ?? "1"; // Varsayılan 1
            return long.Parse(nodeIdStr);
        }
    }

}