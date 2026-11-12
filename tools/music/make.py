#!/usr/bin/env python3
"""Keywright's music and ambience (0.36), written here and rendered by synth.py:

    python3 tools/music/make.py            # every track, into assets/audio/music and assets/audio/ambience (.ogg)
    python3 tools/music/make.py night      # just those
    python3 tools/music/make.py --wav DIR  # WAVs for checking, no .ogg

A theme for the title and the crossroads, one for each market further in, the night (the day's end, Nim's table), the
Guild's night and the treasury; and beds of ambience (a day in the country, a market's murmur, the night, a great hall).
Needs numpy, and ffmpeg (libvorbis) for the .ogg files. The tunes are written out below, a bar at a time; every loop is
seamless (synth.Loop wraps what rings past its end). Scripts/Audio/Music.cs plays them.
"""
import os
import subprocess
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import synth  # noqa: E402
from synth import Loop, hz  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
STEPS = {"C": 0, "D": 2, "E": 4, "F": 5, "G": 7, "A": 9, "B": 11}


def midi(name):
    """'F#4' or 'Bb3' as a MIDI note."""
    step = STEPS[name[0]]
    rest = name[1:]
    if rest[0] == "#":
        step, rest = step + 1, rest[1:]
    elif rest[0] == "b":
        step, rest = step - 1, rest[1:]
    return 12 * (int(rest) + 1) + step


# Chords: the bass note, and the tones a lute plays around it.
CHORDS = {
    "D": ("D2", ["D3", "A3", "D4", "F#4"]), "G": ("G2", ["G3", "B3", "D4", "G4"]), "A": ("A2", ["A3", "C#4", "E4", "A4"]),
    "Bm": ("B2", ["B3", "D4", "F#4", "B4"]), "Em": ("E2", ["E3", "B3", "E4", "G4"]), "C": ("C3", ["C3", "G3", "C4", "E4"]),
    "Dm": ("D2", ["D3", "A3", "D4", "F4"]), "Bb": ("Bb2", ["Bb2", "F3", "Bb3", "D4"]), "F": ("F2", ["F3", "A3", "C4", "F4"]),
    "Gm": ("G2", ["G3", "Bb3", "D4", "G4"]), "Am": ("A2", ["A3", "C4", "E4", "A4"]), "E": ("E2", ["E3", "G#3", "B3", "E4"]),
    "A7": ("A2", ["A3", "C#4", "E4", "G4"]),
}


def tune(notes):
    """A melody as (start beat, MIDI note, beats) from (name, beats) pairs; '-' is a rest."""
    out, at = [], 0.0
    for name, beats in notes:
        if name != "-":
            out.append((at, midi(name), beats))
        at += beats
    return out, at


