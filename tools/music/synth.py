"""Sound for Keywright's music and ambience (0.36): a small additive synthesiser in numpy.

Everything is rendered into a loop of a fixed length, and everything wraps: a note that rings past the end comes back
in at the start, and the reverb is a circular convolution, so a track loops without a seam. Instruments are kept soft
(few harmonics, slow attacks where a breath or a bow would have one), since this plays for hours under a shop.
"""
import numpy as np

SR = 32000


def hz(midi):
    return 440.0 * 2 ** ((midi - 69) / 12)


class Loop:
    """A stereo loop of a fixed length that sounds are added into, wrapping at the end."""

    def __init__(self, seconds, seed=1):
        self.n = int(round(seconds * SR))
        self.dry = np.zeros((2, self.n))
        self.wet = np.zeros((2, self.n))
        self.rng = np.random.default_rng(seed)

    def add(self, start, mono, pan=0.0, gain=1.0, wet=0.25):
        """Adds a mono sound at a time (seconds), panned -1..1, with this much of it sent to the reverb."""
        i = int(round(start * SR)) % self.n
        left, right = np.cos((pan + 1) * np.pi / 4), np.sin((pan + 1) * np.pi / 4)
        idx = (i + np.arange(len(mono))) % self.n
        for ch, g in ((0, left), (1, right)):
            np.add.at(self.dry[ch], idx, mono * gain * g)
            np.add.at(self.wet[ch], idx, mono * gain * g * wet)

    def render(self, reverb_seconds=1.6, reverb_gain=0.5, level=0.5, dark=2600.0, ceiling=None):
        """The loop with its reverb (a circular convolution with a decaying noise tail, darker as it fades).
        dark: where the tail's highs start to fall away; ceiling: a low-pass over the whole of it (for ambience)."""
        out = self.dry.copy()
        if reverb_gain > 0:
            m = min(self.n, int(reverb_seconds * 3 * SR))
            t = np.arange(m) / SR
            for ch in range(2):
                ir = self.rng.standard_normal(m) * np.exp(-t * 6.9 / reverb_seconds)
                ir[: int(0.012 * SR)] = 0  # a little pre-delay
                # Darken the tail: a one-pole low-pass by FFT.
                spec = np.fft.rfft(ir)
                f = np.fft.rfftfreq(m, 1 / SR)
                spec *= 1 / np.sqrt(1 + (f / dark) ** 2)
                ir = np.fft.irfft(spec, m)
                ir /= np.sqrt(np.sum(ir ** 2))
                pad = np.zeros(self.n)
                pad[:m] = ir
                out[ch] += reverb_gain * np.fft.irfft(np.fft.rfft(self.wet[ch]) * np.fft.rfft(pad), self.n)
        if ceiling:
            f = np.fft.rfftfreq(self.n, 1 / SR)
            for ch in range(2):
                out[ch] = np.fft.irfft(np.fft.rfft(out[ch]) / (1 + (f / ceiling) ** 4), self.n)
        peak = np.max(np.abs(out))
        return out * (level / peak) if peak > 0 else out


def env(n, attack, release, curve=1.0):
    """A gain curve: up over the attack, held, down over the release (seconds)."""
    e = np.ones(n)
    a, r = max(1, int(attack * SR)), max(1, int(release * SR))
    a, r = min(a, n), min(r, n)
    e[:a] *= np.linspace(0, 1, a) ** curve
    e[-r:] *= np.linspace(1, 0, r) ** curve
    return e


