using NAudio.Wave;

namespace KB.Jarvis.App.Services;

public sealed record AudioBufferSnapshot(
    int BufferedMilliseconds,
    int TargetMilliseconds,
    long Underruns,
    long TrimmedBytes,
    long SilenceBytes,
    bool Primed);

/// <summary>
/// A never-ending PCM wave provider designed for streamed conversational audio.
/// It keeps the Windows output device running continuously, inserts silence during
/// network gaps, adapts its prebuffer after underruns and trims only stale audio.
/// </summary>
public sealed class AdaptivePcmJitterBuffer : IWaveProvider
{
    private readonly object _gate = new();
    private readonly byte[] _ring;
    private readonly int _minimumTargetBytes;
    private readonly int _maximumTargetBytes;
    private readonly int _stableAdjustmentIntervalMilliseconds;

    private int _readIndex;
    private int _writeIndex;
    private int _count;
    private int _targetBytes;
    private int _fadeSamplesRemaining;
    private bool _primed;
    private long _underruns;
    private long _trimmedBytes;
    private long _silenceBytes;
    private long _lastUnderrunTicks;
    private long _lastStableAdjustmentTicks;

    public AdaptivePcmJitterBuffer(
        WaveFormat waveFormat,
        int capacityMilliseconds = 5000,
        int initialTargetMilliseconds = 260,
        int minimumTargetMilliseconds = 180,
        int maximumTargetMilliseconds = 650)
    {
        ArgumentNullException.ThrowIfNull(waveFormat);
        if (waveFormat.BitsPerSample != 16)
        {
            throw new ArgumentException("The adaptive Jarvis jitter buffer requires 16-bit PCM audio.", nameof(waveFormat));
        }

        WaveFormat = waveFormat;
        var capacityBytes = Align(Math.Max(
            waveFormat.AverageBytesPerSecond,
            waveFormat.AverageBytesPerSecond * capacityMilliseconds / 1000));
        _ring = new byte[capacityBytes];
        _minimumTargetBytes = MillisecondsToBytes(minimumTargetMilliseconds);
        _maximumTargetBytes = Math.Min(_ring.Length / 2, MillisecondsToBytes(maximumTargetMilliseconds));
        _targetBytes = Math.Clamp(
            MillisecondsToBytes(initialTargetMilliseconds),
            _minimumTargetBytes,
            _maximumTargetBytes);
        _stableAdjustmentIntervalMilliseconds = 15000;
        _lastUnderrunTicks = Environment.TickCount64;
        _lastStableAdjustmentTicks = Environment.TickCount64;
    }

    public WaveFormat WaveFormat { get; }

    public event Action<string>? Diagnostic;