def crossroads():
    """The title and Bram's stall: a lilting air in D, six-eight, a whistle over a lute. A week of small chests and
    big skies."""
    beat = 60 / 112.0  # an eighth note; two dotted quarters a bar at 56
    bars = ["D", "G", "D", "A", "D", "G", "A", "D", "Bm", "G", "D", "A", "Bm", "G", "A", "D"]
    air = [
        ("F#4", 3), ("A4", 2), ("F#4", 1), ("G4", 3), ("B4", 2), ("G4", 1), ("A4", 3), ("F#4", 2), ("D4", 1), ("E4", 6),
        ("F#4", 3), ("A4", 2), ("D5", 1), ("B4", 3), ("G4", 2), ("B4", 1), ("A4", 2), ("G4", 1), ("F#4", 2), ("E4", 1), ("D4", 6),
        ("D5", 3), ("B4", 2), ("D5", 1), ("B4", 3), ("G4", 2), ("B4", 1), ("A4", 3), ("F#4", 2), ("A4", 1), ("E4", 3), ("A4", 3),
        ("D5", 2), ("C#5", 1), ("B4", 2), ("A4", 1), ("B4", 3), ("G4", 3), ("A4", 2), ("B4", 1), ("A4", 2), ("E4", 1), ("D4", 6),
    ]
    melody, length = tune(air)
    assert length == len(bars) * 6, length
    total = 2 * length
    loop = Loop(total * beat, seed=11)
    for rep in range(2):
        base = rep * length
        for b, chord in enumerate(bars):
            root, tones = CHORDS[chord]
            t0 = (base + b * 6) * beat
            loop.add(t0, synth.bass(hz(midi(root)), 2.2), gain=0.5, wet=0.1)
            # The lute: low, high, middle, high, middle, high; fuller the second time through.
            pattern = [0, 2, 1, 3, 1, 2] if rep == 0 else [0, 2, 3, 1, 3, 2]
            for i, k in enumerate(pattern):
                vel = (0.34 if i in (0, 3) else 0.24) * (1.0 if rep == 0 else 1.1)
                loop.add(t0 + i * beat, synth.pluck(hz(midi(tones[k])), 1.8, bright=0.78), pan=-0.25, gain=vel, wet=0.3)
        # The whistle has the air the first time; the second, a harp answers it an octave up, and the whistle rests
        # for the first half.
        for start, note, beats in melody:
            t = (base + start) * beat
            if rep == 0 or start >= length / 2:
                loop.add(t, synth.flute(hz(note), beats * beat * 0.96, rng=loop.rng), pan=0.2, gain=0.3, wet=0.45)
            if rep == 1:
                loop.add(t, synth.pluck(hz(note + 12), 1.6, bright=0.7, decay=1.3), pan=0.35, gain=0.2, wet=0.4)
    return loop.render(reverb_seconds=1.8, reverb_gain=0.5)


