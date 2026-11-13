shader_type canvas_item;

// An enchanted thing (scripts/Ui/Enchant.cs, 0.37): light flows over it and licks up off its edges. This is drawn as
// a layer of its own over the thing's picture, from the same picture with a few cells of empty margin round it:
// on the thing, a violet cast that breathes and two bands of light crossing it; round it, a rim of light that hugs
// the silhouette and rises off the top in tongues. Everything moves a cell of the art at a time, so the pixels stay
// pixels, and nothing of the picture underneath is changed.
uniform vec4 tint : source_color = vec4(0.66, 0.36, 1.0, 1.0);
uniform vec4 crest : source_color = vec4(0.97, 0.90, 1.0, 1.0);
// How many of the art's cells the padded picture is across and down.
uniform vec2 cells = vec2(40.0, 40.0);
// Each thing starts somewhere else in the flow, so a shelf of them doesn't pulse as one.
uniform float phase = 0.0;
uniform float strength = 1.0;

float solid(sampler2D picture, vec2 g) {
	return step(0.5, texture(picture, g).a);
}

void fragment() {
	vec2 g = (floor(UV * cells) + 0.5) / cells;
	vec2 cell = 1.0 / cells;
	float t = TIME + phase;
	float breath = 0.5 + 0.5 * sin(t * 2.1);
	vec4 here = texture(TEXTURE, UV);
	if (here.a >= 0.5) {
		// On the thing: two bands going different ways, and where they cross, the crest.
		float a = sin((g.x * cells.x * 0.24 + g.y * cells.y * 0.36) - t * 3.0);
		float b = sin((g.x * cells.x * 0.40 - g.y * cells.y * 0.16) + t * 1.9);
		float flow = smoothstep(0.30, 1.0, a * 0.6 + b * 0.4 + 0.22);
		vec3 light = mix(tint.rgb, crest.rgb, smoothstep(0.55, 1.0, flow));
		COLOR = vec4(light, clamp((0.20 + 0.10 * breath + flow * 0.62) * strength, 0.0, 0.92));
	} else {
		// Round the thing: how near its edge is (a cell, two, three away), looking further down than up, so the
		// light stands taller over its top than it hangs under it.
		float near = 0.0;
		for (int i = 0; i < 8; i++) {
			float ang = float(i) * 0.7853982;
			vec2 dir = vec2(cos(ang), sin(ang));
			near = max(near, solid(TEXTURE, g + dir * cell) * 1.0);
			near = max(near, solid(TEXTURE, g + dir * cell * 2.0) * 0.62);
			near = max(near, solid(TEXTURE, g + dir * cell * 3.0) * 0.30);
		}
		// Tongues of it rising: how far up it reaches comes and goes along the thing, and travels.
		float wave = 0.5 + 0.5 * sin(g.x * cells.x * 0.8 + t * 3.4) * sin(g.x * cells.x * 0.33 - t * 2.2);
		float reach = 2.0 + 3.0 * wave;
		for (int r = 2; r <= 5; r++) {
			float fr = float(r);
			if (fr <= reach) near = max(near, solid(TEXTURE, g + vec2(0.0, cell.y * fr)) * (1.0 - fr / 6.5));
		}
		float flick = 0.72 + 0.28 * sin(g.y * cells.y * 0.9 - t * 6.0 + g.x * cells.x * 0.45);
		vec3 light = mix(tint.rgb, crest.rgb, near * near * 0.8);
		COLOR = vec4(light, clamp(near * flick * (0.78 + 0.22 * breath) * strength, 0.0, 1.0));
	}
}