    public void AddSamples(byte[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (offset < 0 || count < 0 || offset + count > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        count = Align(count);
        if (count <= 0) return;

        string? diagnostic = null;
        lock (_gate)
        {
            if (count >= _ring.Length)
            {
                offset += count - _ring.Length;
                count = Align(_ring.Length);
                _readIndex = 0;
                _writeIndex = 0;
                _count = 0;
                _primed = false;
            }

            var overflow = _count + count - _ring.Length;
            if (overflow > 0)
            {
                // Keep a recent continuous window rather than clearing the whole sentence.
                var preferredRemaining = MillisecondsToBytes(850);
                var requestedDrop = Math.Max(overflow, _count - preferredRemaining);
                var dropped = DropOldestUnsafe(Align(requestedDrop));
                _trimmedBytes += dropped;
                if (dropped > 0)
                {
                    _fadeSamplesRemaining = Math.Max(_fadeSamplesRemaining, WaveFormat.SampleRate / 200); // 5 ms
                    diagnostic = $"Voice backlog trimmed by {dropped / 1000d:F1} KB; newest speech continuity was preserved.";
                }
            }

            WriteUnsafe(source, offset, count);
            if (!_primed && _count >= _targetBytes)
            {
                PrimeUnsafe();
            }
        }

        if (diagnostic is not null) Diagnostic?.Invoke(diagnostic);
    }

    public int Read(byte[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (offset < 0 || count < 0 || offset + count > destination.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Array.Clear(destination, offset, count);
        string? diagnostic = null;
        var copied = 0;

        lock (_gate)
        {
            if (!_primed)
            {
                if (_count >= _targetBytes)
                {
                    PrimeUnsafe();
                }
                else
                {
                    _silenceBytes += count;
                    return count;
                }
            }

            copied = ReadUnsafe(destination, offset, Math.Min(count, _count));
            ApplyFadeUnsafe(destination, offset, copied);

            if (copied < count)
            {
                _silenceBytes += count - copied;
                _underruns++;
                _primed = false;
                _lastUnderrunTicks = Environment.TickCount64;
                var oldTarget = _targetBytes;
                _targetBytes = Math.Min(_maximumTargetBytes, _targetBytes + MillisecondsToBytes(55));
                diagnostic = oldTarget == _targetBytes
                    ? $"Voice stream underrun #{_underruns}; holding maximum {_targetBytes * 1000 / WaveFormat.AverageBytesPerSecond} ms protection window."
                    : $"Voice stream underrun #{_underruns}; adaptive prebuffer increased to {_targetBytes * 1000 / WaveFormat.AverageBytesPerSecond} ms.";
            }
            else
            {
                LowerTargetAfterStablePlaybackUnsafe();
            }
        }

        if (diagnostic is not null) Diagnostic?.Invoke(diagnostic);
        return count;
    }

    public void Reset(string? reason = null)
    {
        lock (_gate)
        {
            _readIndex = 0;
            _writeIndex = 0;
            _count = 0;
            _primed = false;
            _fadeSamplesRemaining = 0;
        }

        if (!string.IsNullOrWhiteSpace(reason)) Diagnostic?.Invoke(reason);
    }

    public AudioBufferSnapshot Snapshot()
    {
        lock (_gate)
        {
            return new AudioBufferSnapshot(
                BytesToMilliseconds(_count),
                BytesToMilliseconds(_targetBytes),
                _underruns,
                _trimmedBytes,
                _silenceBytes,
                _primed);
        }
    }

    private void PrimeUnsafe()
    {
        _primed = true;
        _fadeSamplesRemaining = Math.Max(_fadeSamplesRemaining, WaveFormat.SampleRate / 200); // 5 ms fade-in
    }

    private void LowerTargetAfterStablePlaybackUnsafe()
    {
        var now = Environment.TickCount64;
        if (now - _lastUnderrunTicks < 30000
            || now - _lastStableAdjustmentTicks < _stableAdjustmentIntervalMilliseconds
            || _targetBytes <= _minimumTargetBytes)
        {
            return;
        }

        _targetBytes = Math.Max(_minimumTargetBytes, _targetBytes - MillisecondsToBytes(20));
        _lastStableAdjustmentTicks = now;
    }

    private void ApplyFadeUnsafe(byte[] buffer, int offset, int count)
    {
        if (_fadeSamplesRemaining <= 0 || count < 2) return;

        var sampleCount = Math.Min(_fadeSamplesRemaining, count / 2);
        var originalFadeLength = Math.Max(1, WaveFormat.SampleRate / 200);
        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var byteIndex = offset + sampleIndex * 2;
            var sample = (short)(buffer[byteIndex] | buffer[byteIndex + 1] << 8);
            var completed = originalFadeLength - _fadeSamplesRemaining + sampleIndex + 1;
            var gain = Math.Clamp(completed / (double)originalFadeLength, 0d, 1d);
            var faded = (short)Math.Clamp((int)Math.Round(sample * gain), short.MinValue, short.MaxValue);
            buffer[byteIndex] = (byte)(faded & 0xFF);
            buffer[byteIndex + 1] = (byte)((faded >> 8) & 0xFF);
        }
        _fadeSamplesRemaining -= sampleCount;
    }

    private void WriteUnsafe(byte[] source, int offset, int count)
    {
        var first = Math.Min(count, _ring.Length - _writeIndex);
        Buffer.BlockCopy(source, offset, _ring, _writeIndex, first);
        var remaining = count - first;
        if (remaining > 0)
        {
            Buffer.BlockCopy(source, offset + first, _ring, 0, remaining);
        }
        _writeIndex = (_writeIndex + count) % _ring.Length;
        _count += count;
    }

    private int ReadUnsafe(byte[] destination, int offset, int count)
    {
        if (count <= 0) return 0;
        var first = Math.Min(count, _ring.Length - _readIndex);
        Buffer.BlockCopy(_ring, _readIndex, destination, offset, first);
        var remaining = count - first;
        if (remaining > 0)
        {
            Buffer.BlockCopy(_ring, 0, destination, offset + first, remaining);
        }
        _readIndex = (_readIndex + count) % _ring.Length;
        _count -= count;
        return count;
    }

    private int DropOldestUnsafe(int requested)
    {
        var dropped = Math.Min(_count, Math.Max(0, Align(requested)));
        _readIndex = (_readIndex + dropped) % _ring.Length;
        _count -= dropped;
        return dropped;
    }

    private int MillisecondsToBytes(int milliseconds) =>
        Align(Math.Max(WaveFormat.BlockAlign, WaveFormat.AverageBytesPerSecond * milliseconds / 1000));

    private int BytesToMilliseconds(int bytes) =>
        WaveFormat.AverageBytesPerSecond == 0 ? 0 : (int)Math.Round(bytes * 1000d / WaveFormat.AverageBytesPerSecond);

    private int Align(int value) => value - value % Math.Max(1, WaveFormat.BlockAlign);
}