def thornwick():
    """Thornwick Market: a jig in E dorian, a whistle and a lute with a frame drum under them; the lute keeps going
    while the whistle draws breath."""
    beat = 60 / 252.0  # an eighth; two dotted quarters a bar at 84
    a_bars = ["Em", "Em", "D", "D", "Em", "Em", "D", "Em"]
    b_bars = ["G", "D", "Em", "Bm", "G", "D", "Em", "Em"]
    a_tune = [
        ("E4", 1), ("G4", 1), ("B4", 1), ("E5", 1), ("B4", 1), ("G4", 1), ("B4", 1), ("A4", 1), ("G4", 1), ("F#4", 1), ("E4", 1), ("F#4", 1),
        ("D4", 1), ("F#4", 1), ("A4", 1), ("D5", 1), ("A4", 1), ("F#4", 1), ("A4", 1), ("G4", 1), ("F#4", 1), ("E4", 1), ("D4", 1), ("E4", 1),
        ("E4", 1), ("G4", 1), ("B4", 1), ("E5", 1), ("B4", 1), ("G4", 1), ("B4", 1), ("C#5", 1), ("D5", 1), ("E5", 1), ("D5", 1), ("B4", 1),
        ("A4", 1), ("B4", 1), ("A4", 1), ("F#4", 1), ("D4", 1), ("F#4", 1), ("E4", 3), ("E4", 2), ("-", 1),
    ]
    b_tune = [
        ("G4", 1), ("B4", 1), ("D5", 1), ("D5", 1), ("B4", 1), ("G4", 1), ("F#4", 1), ("A4", 1), ("D5", 1), ("D5", 1), ("A4", 1), ("F#4", 1),
        ("E4", 1), ("G4", 1), ("B4", 1), ("B4", 1), ("G4", 1), ("E4", 1), ("F#4", 1), ("B4", 1), ("D5", 1), ("D5", 1), ("B4", 1), ("F#4", 1),
        ("G4", 1), ("B4", 1), ("D5", 1), ("E5", 1), ("D5", 1), ("B4", 1), ("A4", 1), ("F#4", 1), ("D4", 1), ("F#4", 1), ("A4", 1), ("D5", 1),
        ("B4", 1), ("G4", 1), ("E4", 1), ("A4", 1), ("F#4", 1), ("D4", 1), ("E4", 3), ("E4", 2), ("-", 1),
    ]
    a_mel, a_len = tune(a_tune)
    b_mel, b_len = tune(b_tune)
    assert a_len == 48 and b_len == 48, (a_len, b_len)
    # A (lute alone), A (whistle), B (whistle), B (lute and harp), A (whistle), B (whistle): six sections of eight bars.
    plan = [(a_bars, a_mel, None), (a_bars, a_mel, "whistle"), (b_bars, b_mel, "whistle"), (b_bars, b_mel, "harp"),
            (a_bars, a_mel, "whistle"), (b_bars, b_mel, "whistle")]
    total = len(plan) * 48
    loop = Loop(total * beat, seed=23)
    for s, (bars, mel, voice) in enumerate(plan):
        base = s * 48
        for b, chord in enumerate(bars):
            root, tones = CHORDS[chord]
            t0 = (base + b * 6) * beat
            loop.add(t0, synth.bass(hz(midi(root)), 1.4), gain=0.5, wet=0.1)
            loop.add(t0 + 3 * beat, synth.bass(hz(midi(root)) * 1.5, 1.0), gain=0.3, wet=0.1)
            for i, k in enumerate([0, 2, 3, 1, 2, 3]):
                loop.add(t0 + i * beat, synth.pluck(hz(midi(tones[k])), 1.2, bright=0.8, decay=1.4), pan=-0.3,
                         gain=0.3 if i in (0, 3) else 0.2, wet=0.25)
            # The drum on one and four, a tambourine's touch on the eighths between; it sits out the first section.
            if s > 0:
                loop.add(t0, synth.drum(), gain=0.5, wet=0.15)
                loop.add(t0 + 3 * beat, synth.drum(pitch=92.0), gain=0.32, wet=0.15)
                for i in (2, 5):
                    loop.add(t0 + i * beat, synth.tick(loop.rng), pan=0.4, gain=0.07, wet=0.2)
        for start, note, beats in mel:
            t = (base + start) * beat
            if voice == "whistle":
                loop.add(t, synth.flute(hz(note + 12), max(0.16, beats * beat * 0.9), vibrato=0.004, breath=0.03, rng=loop.rng), pan=0.25, gain=0.2, wet=0.4)
            elif voice == "harp":
                loop.add(t, synth.pluck(hz(note + 12), 1.2, bright=0.7, decay=1.5), pan=0.3, gain=0.26, wet=0.4)
    return loop.render(reverb_seconds=1.3, reverb_gain=0.4)