def pluck(freq, dur, bright=1.0, decay=1.0, harmonics=10):
    """A plucked string (lute, harp): harmonics that die away, the high ones first."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for k in range(1, harmonics + 1):
        if freq * k > SR * 0.42:
            break
        amp = (1.0 / k) * np.abs(np.sin(np.pi * k * 0.23)) * bright ** (k - 1)
        rate = decay * (1.6 + 0.55 * k * k * (freq / 220.0) ** 0.5)
        out += amp * np.exp(-t * rate) * np.sin(2 * np.pi * freq * k * t * (1 + 0.0004 * k * k))
    out *= env(n, 0.003, min(0.08, dur * 0.3))
    return out


def flute(freq, dur, vibrato=0.006, breath=0.04, rng=None):
    """A whistle or a recorder: nearly a sine, a slow start, a waver that grows, a little breath."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    depth = vibrato * np.clip((t - 0.18) / 0.35, 0, 1)
    phase = 2 * np.pi * freq * np.cumsum(1 + depth * np.sin(2 * np.pi * 5.1 * t)) / SR
    out = np.sin(phase) + 0.22 * np.sin(2 * phase) + 0.07 * np.sin(3 * phase)
    if breath > 0 and rng is not None:
        noise = rng.standard_normal(n)
        spec = np.fft.rfft(noise)
        f = np.fft.rfftfreq(n, 1 / SR)
        spec *= np.exp(-((f - freq * 2.2) / (freq * 0.9)) ** 2)
        out += breath * 6 * np.fft.irfft(spec, n) / max(1e-9, np.std(np.fft.irfft(spec, n))) * 0.15
    return out * env(n, 0.07, min(0.16, dur * 0.4), 0.7)


def strings(freq, dur, detune=0.004):
    """Bowed strings, far off: a few detuned voices, a slow bow."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for d in (-detune, 0.0, detune):
        f = freq * (1 + d)
        for k, a in ((1, 1.0), (2, 0.45), (3, 0.25), (4, 0.12), (5, 0.06)):
            if f * k < SR * 0.42:
                out += a * np.sin(2 * np.pi * f * k * t + k * (d * 500 + 0.3))
    return out / 3 * env(n, min(0.5, dur * 0.35), min(0.7, dur * 0.4), 0.8)


def bell(freq, dur, soft=1.0):
    """A small bell or a music-box tine: a few partials that don't quite agree, dying quickly."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for ratio, amp, rate in ((1.0, 1.0, 2.6), (2.0, 0.28, 4.2), (3.01, 0.14, 6.5), (4.2, 0.05, 10.0)):
        if freq * ratio < SR * 0.42:
            out += amp * np.exp(-t * rate * soft) * np.sin(2 * np.pi * freq * ratio * t)
    return out * env(n, 0.002, min(0.1, dur * 0.3))


def bass(freq, dur):
    """A soft plucked bass: the note and its octave, round."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    out = np.exp(-t * 2.2) * (np.sin(2 * np.pi * freq * t) + 0.3 * np.sin(4 * np.pi * freq * t) + 0.08 * np.sin(6 * np.pi * freq * t))
    return out * env(n, 0.006, min(0.12, dur * 0.3))


def drum(dur=0.35, pitch=78.0):
    """A frame drum, played with the fingers."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    return np.exp(-t * 16) * np.sin(2 * np.pi * (pitch * t + 14 * (1 - np.exp(-t * 30)) / 30)) * env(n, 0.002, 0.05)


def tick(rng, dur=0.07, centre=5200.0):
    """A tambourine's touch: a short breath of high noise."""
    n = int(dur * SR)
    noise = rng.standard_normal(n)
    spec = np.fft.rfft(noise)
    f = np.fft.rfftfreq(n, 1 / SR)
    spec *= np.exp(-((f - centre) / 1800.0) ** 2)
    out = np.fft.irfft(spec, n)
    return out / max(1e-9, np.max(np.abs(out))) * np.exp(-np.arange(n) / SR * 55)


def noise_band(rng, n, low, high):
    """Noise between two frequencies (for wind, a murmur, a hum's air)."""
    spec = np.fft.rfft(rng.standard_normal(n))
    f = np.fft.rfftfreq(n, 1 / SR)
    spec *= (1 / (1 + (low / np.maximum(f, 1e-6)) ** 4)) * (1 / (1 + (f / high) ** 4))
    out = np.fft.irfft(spec, n)
    return out / max(1e-9, np.std(out))


def write_wav(path, stereo):
    import wave
    data = np.clip(stereo.T, -1, 1)
    pcm = (data * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(pcm.tobytes())