def crownhold():
    """Crownhold: stately, in D minor; a plucked keyboard's broken chords, strings under them, a slow tune on a flute
    and then lower, on the strings."""
    beat = 60 / 72.0
    bars = ["Dm", "Bb", "F", "C", "Dm", "Gm", "A", "Dm", "F", "C", "Dm", "Bb", "F", "C", "A7", "Dm"]
    air = [
        ("A4", 2), ("F4", 1), ("A4", 1), ("D5", 2), ("Bb4", 2), ("C5", 2), ("A4", 1), ("F4", 1), ("G4", 3), ("E4", 1),
        ("F4", 2), ("A4", 1), ("D5", 1), ("Bb4", 2), ("G4", 2), ("A4", 1), ("C#5", 1), ("E5", 1), ("C#5", 1), ("D5", 4),
        ("A4", 2), ("C5", 2), ("E5", 2), ("C5", 1), ("G4", 1), ("F4", 1), ("A4", 1), ("D5", 2), ("D5", 2), ("Bb4", 2),
        ("C5", 1), ("A4", 1), ("F4", 2), ("G4", 2), ("E4", 2), ("F4", 1), ("D4", 1), ("E4", 1), ("C#4", 1), ("D4", 4),
    ]
    melody, length = tune(air)
    assert length == len(bars) * 4, length
    loop = Loop(2 * length * beat, seed=37)
    for rep in range(2):
        base = rep * length
        for b, chord in enumerate(bars):
            root, tones = CHORDS[chord]
            t0 = (base + b * 4) * beat
            loop.add(t0, synth.bass(hz(midi(root)), 2.6), gain=0.45, wet=0.15)
            for i, k in enumerate([0, 1, 2, 3, 2, 1, 2, 3]):
                loop.add(t0 + i * beat / 2, synth.pluck(hz(midi(tones[k])), 1.3, bright=0.9, decay=1.7, harmonics=12), pan=-0.3,
                         gain=0.26 if i % 4 == 0 else 0.17, wet=0.3)
            # Strings hold the chord, low: third and fifth.
            for k in (1, 2):
                loop.add(t0, synth.strings(hz(midi(tones[k])), 4 * beat * 1.05), pan=0.1, gain=0.07, wet=0.6)
        for start, note, beats in melody:
            t = (base + start) * beat
            if rep == 0:
                loop.add(t, synth.flute(hz(note), beats * beat * 0.97, vibrato=0.005, rng=loop.rng), pan=0.2, gain=0.26, wet=0.5)
            else:
                loop.add(t, synth.strings(hz(note - 12), beats * beat * 1.02, detune=0.003), pan=0.25, gain=0.2, wet=0.55)
                if start % 4 == 0:
                    loop.add(t, synth.bell(hz(note + 12), 1.6), pan=0.4, gain=0.07, wet=0.6)
    return loop.render(reverb_seconds=2.2, reverb_gain=0.55)


def emporium():
    """The Royal Emporium, by the Gate: a slow waltz in A minor on small bells and a harp, over a low note that never
    quite stops, like something humming down the street."""
    beat = 60 / 96.0
    bars = ["Am", "Am", "F", "E", "Am", "Dm", "E", "Am", "C", "G", "F", "E", "Am", "Dm", "E", "Am"]
    air = [
        ("E5", 2), ("C5", 1), ("A4", 3), ("C5", 2), ("A4", 1), ("B4", 2), ("G#4", 1),
        ("A4", 1), ("C5", 1), ("E5", 1), ("D5", 2), ("A4", 1), ("B4", 1), ("G#4", 1), ("B4", 1), ("A4", 3),
        ("G4", 1), ("C5", 1), ("E5", 1), ("D5", 2), ("B4", 1), ("C5", 1), ("A4", 1), ("F4", 1), ("E4", 2), ("G#4", 1),
        ("A4", 1), ("B4", 1), ("C5", 1), ("D5", 1), ("E5", 1), ("F5", 1), ("E5", 2), ("B4", 1), ("A4", 3),
    ]
    melody, length = tune(air)
    assert length == len(bars) * 3, length
    loop = Loop(2 * length * beat, seed=41)
    n = loop.n
    t = np.arange(n) / synth.SR
    # The hum: A1 and its fifth, swelling slowly (whole cycles of the loop, so it joins up).
    cycles = round(loop.n / synth.SR / 7.5)
    swell = 0.6 + 0.4 * np.sin(2 * np.pi * cycles * t / (n / synth.SR))
    for f, g in ((55.0, 0.11), (82.5, 0.05), (110.0, 0.03)):
        f = round(f * n / synth.SR) / (n / synth.SR)
        loop.add(0, np.sin(2 * np.pi * f * t) * swell, gain=g, wet=0.3)
    for rep in range(2):
        base = rep * length
        for b, chord in enumerate(bars):
            root, tones = CHORDS[chord]
            t0 = (base + b * 3) * beat
            loop.add(t0, synth.bass(hz(midi(root)), 2.4), gain=0.4, wet=0.2)
            for i, k in enumerate([1, 2, 3] if rep == 0 else [2, 3, 1]):
                loop.add(t0 + i * beat, synth.pluck(hz(midi(tones[k])), 1.8, bright=0.72), pan=-0.3, gain=0.26 if i else 0.2, wet=0.4)
        for start, note, beats in melody:
            when = (base + start) * beat
            loop.add(when, synth.bell(hz(note + (12 if rep else 0)), 2.2, soft=0.8), pan=0.25, gain=0.24 if rep == 0 else 0.16, wet=0.6)
            if rep == 1:
                loop.add(when, synth.flute(hz(note), beats * beat * 0.98, vibrato=0.005, rng=loop.rng), pan=0.1, gain=0.16, wet=0.6)
    return loop.render(reverb_seconds=2.8, reverb_gain=0.65)


def night():
    """After the shutters are down: a music box over held strings, in G, with room between the notes."""
    beat = 60 / 60.0
    bars = ["G", "Em", "C", "D", "G", "Em", ("C", "D"), "G"]
    air = [("B4", 2), ("D5", 2), ("E5", 2), ("B4", 2), ("C5", 2), ("E5", 1), ("C5", 1), ("A4", 4),
           ("B4", 2), ("G4", 2), ("E4", 2), ("G4", 2), ("C5", 1), ("B4", 1), ("A4", 1), ("F#4", 1), ("G4", 4)]
    melody, length = tune(air)
    assert length == len(bars) * 4, length
    loop = Loop(2 * length * beat, seed=53)
    for rep in range(2):
        base = rep * length
        for b, chord in enumerate(bars):
            halves = chord if isinstance(chord, tuple) else (chord, chord)
            for h, name in enumerate(halves):
                root, tones = CHORDS[name]
                t0 = (base + b * 4 + h * 2) * beat
                if h == 0 or halves[0] != halves[1]:
                    loop.add(t0, synth.bass(hz(midi(root)), 3.0), gain=0.32, wet=0.25)
                for k in (0, 1, 2):
                    loop.add(t0, synth.strings(hz(midi(tones[k])), 2 * beat * 1.15), pan=-0.1 + 0.15 * k, gain=0.055, wet=0.7)
                if rep == 1:
                    for i, k in enumerate([1, 2, 3, 2]):
                        loop.add(t0 + i * beat / 2, synth.pluck(hz(midi(tones[k])), 1.6, bright=0.65), pan=-0.3, gain=0.13, wet=0.5)
        for start, note, beats in melody:
            loop.add((base + start) * beat, synth.bell(hz(note + 12), 2.6, soft=0.9), pan=0.2, gain=0.22, wet=0.6)
            if rep == 1 and beats >= 2:
                loop.add((base + start + 1) * beat, synth.bell(hz(note + 19), 2.0, soft=1.1), pan=0.4, gain=0.07, wet=0.7)
    return loop.render(reverb_seconds=2.6, reverb_gain=0.6)


def guild():
    """Mister Vane's night: low strings marking time in D minor, a bell that tolls every other bar, and two high notes
    a half-step apart that won't settle."""
    beat = 60 / 76.0
    bars = ["Dm", "Dm", "Bb", "A", "Dm", "Gm", "A", "Dm"]
    loop = Loop(2 * len(bars) * 4 * beat, seed=67)
    for rep in range(2):
        for b, chord in enumerate(bars):
            root, tones = CHORDS[chord]
            t0 = ((rep * len(bars) + b) * 4) * beat
            # Eighth notes on the root, short, the first of each pair heavier.
            for i in range(8):
                loop.add(t0 + i * beat / 2, synth.strings(hz(midi(root)) * 2, beat * 0.42, detune=0.002) * synth.env(int(beat * 0.42 * synth.SR), 0.02, 0.12),
                         pan=-0.15, gain=0.2 if i % 2 == 0 else 0.12, wet=0.35)
            for k in (1, 2):
                loop.add(t0, synth.strings(hz(midi(tones[k])), 4 * beat * 1.05), pan=0.2, gain=0.06, wet=0.6)
            if b % 2 == 0:
                loop.add(t0, synth.bell(hz(midi(root)) * 2, 3.5, soft=0.5), gain=0.2, wet=0.7)
            # The two notes: A and B flat above, answering each other across the bars (and C sharp against the A chord).
            high = {"A": ("E5", "C#5")}.get(chord, ("A5", "Bb5") if b % 4 < 2 else ("F5", "E5"))
            if rep == 1 or b >= 2:
                loop.add(t0 + 1 * beat, synth.bell(hz(midi(high[0])), 2.4, soft=0.7), pan=0.35, gain=0.1, wet=0.7)
                loop.add(t0 + 2.5 * beat, synth.bell(hz(midi(high[1])), 2.4, soft=0.7), pan=0.45, gain=0.08, wet=0.7)
    return loop.render(reverb_seconds=2.4, reverb_gain=0.6)


def treasury():
    """The treasury: no tune. A low note that swells and sinks as if it were breathing, and now and then something
    small and bright, a long way off."""
    seconds = 36.0
    loop = Loop(seconds, seed=71)
    n = loop.n
    t = np.arange(n) / synth.SR
    breath = 0.5 + 0.5 * np.sin(2 * np.pi * 8 * t / seconds - np.pi / 2)  # eight breaths a loop: four and a half seconds each
    breath = breath ** 1.6
    for f, g in ((36.7, 0.3), (55.0, 0.22), (73.4, 0.12), (110.2, 0.05)):
        f = round(f * seconds) / seconds
        loop.add(0, np.sin(2 * np.pi * f * t) * (0.25 + 0.75 * breath), gain=g, wet=0.4)
    air = synth.noise_band(loop.rng, n, 120, 600) * (0.1 + 0.9 * breath)
    edge = int(0.5 * synth.SR)
    air[:edge] *= np.linspace(0, 1, edge)
    air[-edge:] *= np.linspace(1, 0, edge)
    loop.add(0, air, gain=0.035, wet=0.5)
    for when, note in ((5.0, "A5"), (13.5, "Bb5"), (22.0, "E6"), (30.5, "A5")):
        loop.add(when, synth.bell(hz(midi(note)), 3.0, soft=0.6), pan=0.5, gain=0.05, wet=0.9)
    return loop.render(reverb_seconds=3.2, reverb_gain=0.7, level=0.42)


# ---- Ambience ----

def _faded(x, seconds=1.0):
    """A bed of noise made to join its own start: its last second faded into its first."""
    k = int(seconds * synth.SR)
    head, tail = x[:k].copy(), x[-k:].copy()
    ramp = np.linspace(0, 1, k)
    out = x[:-k].copy()
    out[:k] = head * ramp + tail * (1 - ramp)
    return out


def _birds(loop, seconds, count, low=2600.0, high=5200.0):
    for _ in range(count):
        when = loop.rng.uniform(0, seconds)
        base = loop.rng.uniform(low, high)
        pan = loop.rng.uniform(-0.8, 0.8)
        for chirp in range(loop.rng.integers(2, 6)):
            dur = loop.rng.uniform(0.05, 0.11)
            k = int(dur * synth.SR)
            tt = np.arange(k) / synth.SR
            sweep = base * (1 + loop.rng.uniform(-0.18, 0.28) * tt / dur)
            tone = np.sin(2 * np.pi * np.cumsum(sweep) / synth.SR) * np.sin(np.pi * tt / dur) ** 2
            loop.add(when + chirp * loop.rng.uniform(0.09, 0.16), tone, pan=pan, gain=loop.rng.uniform(0.02, 0.05), wet=0.8)


def _wind(loop, seconds, gain, low=180.0, high=900.0, seed_shift=0):
    n = loop.n
    t = np.arange(n) / synth.SR
    raw = synth.noise_band(loop.rng, n + synth.SR, low, high)
    bed = _faded(raw)
    gust = 0.55 + 0.45 * np.sin(2 * np.pi * 2 * t / seconds + seed_shift) * np.sin(2 * np.pi * 3 * t / seconds + 1.3)
    loop.add(0, bed[:n] * gust, pan=-0.2, gain=gain, wet=0.2)
    raw = synth.noise_band(loop.rng, n + synth.SR, low * 1.3, high * 1.4)
    loop.add(0, _faded(raw)[:n] * (1.1 - gust), pan=0.3, gain=gain * 0.7, wet=0.2)


def _murmur(loop, seconds, voices, gain):
    """A crowd a little way off: many short breaths of noise shaped like vowels, never a word."""
    for _ in range(voices):
        when = loop.rng.uniform(0, seconds)
        pan = loop.rng.uniform(-0.9, 0.9)
        pitch = loop.rng.uniform(95, 210)
        f1, f2 = loop.rng.uniform(380, 780), loop.rng.uniform(1000, 1900)
        for syll in range(loop.rng.integers(2, 7)):
            dur = loop.rng.uniform(0.12, 0.3)
            k = int(dur * synth.SR)
            tt = np.arange(k) / synth.SR
            # A buzz at the voice's pitch through two resonances.
            buzz = np.zeros(k)
            for h in range(1, 24):
                fh = pitch * h * (1 + 0.03 * np.sin(2 * np.pi * 4 * tt))
                amp = 1 / (1 + ((fh.mean() - f1) / 160) ** 2) + 0.5 / (1 + ((fh.mean() - f2) / 240) ** 2)
                buzz += amp / h ** 0.5 * np.sin(2 * np.pi * np.cumsum(fh) / synth.SR + h)
            buzz *= np.sin(np.pi * tt / dur) ** 1.5
            loop.add(when + syll * loop.rng.uniform(0.16, 0.34), buzz, pan=pan, gain=gain * loop.rng.uniform(0.5, 1.0), wet=0.9)
            f1 *= loop.rng.uniform(0.85, 1.18)
            f2 *= loop.rng.uniform(0.9, 1.12)


def amb_day():
    """The Brindle crossroads: wind in the barley, birds."""
    seconds = 40.0
    loop = Loop(seconds, seed=101)
    _wind(loop, seconds, 0.05, low=160, high=650)
    birds = Loop(seconds, seed=102)
    _birds(birds, seconds, 26)
    wind = loop.render(reverb_seconds=1.2, reverb_gain=0.3, level=0.22, dark=1200.0, ceiling=1300.0)
    return wind + birds.render(reverb_seconds=1.2, reverb_gain=0.5, level=0.12)


def amb_market():
    """Thornwick and Crownhold: a market's murmur, a few birds over the roofs."""
    seconds = 40.0
    loop = Loop(seconds, seed=103)
    _wind(loop, seconds, 0.02, low=250, high=700)
    _murmur(loop, seconds, 150, 0.012)
    birds = Loop(seconds, seed=104)
    _birds(birds, seconds, 8, low=3000, high=4600)
    crowd = loop.render(reverb_seconds=1.8, reverb_gain=1.4, level=0.3, dark=900.0, ceiling=1500.0)
    return crowd + birds.render(reverb_seconds=1.2, reverb_gain=0.5, level=0.06)


def amb_hall():
    """The Royal Emporium: a hall's hush, voices kept low, and the Gate down the street."""
    seconds = 40.0
    loop = Loop(seconds, seed=107)
    _wind(loop, seconds, 0.015, low=120, high=420)
    _murmur(loop, seconds, 70, 0.009)
    n = loop.n
    t = np.arange(n) / synth.SR
    for f, g in ((55.0, 0.05), (110.0, 0.015)):
        f = round(f * seconds) / seconds
        loop.add(0, np.sin(2 * np.pi * f * t) * (0.7 + 0.3 * np.sin(2 * np.pi * 5 * t / seconds)), gain=g, wet=0.5)
    return loop.render(reverb_seconds=3.0, reverb_gain=1.6, level=0.3, dark=700.0, ceiling=1100.0)


def amb_night():
    """Night: crickets, and an owl twice."""
    seconds = 40.0
    loop = Loop(seconds, seed=109)
    _wind(loop, seconds, 0.02, low=150, high=500)
    for voice in range(5):
        f = loop.rng.uniform(3900, 4700)
        pan = loop.rng.uniform(-0.9, 0.9)
        rate = loop.rng.uniform(0.42, 0.6)
        when = loop.rng.uniform(0, 1)
        while when < seconds:
            if loop.rng.random() < 0.8:
                for pulse in range(3):
                    k = int(0.022 * synth.SR)
                    tt = np.arange(k) / synth.SR
                    loop.add(when + pulse * 0.034, np.sin(2 * np.pi * f * tt) * np.sin(np.pi * tt / 0.022) ** 2, pan=pan,
                             gain=0.018 * loop.rng.uniform(0.6, 1.0), wet=0.5)
            when += rate
    for when in (9.0, 27.5):
        for i, (f, d) in enumerate(((392.0, 0.35), (349.0, 0.6))):
            k = int(d * synth.SR)
            tt = np.arange(k) / synth.SR
            hoot = (np.sin(2 * np.pi * f * tt) + 0.2 * np.sin(4 * np.pi * f * tt)) * np.sin(np.pi * tt / d) ** 1.5
            loop.add(when + i * 0.5, hoot, pan=-0.6, gain=0.03, wet=1.2)
    return loop.render(reverb_seconds=2.0, reverb_gain=0.9, level=0.3)


TRACKS = {
    "music/crossroads": crossroads, "music/thornwick": thornwick, "music/crownhold": crownhold, "music/emporium": emporium,
    "music/night": night, "music/guild": guild, "music/treasury": treasury,
    "ambience/day": amb_day, "ambience/market": amb_market, "ambience/hall": amb_hall, "ambience/night": amb_night,
}


def report(name, audio):
    mono = audio.mean(axis=0)
    spec = np.abs(np.fft.rfft(mono))
    freqs = np.fft.rfftfreq(len(mono), 1 / synth.SR)
    centroid = float((spec * freqs).sum() / spec.sum())
    rms = float(np.sqrt(np.mean(mono ** 2)))
    seam = float(np.abs(audio[:, 0] - audio[:, -1]).max())
    step = float(np.percentile(np.abs(np.diff(mono)), 99))
    print(f"{name:20s} {audio.shape[1] / synth.SR:6.1f}s  peak {np.abs(audio).max():.2f}  rms {20 * np.log10(rms):6.1f} dB  "
          f"centre {centroid:5.0f} Hz  seam {seam:.4f} (a usual step {step:.4f})")


def main():
    args = sys.argv[1:]
    wav_dir = None
    if "--wav" in args:
        i = args.index("--wav")
        wav_dir = args[i + 1]
        del args[i:i + 2]
        os.makedirs(wav_dir, exist_ok=True)
    wanted = [k for k in TRACKS if not args or k.split("/")[1] in args]
    for key in wanted:
        audio = TRACKS[key]()
        report(key, audio)
        if wav_dir:
            synth.write_wav(os.path.join(wav_dir, key.replace("/", "_") + ".wav"), audio)
            continue
        out = os.path.join(ROOT, "assets", "audio", key + ".ogg")
        os.makedirs(os.path.dirname(out), exist_ok=True)
        tmp = out[:-4] + ".tmp.wav"
        synth.write_wav(tmp, audio)
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "2", out], check=True)
        os.remove(tmp)


if __name__ == "__main__":
    main()
